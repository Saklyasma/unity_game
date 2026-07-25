import { useId } from 'react'
import VoiceInputButton from './VoiceInputButton.jsx'

export default function ChatInput({ input, setInput, onSubmit, isSending }) {
  const inputId = useId()

  const handleSubmit = (event) => {
    event.preventDefault()
    onSubmit()
  }

  return (
    <form className="ai-chat-input-row" onSubmit={handleSubmit}>
      <label className="sr-only" htmlFor={inputId}>Message the AI Tutor</label>
      <input
        id={inputId}
        type="text"
        className="ai-chat-input"
        placeholder="Ask your AI coach anything..."
        value={input}
        onChange={(event) => setInput(event.target.value)}
        disabled={isSending}
      />
      <VoiceInputButton onResult={setInput} />
      <button type="submit" className="btn btn-primary btn-sm" disabled={isSending || !input.trim()}>
        Send
      </button>
    </form>
  )
}
