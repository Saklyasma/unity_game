import { useEffect, useState } from 'react'
import { useApiResource } from '../../hooks/useApiResource.js'
import { predictionsApi } from '../../api/predictions.js'
import { matchesApi } from '../../api/matches.js'
import Modal from '../../components/ui/Modal.jsx'

function emptyForm(matches) {
  return {
    matchId: matches[0]?.id ?? '',
    playerName: '',
    predictedHomeScore: 0,
    predictedAwayScore: 0
  }
}

/** Create + read only — PredictionsController has no update/delete endpoint on the backend. */
export default function ManagePredictions() {
  const { items, loading, error, create } = useApiResource(predictionsApi)
  const [matches, setMatches] = useState([])
  const [form, setForm] = useState(null)
  const [formError, setFormError] = useState(null)
  const [saving, setSaving] = useState(false)

  useEffect(() => {
    matchesApi.list().then(setMatches).catch(() => setMatches([]))
  }, [])

  function matchLabel(id) {
    const match = matches.find((m) => m.id === id)
    return match ? `${match.homeTeam.name} vs ${match.awayTeam.name}` : id
  }

  function openCreate() {
    setForm(emptyForm(matches))
    setFormError(null)
  }

  function closeModal() {
    setForm(null)
    setFormError(null)
  }

  async function handleSubmit(e) {
    e.preventDefault()
    setSaving(true)
    setFormError(null)
    try {
      await create({ ...form, matchId: Number(form.matchId) })
      closeModal()
    } catch (err) {
      setFormError(err.message)
    } finally {
      setSaving(false)
    }
  }

  return (
    <div>
      <div className="admin-header">
        <div>
          <h1>🔮 Prédictions</h1>
          <p>Pronostics de score soumis par les joueurs — lecture + soumission via l'API Predictions (pas de modification/suppression côté backend).</p>
        </div>
        <button type="button" className="btn btn-primary" onClick={openCreate} disabled={matches.length === 0}>
          + Soumettre une prédiction
        </button>
      </div>

      {error && <div className="form-error">Impossible de charger les données depuis l'API : {error}</div>}

      <div className="table-wrap">
        {loading ? (
          <div className="empty-state">Chargement depuis l'API...</div>
        ) : items.length === 0 ? (
          <div className="empty-state">Aucune prédiction pour l'instant.</div>
        ) : (
          <table>
            <thead>
              <tr>
                <th>Joueur</th>
                <th>Match</th>
                <th>Score prédit</th>
                <th>Soumis le</th>
              </tr>
            </thead>
            <tbody>
              {items.map((p) => (
                <tr key={p.id}>
                  <td>{p.playerName}</td>
                  <td>{matchLabel(p.matchId)}</td>
                  <td>
                    {p.predictedHomeScore} : {p.predictedAwayScore}
                  </td>
                  <td>{new Date(p.submittedAtUtc).toLocaleString()}</td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>

      {form && (
        <Modal title="Soumettre une prédiction" onClose={closeModal}>
          <form onSubmit={handleSubmit}>
            {formError && <div className="form-error">{formError}</div>}

            <div className="field">
              <label htmlFor="matchId">Match</label>
              <select
                id="matchId"
                value={form.matchId}
                onChange={(e) => setForm((prev) => ({ ...prev, matchId: Number(e.target.value) }))}
                required
              >
                {matches.map((m) => (
                  <option key={m.id} value={m.id}>
                    {m.homeTeam.name} vs {m.awayTeam.name}
                  </option>
                ))}
              </select>
            </div>

            <div className="field">
              <label htmlFor="playerName">Nom du joueur</label>
              <input
                id="playerName"
                type="text"
                value={form.playerName}
                onChange={(e) => setForm((prev) => ({ ...prev, playerName: e.target.value }))}
                required
              />
            </div>

            <div className="field">
              <label htmlFor="predictedHomeScore">Score prédit — domicile</label>
              <input
                id="predictedHomeScore"
                type="number"
                min={0}
                max={20}
                value={form.predictedHomeScore}
                onChange={(e) => setForm((prev) => ({ ...prev, predictedHomeScore: Number(e.target.value) }))}
                required
              />
            </div>

            <div className="field">
              <label htmlFor="predictedAwayScore">Score prédit — extérieur</label>
              <input
                id="predictedAwayScore"
                type="number"
                min={0}
                max={20}
                value={form.predictedAwayScore}
                onChange={(e) => setForm((prev) => ({ ...prev, predictedAwayScore: Number(e.target.value) }))}
                required
              />
            </div>

            <div className="modal-actions">
              <button type="button" className="btn btn-outline" onClick={closeModal}>
                Annuler
              </button>
              <button type="submit" className="btn btn-primary" disabled={saving}>
                {saving ? 'Envoi...' : 'Envoyer'}
              </button>
            </div>
          </form>
        </Modal>
      )}
    </div>
  )
}
