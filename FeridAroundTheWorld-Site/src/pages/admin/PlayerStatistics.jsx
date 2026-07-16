import { useAuth } from '../../context/AuthContext.jsx'

const WORLD_LEVELS = ['Stade de Tunis', 'Arène de Rio', 'Colisée de Tokyo']

// Deterministic pseudo-stats derived from the account id, so numbers stay stable across renders
// (there is no real gameplay backend yet — see the "frontend only, mocked data" scope for this project).
function statsFor(id) {
  const score = ((id * 733) % 2400) + 100
  const level = WORLD_LEVELS[id % WORLD_LEVELS.length]
  const countriesVisited = (id % 8) + 1
  const quizzesCompleted = (id * 3) % 20
  return { score, level, countriesVisited, quizzesCompleted }
}

export default function PlayerStatistics() {
  const { users } = useAuth()
  const players = users.filter((u) => u.role === 'player')

  return (
    <div>
      <div className="admin-header">
        <div>
          <h1>📊 Statistiques des joueurs</h1>
          <p>Aperçu (données de démonstration) de la progression des joueurs inscrits.</p>
        </div>
      </div>

      <div className="stat-grid">
        <div className="stat-card">
          <div className="value">{players.length}</div>
          <div className="label">Joueurs inscrits</div>
        </div>
        <div className="stat-card">
          <div className="value">
            {players.reduce((sum, p) => sum + statsFor(p.id).score, 0)}
          </div>
          <div className="label">Score total cumulé</div>
        </div>
      </div>

      <div className="table-wrap">
        {players.length === 0 ? (
          <div className="empty-state">Aucun joueur inscrit pour le moment.</div>
        ) : (
          <table>
            <thead>
              <tr>
                <th>Joueur</th>
                <th>Niveau actuel</th>
                <th>Score</th>
                <th>Pays visités</th>
                <th>Quiz complétés</th>
              </tr>
            </thead>
            <tbody>
              {players.map((p) => {
                const stats = statsFor(p.id)
                return (
                  <tr key={p.id}>
                    <td>
                      {p.avatar} {p.name}
                    </td>
                    <td>{stats.level}</td>
                    <td>{stats.score}</td>
                    <td>{stats.countriesVisited}</td>
                    <td>{stats.quizzesCompleted}</td>
                  </tr>
                )
              })}
            </tbody>
          </table>
        )}
      </div>
    </div>
  )
}
