import { apiClient } from './client.js'

export const quizQuestionsApi = {
  list: () => apiClient.get('/QuizQuestions'),
  getById: (id) => apiClient.get(`/QuizQuestions/${id}`),
  listByCountry: (countryId, language) =>
    apiClient.get(`/QuizQuestions/country/${countryId}${language ? `?language=${language}` : ''}`),
  create: (payload) => apiClient.post('/QuizQuestions', payload),
  update: (id, payload) => apiClient.put(`/QuizQuestions/${id}`, payload),
  remove: (id) => apiClient.del(`/QuizQuestions/${id}`)
}
