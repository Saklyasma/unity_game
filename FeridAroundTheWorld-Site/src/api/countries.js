import { apiClient } from './client.js'

export const countriesApi = {
  list: () => apiClient.get('/Countries'),
  getById: (id) => apiClient.get(`/Countries/${id}`),
  create: (payload) => apiClient.post('/Countries', payload),
  update: (id, payload) => apiClient.put(`/Countries/${id}`, payload),
  remove: (id) => apiClient.del(`/Countries/${id}`)
}
