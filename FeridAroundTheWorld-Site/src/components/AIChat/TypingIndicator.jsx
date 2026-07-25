export default function TypingIndicator() {
  return (
    <div className="ai-chat-bubble ai-chat-bubble--model ai-chat-typing" aria-live="polite" aria-label="AI Tutor is typing">
      <span className="ai-chat-typing-dot" />
      <span className="ai-chat-typing-dot" />
      <span className="ai-chat-typing-dot" />
    </div>
  )
}
