import { apiClient } from './client.js'

/** Create + read only on the backend — no update/delete endpoint exists for predictions. */
export const predictionsApi = {
  list: () => apiClient.get('/Predictions'),
  create: (payload) => apiClient.post('/Predictions', payload)
}
