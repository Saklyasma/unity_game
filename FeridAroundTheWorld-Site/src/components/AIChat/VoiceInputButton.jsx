import { useEffect, useRef, useState } from 'react'

const SpeechRecognitionCtor =
  typeof window !== 'undefined' ? window.SpeechRecognition || window.webkitSpeechRecognition : null

/** Feature-detected voice input — renders nothing in browsers without Web Speech API support. */
export default function VoiceInputButton({ onResult }) {
  const [listening, setListening] = useState(false)
  const recognitionRef = useRef(null)

  useEffect(() => {
    if (!SpeechRecognitionCtor) return undefined
    const recognition = new SpeechRecognitionCtor()
    recognition.continuous = false
    recognition.interimResults = false
    recognition.onresult = (event) => {
      const transcript = event.results[0]?.[0]?.transcript
      if (transcript) onResult(transcript)
    }
    recognition.onend = () => setListening(false)
    recognition.onerror = () => setListening(false)
    recognitionRef.current = recognition
    return () => recognition.stop()
  }, [onResult])

  if (!SpeechRecognitionCtor) return null

  const toggleListening = () => {
    if (listening) {
      recognitionRef.current?.stop()
      setListening(false)
    } else {
      recognitionRef.current?.start()
      setListening(true)
    }
  }

  return (
    <button
      type="button"
      className={`ai-chat-icon-btn${listening ? ' ai-chat-icon-btn--active' : ''}`}
      onClick={toggleListening}
      aria-pressed={listening}
      aria-label={listening ? 'Stop voice input' : 'Start voice input'}
      title={listening ? 'Stop voice input' : 'Voice input'}
    >
      🎤
    </button>
  )
}
