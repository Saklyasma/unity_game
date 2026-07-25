import { apiClient } from './client.js'

/**
 * AI Tutor endpoints (WorldCupApi.Api: PlayerProfilesController, AiChatController,
 * AiRecommendationsController, AiQuizAttemptsController). Gemini is only ever called
 * from the backend — this file never talks to Gemini directly.
 */
export const aiTutorApi = {
  getOrCreateProfile: (playerId, username) =>
    apiClient.get(`/PlayerProfiles/${playerId}${username ? `?username=${encodeURIComponent(username)}` : ''}`),

  updateProfile: (playerId, payload) => apiClient.put(`/PlayerProfiles/${playerId}`, payload),

  sendMessage: (playerId, message) => apiClient.post('/AiChat/messages', { playerId, message }),

  getChatHistory: (playerId, take = 20) => apiClient.get(`/AiChat/messages/${playerId}?take=${take}`),

  getRecommendation: (playerId) => apiClient.get(`/AiRecommendations/${playerId}`),

  submitAnswer: (playerId, { topic, questionId, isCorrect, responseTimeMs, difficultyAttempted }) =>
    apiClient.post('/AiQuizAttempts', {
      playerId,
      topic,
      questionId,
      isCorrect,
      responseTimeMs,
      difficultyAttempted
    })
}
