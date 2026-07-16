import { useCallback, useEffect, useState } from 'react'

const CATEGORIES = ['geography', 'flags', 'football']
const CATEGORY_LABELS = { geography: 'géographie', flags: 'drapeaux', football: 'football' }

// Simplified knowledge-tracing update rule: a correct answer nudges mastery toward 1,
// a wrong answer decays it toward 0. No training, no external model — just an explicit
// adaptive formula, which is what actually implements "learns from the player's answers".
const LEARN_RATE = 0.35
const FORGET_RATE = 0.15

function clamp(value) {
  return Math.min(0.98, Math.max(0.05, value))
}

function defaultState() {
  return {
    masteries: { geography: 0.5, flags: 0.5, football: 0.5 },
    history: []
  }
}

function storageKey(userId) {
  return `fatw_chat_progress_${userId}`
}

function loadState(userId) {
  const raw = localStorage.getItem(storageKey(userId))
  if (!raw) return defaultState()
  try {
    const parsed = JSON.parse(raw)
    return { ...defaultState(), ...parsed }
  } catch {
    return defaultState()
  }
}

/** Per-user (localStorage-backed) adaptive mastery tracker for the chatbot's quiz feature. */
export function useChatProgress(userId) {
  const [state, setState] = useState(() => loadState(userId))

  useEffect(() => {
    setState(loadState(userId))
  }, [userId])

  useEffect(() => {
    localStorage.setItem(storageKey(userId), JSON.stringify(state))
  }, [userId, state])

  const recordAnswer = useCallback((category, correct) => {
    setState((prev) => {
      const current = prev.masteries[category] ?? 0.5
      const next = clamp(correct ? current + (1 - current) * LEARN_RATE : current - current * FORGET_RATE)
      return {
        masteries: { ...prev.masteries, [category]: next },
        history: [...prev.history, { category, correct, at: Date.now() }]
      }
    })
  }, [])

  const weakestCategory = useCallback(() => {
    return CATEGORIES.reduce((weakest, category) =>
      state.masteries[category] < state.masteries[weakest] ? category : weakest,
      CATEGORIES[0]
    )
  }, [state.masteries])

  const overallMastery = CATEGORIES.reduce((sum, c) => sum + state.masteries[c], 0) / CATEGORIES.length
  const level = overallMastery < 0.34 ? 'Débutant' : overallMastery < 0.67 ? 'Intermédiaire' : 'Avancé'
  const totalAnswered = state.history.length
  const totalCorrect = state.history.filter((h) => h.correct).length

  return {
    masteries: state.masteries,
    categoryLabels: CATEGORY_LABELS,
    overallMastery,
    level,
    totalAnswered,
    totalCorrect,
    weakestCategory,
    recordAnswer
  }
}
