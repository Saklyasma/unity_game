import { useId, useState } from 'react'
import { useAuth } from '../context/AuthContext.jsx'
import { useChatProgress } from '../hooks/useChatProgress.js'
import { CHAT_QUIZ_BANK, pickRandomQuestion } from '../data/chatQuizBank.js'
import { CHAT_INTENTS, FALLBACK_SCORE_THRESHOLD } from '../data/chatIntents.js'
import { findBestMatch } from '../lib/textSimilarity.js'

function matchIntent(message) {
  const flatExamples = CHAT_INTENTS.flatMap((intent) => intent.examples.map((example) => ({ intent: intent.id, example })))
  const { index, score } = findBestMatch(message, flatExamples.map((e) => e.example))
  if (index === -1 || score < FALLBACK_SCORE_THRESHOLD) return 'fallback'
  return flatExamples[index].intent
}

function bubble(role, text) {
  return { id: `${Date.now()}-${Math.random().toString(36).slice(2, 7)}`, role, text }
}

// Shown as quick-reply chips right after the greeting, so a first-time user sees
// straight away what they can ask instead of having to guess free text.
const SUGGESTIONS = [
  { label: '🎯 Un quiz surprise', text: 'je veux jouer' },
  { label: '📊 Ma progression / mon niveau / mes stats', text: 'montre moi ma progression' },
  { label: '🩹 Mon point faible', text: 'mon point faible' }
]

export default function Chatbot() {
  const { isAuthenticated, user } = useAuth()
  const progress = useChatProgress(user?.id ?? 'anonymous')
  const [open, setOpen] = useState(false)
  const [messages, setMessages] = useState(() => [
    bubble(
      'bot',
      "Salut, je suis l'assistant de Ferid Around the World ! Choisis une suggestion ci-dessous, ou écris-moi directement. 🌍"
    )
  ])
  const [input, setInput] = useState('')
  const [activeQuestion, setActiveQuestion] = useState(null)
  const inputId = useId()

  if (!isAuthenticated) return null

  function say(text) {
    setMessages((prev) => [...prev, bubble('bot', text)])
  }

  function askQuestion(category) {
    const pool = category ? CHAT_QUIZ_BANK.filter((q) => q.category === category) : CHAT_QUIZ_BANK
    const question = pool[Math.floor(Math.random() * pool.length)] ?? pickRandomQuestion()
    setActiveQuestion(question)
    say(question.question)
  }

  function handleAnswer(optionIndex) {
    if (!activeQuestion) return
    const correct = optionIndex === activeQuestion.correctIndex
    progress.recordAnswer(activeQuestion.category, correct)
    setMessages((prev) => [...prev, bubble('user', activeQuestion.options[optionIndex])])

    const label = progress.categoryLabels[activeQuestion.category]
    if (correct) {
      say(`Bonne réponse ! 🎉 Ta maîtrise en ${label} progresse.`)
    } else {
      const correctAnswer = activeQuestion.options[activeQuestion.correctIndex]
      say(`Pas tout à fait — la bonne réponse était "${correctAnswer}". On continue en ${label} !`)
    }
    setActiveQuestion(null)
  }

  function processMessage(text) {
    setMessages((prev) => [...prev, bubble('user', text)])

    const intent = matchIntent(text)
    switch (intent) {
      case 'greeting':
        say(`Salut ${user.name} ! Prêt(e) pour un quiz surprise ?`)
        break
      case 'start_quiz':
        askQuestion()
        break
      case 'ask_progress': {
        const pct = Math.round(progress.overallMastery * 100)
        say(
          `Niveau global : ${progress.level} (${pct}%). Questions répondues : ${progress.totalAnswered}, bonnes réponses : ${progress.totalCorrect}.`
        )
        break
      }
      case 'ask_weakness': {
        const weak = progress.weakestCategory()
        say(`Ton point à travailler : ${progress.categoryLabels[weak]}. Je te pose une question dessus ?`)
        askQuestion(weak)
        break
      }
      case 'ask_hint':
        if (activeQuestion) {
          const wrongIndex = activeQuestion.options.findIndex((_, i) => i !== activeQuestion.correctIndex)
          say(`Indice : ce n'est pas "${activeQuestion.options[wrongIndex]}". 🔍`)
        } else {
          say("Demande-moi d'abord un quiz surprise, je te donnerai un indice dessus !")
        }
        break
      case 'thanks':
        say('Avec plaisir ! 😊')
        break
      default:
        say("Je n'ai pas bien compris. Essaie : \"quiz surprise\", \"ma progression\", ou \"un indice\".")
    }
  }

  function handleSend(e) {
    e.preventDefault()
    const text = input.trim()
    if (!text) return
    setInput('')
    processMessage(text)
  }

  function handleSuggestion(text) {
    processMessage(text)
  }

  return (
    <div className="chatbot-root">
      {open && (
        <div className="chatbot-panel">
          <div className="chatbot-header">
            <span>🤖 Assistant Ferid</span>
            <button type="button" className="chatbot-close" onClick={() => setOpen(false)} aria-label="Fermer">
              ✕
            </button>
          </div>

          <div className="chatbot-messages">
            {messages.map((m) => (
              <div key={m.id} className={`chatbot-bubble chatbot-bubble-${m.role}`}>
                {m.text}
              </div>
            ))}

            {activeQuestion && (
              <div className="chatbot-options">
                {activeQuestion.options.map((option, index) => (
                  <button key={option} type="button" className="btn btn-outline btn-sm" onClick={() => handleAnswer(index)}>
                    {option}
                  </button>
                ))}
              </div>
            )}

            {messages.length === 1 && !activeQuestion && (
              <div className="chatbot-options">
                {SUGGESTIONS.map((suggestion) => (
                  <button
                    key={suggestion.label}
                    type="button"
                    className="btn btn-outline btn-sm"
                    onClick={() => handleSuggestion(suggestion.text)}
                  >
                    {suggestion.label}
                  </button>
                ))}
              </div>
            )}
          </div>

          <form className="chatbot-input-row" onSubmit={handleSend}>
            <label htmlFor={inputId} className="sr-only">
              Message
            </label>
            <input
              id={inputId}
              type="text"
              placeholder="Écris un message..."
              value={input}
              onChange={(e) => setInput(e.target.value)}
            />
            <button type="submit" className="btn btn-primary btn-sm">
              Envoyer
            </button>
          </form>
        </div>
      )}

      <button type="button" className="chatbot-toggle" onClick={() => setOpen((prev) => !prev)}>
        {open ? '✕' : '🤖'}
      </button>
    </div>
  )
}
