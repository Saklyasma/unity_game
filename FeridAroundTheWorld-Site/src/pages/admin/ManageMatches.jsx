import { useApiResource } from '../../hooks/useApiResource.js'
import { matchesApi } from '../../api/matches.js'

const STATUS_BADGE = {
  Scheduled: 'badge-gray',
  Live: 'badge-yellow',
  Finished: 'badge-blue'
}

/** Read-only — MatchesController only exposes GET endpoints on the backend. */
export default function ManageMatches() {
  const { items, loading, error } = useApiResource(matchesApi)

  return (
    <div>
      <div className="admin-header">
        <div>
          <h1>📅 Matchs</h1>
          <p>Rencontres du tournoi, en lecture seule depuis l'API Matches (aucun CRUD côté backend).</p>
        </div>
      </div>

      {error && <div className="form-error">Impossible de charger les données depuis l'API : {error}</div>}

      <div className="table-wrap">
        {loading ? (
          <div className="empty-state">Chargement depuis l'API...</div>
        ) : items.length === 0 ? (
          <div className="empty-state">Aucun match trouvé.</div>
        ) : (
          <table>
            <thead>
              <tr>
                <th>Rencontre</th>
                <th>Stade</th>
                <th>Coup d'envoi (UTC)</th>
                <th>Score</th>
                <th>Statut</th>
              </tr>
            </thead>
            <tbody>
              {items.map((match) => (
                <tr key={match.id}>
                  <td>
                    {match.homeTeam.flagEmoji} {match.homeTeam.name} vs {match.awayTeam.name} {match.awayTeam.flagEmoji}
                  </td>
                  <td>{match.stadium}</td>
                  <td>{new Date(match.kickOffUtc).toLocaleString()}</td>
                  <td>
                    {match.homeScore ?? '-'} : {match.awayScore ?? '-'}
                  </td>
                  <td>
                    <span className={`badge ${STATUS_BADGE[match.status] ?? 'badge-gray'}`}>{match.status}</span>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </div>
  )
}
