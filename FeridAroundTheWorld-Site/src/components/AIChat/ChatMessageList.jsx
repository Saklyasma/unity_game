import ReactMarkdown from 'react-markdown'
import SpeakReplyToggle from './SpeakReplyToggle.jsx'
import TypingIndicator from './TypingIndicator.jsx'

export default function ChatMessageList({ messages, isSending }) {
  return (
    <div className="ai-chat-messages">
      {messages.map((message) => (
        <div
          key={message.id}
          className={`ai-chat-bubble ai-chat-bubble--${message.role === 'model' ? 'model' : 'user'}`}
        >
          {message.role === 'model' ? (
            <>
              <div className="ai-chat-markdown">
                <ReactMarkdown>{message.text}</ReactMarkdown>
              </div>
              <SpeakReplyToggle text={message.text} />
            </>
          ) : (
            message.text
          )}
        </div>
      ))}
      {isSending && <TypingIndicator />}
    </div>
  )
}
