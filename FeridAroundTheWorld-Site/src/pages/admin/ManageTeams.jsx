import { useApiResource } from '../../hooks/useApiResource.js'
import { teamsApi } from '../../api/teams.js'

/** Read-only — TeamsController only exposes GET endpoints on the backend. */
export default function ManageTeams() {
  const { items, loading, error } = useApiResource(teamsApi)

  return (
    <div>
      <div className="admin-header">
        <div>
          <h1>🏆 Équipes</h1>
          <p>Équipes nationales du tournoi, en lecture seule depuis l'API Teams (aucun CRUD côté backend).</p>
        </div>
      </div>

      {error && <div className="form-error">Impossible de charger les données depuis l'API : {error}</div>}

      <div className="table-wrap">
        {loading ? (
          <div className="empty-state">Chargement depuis l'API...</div>
        ) : items.length === 0 ? (
          <div className="empty-state">Aucune équipe trouvée.</div>
        ) : (
          <table>
            <thead>
              <tr>
                <th>Drapeau</th>
                <th>Équipe</th>
                <th>Code</th>
                <th>Groupe</th>
              </tr>
            </thead>
            <tbody>
              {items.map((team) => (
                <tr key={team.id}>
                  <td>{team.flagEmoji}</td>
                  <td>{team.name}</td>
                  <td>{team.code}</td>
                  <td>
                    <span className="badge badge-blue">{team.group}</span>
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
