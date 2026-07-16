import { apiClient } from './client.js'

export const botStatsApi = {
  list: () => apiClient.get('/BotStats'),
  getById: (id) => apiClient.get(`/BotStats/${id}`),
  getByCountry: (countryId) => apiClient.get(`/BotStats/country/${countryId}`),
  create: (payload) => apiClient.post('/BotStats', payload),
  update: (id, payload) => apiClient.put(`/BotStats/${id}`, payload),
  remove: (id) => apiClient.del(`/BotStats/${id}`)
}
