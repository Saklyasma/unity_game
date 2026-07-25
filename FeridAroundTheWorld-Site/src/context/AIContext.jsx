import { createContext, useCallback, useContext, useEffect, useState } from 'react'
import { useAuth } from './AuthContext.jsx'
import { aiTutorApi } from '../api/aiTutor.js'

const AIContext = createContext(null)

const THEME_KEY = 'fatw_ai_tutor_theme'

export function AIProvider({ children }) {
  const { user, isAuthenticated } = useAuth()

  const [profile, setProfile] = useState(null)
  const [messages, setMessages] = useState([])
  const [isOpen, setIsOpen] = useState(false)
  const [isSending, setIsSending] = useState(false)
  const [isLoadingProfile, setIsLoadingProfile] = useState(false)
  const [error, setError] = useState(null)
  const [unreadCount, setUnreadCount] = useState(0)
  const [darkMode, setDarkMode] = useState(() => localStorage.getItem(THEME_KEY) === 'dark')

  const refreshProfile = useCallback(async () => {
    if (!user) return
    setIsLoadingProfile(true)
    setError(null)
    try {
      const data = await aiTutorApi.getOrCreateProfile(user.id, user.name)
      setProfile(data)
    } catch (err) {
      setError(err.message)
    } finally {
      setIsLoadingProfile(false)
    }
  }, [user])

  const loadHistory = useCallback(async () => {
    if (!user) return
    try {
      const history = await aiTutorApi.getChatHistory(user.id)
      if (history.length === 0) {
        setMessages([{
          id: 'greeting',
          role: 'model',
          text: `Hi ${user.name.split(' ')[0]}! I'm your AI football tutor. Ask me anything, or pick a suggestion below.`
        }])
      } else {
        setMessages(history.map((m) => ({ id: `${m.createdAtUtc}-${m.role}`, role: m.role, text: m.content })))
      }
    } catch {
      // Non-fatal: chat still works without prior history.
    }
  }, [user])

  useEffect(() => {
    if (isAuthenticated) {
      refreshProfile()
      loadHistory()
    } else {
      setProfile(null)
      setMessages([])
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [isAuthenticated, user?.id])

  useEffect(() => {
    localStorage.setItem(THEME_KEY, darkMode ? 'dark' : 'light')
  }, [darkMode])

  const sendMessage = useCallback(async (text) => {
    if (!user || !text.trim()) return
    const userBubble = { id: `${Date.now()}-user`, role: 'user', text }
    setMessages((prev) => [...prev, userBubble])
    setIsSending(true)
    setError(null)
    try {
      const response = await aiTutorApi.sendMessage(user.id, text)
      setMessages((prev) => [...prev, { id: `${Date.now()}-model`, role: 'model', text: response.reply }])
      if (!isOpen) setUnreadCount((c) => c + 1)
      return response
    } catch (err) {
      setError(err.message)
      setMessages((prev) => [
        ...prev,
        { id: `${Date.now()}-error`, role: 'model', text: "Sorry, I couldn't reply just now. Please try again." }
      ])
    } finally {
      setIsSending(false)
    }
  }, [user, isOpen])

  const submitAnswer = useCallback(async (payload) => {
    if (!user) return null
    try {
      const result = await aiTutorApi.submitAnswer(user.id, payload)
      setProfile((prev) =>
        prev && {
          ...prev,
          xp: result.newXp,
          level: result.newLevel,
          coins: result.newCoins,
          eloRating: result.newEloRating,
          difficultyBucket: result.newDifficultyBucket,
          topicMasteries: prev.topicMasteries.map((m) =>
            m.topic === result.topicMastery.topic ? result.topicMastery : m
          )
        }
      )
      return result
    } catch (err) {
      setError(err.message)
      return null
    }
  }, [user])

  const getRecommendation = useCallback(async () => {
    if (!user) return null
    try {
      return await aiTutorApi.getRecommendation(user.id)
    } catch (err) {
      setError(err.message)
      return null
    }
  }, [user])

  const toggleOpen = useCallback(() => {
    setIsOpen((prev) => {
      const next = !prev
      if (next) setUnreadCount(0)
      return next
    })
  }, [])

  const toggleDarkMode = useCallback(() => setDarkMode((d) => !d), [])

  const value = {
    profile,
    messages,
    isOpen,
    isSending,
    isLoadingProfile,
    error,
    unreadCount,
    darkMode,
    sendMessage,
    submitAnswer,
    getRecommendation,
    refreshProfile,
    toggleOpen,
    toggleDarkMode
  }

  return <AIContext.Provider value={value}>{children}</AIContext.Provider>
}

export function useAITutor() {
  const ctx = useContext(AIContext)
  if (!ctx) {
    throw new Error('useAITutor must be used within an AIProvider')
  }
  return ctx
}
