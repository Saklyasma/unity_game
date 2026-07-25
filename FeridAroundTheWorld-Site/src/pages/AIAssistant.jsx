import { useEffect, useState } from 'react'
import { useAITutor } from '../context/AIContext.jsx'
import { useAIChat } from '../hooks/useAIChat.js'
import ChatMessageList from '../components/AIChat/ChatMessageList.jsx'
import ChatInput from '../components/AIChat/ChatInput.jsx'
import SuggestedQuestions from '../components/AIChat/SuggestedQuestions.jsx'
import { topicLabel } from '../data/aiTutorTopics.js'
import '../components/AIChat/aiChat.css'

export default function AIAssistant() {
  const { profile, isLoadingProfile, getRecommendation } = useAITutor()
  const { messages, isSending, input, setInput, submit, submitSuggestion } = useAIChat()
  const [recommendation, setRecommendation] = useState(null)

  useEffect(() => {
    let active = true
    getRecommendation().then((result) => {
      if (active) setRecommendation(result)
    })
    return () => {
      active = false
    }
  }, [getRecommendation])

  const weakTopics = profile
    ? [...profile.topicMasteries].sort((a, b) => a.masteryProbability - b.masteryProbability).slice(0, 3)
    : []

  return (
    <div className="ai-assistant-page">
      <h1>Your AI Football Coach</h1>

      {isLoadingProfile && <p>Loading your profile...</p>}

      {profile && (
        <div className="ai-assistant-summary">
          <div className="ai-assistant-stat"><strong>Level</strong> {profile.level}</div>
          <div className="ai-assistant-stat"><strong>XP</strong> {profile.xp}</div>
          <div className="ai-assistant-stat"><strong>Coins</strong> {profile.coins}</div>
          <div className="ai-assistant-stat"><strong>Difficulty</strong> {profile.difficultyBucket}</div>
          <div className="ai-assistant-stat"><strong>Accuracy</strong> {Math.round(profile.accuracy * 100)}%</div>
        </div>
      )}

      <div className="ai-assistant-grid">
        <section className="ai-assistant-panel">
          <h2>Weak Topics</h2>
          <ul>
            {weakTopics.map((topic) => (
              <li key={topic.topic}>
                {topicLabel(topic.topic)} — {Math.round(topic.masteryProbability * 100)}% mastery
              </li>
            ))}
          </ul>

          {recommendation && (
            <div className="ai-assistant-recommendation">
              <h3>Recommended next</h3>
              <p>{recommendation.recommendedQuizLabel}</p>
              <p className="ai-assistant-reason">{recommendation.reason}</p>
              {recommendation.recommendedCountryName && (
                <p>Next country to explore: <strong>{recommendation.recommendedCountryName}</strong></p>
              )}
            </div>
          )}
        </section>

        <section className="ai-assistant-chat-panel ai-chat-theme">
          <ChatMessageList messages={messages} isSending={isSending} />
          <SuggestedQuestions onPick={submitSuggestion} />
          <ChatInput input={input} setInput={setInput} onSubmit={submit} isSending={isSending} />
        </section>
      </div>
    </div>
  )
}
