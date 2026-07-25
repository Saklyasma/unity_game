import { useAuth } from '../../context/AuthContext.jsx'
import { useAITutor } from '../../context/AIContext.jsx'
import { useAIChat } from '../../hooks/useAIChat.js'
import ChatMessageList from './ChatMessageList.jsx'
import ChatInput from './ChatInput.jsx'
import SuggestedQuestions from './SuggestedQuestions.jsx'
import './aiChat.css'

export default function AIChatWidget() {
  const { isAuthenticated } = useAuth()
  const { profile, isOpen, toggleOpen, unreadCount, darkMode, toggleDarkMode } = useAITutor()
  const { messages, isSending, input, setInput, submit, submitSuggestion } = useAIChat()

  if (!isAuthenticated) return null

  return (
    <div className="ai-chat-widget" data-theme={darkMode ? 'dark' : 'light'}>
      {isOpen && (
        <div className="ai-chat-panel">
          <div className="ai-chat-header">
            <div>
              <p className="ai-chat-title">AI Tutor</p>
              {profile && (
                <p className="ai-chat-subtitle">
                  Level {profile.level} · {profile.xp} XP · {profile.difficultyBucket}
                </p>
              )}
            </div>
            <div className="ai-chat-header-actions">
              <button
                type="button"
                className="ai-chat-icon-btn"
                onClick={toggleDarkMode}
                aria-label="Toggle AI Tutor dark mode"
                title="Toggle dark mode"
              >
                {darkMode ? '☀️' : '🌙'}
              </button>
              <button
                type="button"
                className="ai-chat-icon-btn"
                onClick={toggleOpen}
                aria-label="Close AI Tutor"
              >
                ✕
              </button>
            </div>
          </div>

          <ChatMessageList messages={messages} isSending={isSending} />

          {messages.length <= 1 && (
            <SuggestedQuestions onPick={submitSuggestion} />
          )}

          <ChatInput input={input} setInput={setInput} onSubmit={submit} isSending={isSending} />
        </div>
      )}

      <button
        type="button"
        className="ai-chat-toggle"
        onClick={toggleOpen}
        aria-label={isOpen ? 'Close AI Tutor' : 'Open AI Tutor'}
      >
        {isOpen ? '✕' : '🎓'}
        {!isOpen && unreadCount > 0 && <span className="ai-chat-unread-badge">{unreadCount}</span>}
      </button>
    </div>
  )
}
