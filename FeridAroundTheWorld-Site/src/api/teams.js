import { apiClient } from './client.js'

/** Read-only on the backend — TeamsController only exposes GET endpoints. */
export const teamsApi = {
  list: () => apiClient.get('/Teams'),
  getById: (id) => apiClient.get(`/Teams/${id}`)
}
