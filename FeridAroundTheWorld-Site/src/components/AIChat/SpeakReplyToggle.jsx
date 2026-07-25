import { stripMarkdown } from '../../lib/stripMarkdown.js'

const supportsSpeech = typeof window !== 'undefined' && 'speechSynthesis' in window

/** Feature-detected speech synthesis for one reply — renders nothing if unsupported. */
export default function SpeakReplyToggle({ text }) {
  if (!supportsSpeech) return null

  const speak = () => {
    window.speechSynthesis.cancel()
    const utterance = new SpeechSynthesisUtterance(stripMarkdown(text))
    window.speechSynthesis.speak(utterance)
  }

  return (
    <button
      type="button"
      className="ai-chat-icon-btn ai-chat-speak-btn"
      onClick={speak}
      aria-label="Read this reply aloud"
      title="Read aloud"
    >
      🔊
    </button>
  )
}
