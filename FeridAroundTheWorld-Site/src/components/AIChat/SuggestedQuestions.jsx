const DEFAULT_SUGGESTIONS = [
  'What should I study?',
  'Why did I lose?',
  'Explain the offside rule.',
  'Which country should I visit next?',
  'What are my weaknesses?',
  "Give me today's challenge.",
  'Motivate me!',
  'Create a personalized training plan.'
]

export default function SuggestedQuestions({ suggestions = DEFAULT_SUGGESTIONS, onPick, max = 4 }) {
  return (
    <div className="ai-chat-suggestions">
      {suggestions.slice(0, max).map((question) => (
        <button
          key={question}
          type="button"
          className="ai-chat-suggestion-chip"
          onClick={() => onPick(question)}
        >
          {question}
        </button>
      ))}
    </div>
  )
}
