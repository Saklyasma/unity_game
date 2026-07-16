import { apiClient } from './client.js'

/** Read-only on the backend — MatchesController only exposes GET endpoints. */
export const matchesApi = {
  list: () => apiClient.get('/Matches'),
  getById: (id) => apiClient.get(`/Matches/${id}`),
  listByTeam: (teamId) => apiClient.get(`/Matches/team/${teamId}`)
}
