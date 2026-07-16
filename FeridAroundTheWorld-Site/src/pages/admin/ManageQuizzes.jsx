import { useEffect, useState } from 'react'
import { useApiResource } from '../../hooks/useApiResource.js'
import { quizQuestionsApi } from '../../api/quizQuestions.js'
import { countriesApi } from '../../api/countries.js'
import Modal from '../../components/ui/Modal.jsx'

const LANGUAGES = [
  { value: 'ar', label: 'Arabe' },
  { value: 'en', label: 'Anglais' },
  { value: 'fr', label: 'Français' }
]

function emptyForm(countries) {
  return {
    countryId: countries[0]?.id ?? '',
    language: 'ar',
    question: '',
    answers: ['', ''],
    correctIndex: 0,
    audioResource: ''
  }
}

/** Defensive-quiz questions per country/language — full CRUD via /api/QuizQuestions (MongoDB WorldCupApiDb). */
export default function ManageQuizzes() {
  const { items, loading, error, create, update, remove } = useApiResource(quizQuestionsApi)
  const [countries, setCountries] = useState([])
  const [editingId, setEditingId] = useState(null)
  const [form, setForm] = useState(null)
  const [formError, setFormError] = useState(null)
  const [saving, setSaving] = useState(false)

  useEffect(() => {
    countriesApi.list().then(setCountries).catch(() => setCountries([]))
  }, [])

  function countryName(id) {
    return countries.find((c) => c.id === id)?.name ?? id
  }

  function openCreate() {
    setEditingId('new')
    setForm(emptyForm(countries))
    setFormError(null)
  }

  function openEdit(item) {
    setEditingId(item.id)
    setForm({
      countryId: item.countryId,
      language: item.language,
      question: item.question,
      answers: [...item.answers],
      correctIndex: item.correctIndex,
      audioResource: item.audioResource ?? ''
    })
    setFormError(null)
  }

  function closeModal() {
    setEditingId(null)
    setForm(null)
    setFormError(null)
  }

  function updateAnswer(index, value) {
    setForm((prev) => {
      const answers = [...prev.answers]
      answers[index] = value
      return { ...prev, answers }
    })
  }

  function addAnswer() {
    setForm((prev) => ({ ...prev, answers: [...prev.answers, ''] }))
  }

  function removeAnswer(index) {
    setForm((prev) => {
      const answers = prev.answers.filter((_, i) => i !== index)
      return { ...prev, answers, correctIndex: Math.min(prev.correctIndex, answers.length - 1) }
    })
  }

  async function handleSubmit(e) {
    e.preventDefault()
    setSaving(true)
    setFormError(null)
    const payload = { ...form, countryId: Number(form.countryId), correctIndex: Number(form.correctIndex) }
    try {
      if (editingId === 'new') {
        await create(payload)
      } else {
        await update(editingId, payload)
      }
      closeModal()
    } catch (err) {
      setFormError(err.message)
    } finally {
      setSaving(false)
    }
  }

  async function handleDelete(item) {
    if (!window.confirm(`Supprimer la question "${item.question}" ?`)) return
    try {
      await remove(item.id)
    } catch (err) {
      window.alert(err.message)
    }
  }

  return (
    <div>
      <div className="admin-header">
        <div>
          <h1>🧠 Gérer les quiz</h1>
          <p>Questions du quiz défensif par pays/langue — CRUD complet consommant l'API QuizQuestions.</p>
        </div>
        <button type="button" className="btn btn-primary" onClick={openCreate} disabled={countries.length === 0}>
          + Ajouter
        </button>
      </div>

      {error && <div className="form-error">Impossible de charger les données depuis l'API : {error}</div>}

      <div className="table-wrap">
        {loading ? (
          <div className="empty-state">Chargement depuis l'API...</div>
        ) : items.length === 0 ? (
          <div className="empty-state">Aucune question pour l'instant. Clique sur "Ajouter" pour commencer.</div>
        ) : (
          <table>
            <thead>
              <tr>
                <th>Question</th>
                <th>Pays</th>
                <th>Langue</th>
                <th>Réponses</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {items.map((item) => (
                <tr key={item.id}>
                  <td>{item.question}</td>
                  <td>{countryName(item.countryId)}</td>
                  <td>
                    <span className="badge badge-blue">{item.language}</span>
                  </td>
                  <td>{item.answers.length}</td>
                  <td>
                    <div className="row-actions">
                      <button type="button" className="btn btn-outline btn-sm" onClick={() => openEdit(item)}>
                        Modifier
                      </button>
                      <button type="button" className="btn btn-danger btn-sm" onClick={() => handleDelete(item)}>
                        Supprimer
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>

      {form && (
        <Modal title={editingId === 'new' ? 'Ajouter — une question de quiz' : 'Modifier — question de quiz'} onClose={closeModal}>
          <form onSubmit={handleSubmit}>
            {formError && <div className="form-error">{formError}</div>}

            <div className="field">
              <label htmlFor="countryId">Pays</label>
              <select
                id="countryId"
                value={form.countryId}
                onChange={(e) => setForm((prev) => ({ ...prev, countryId: Number(e.target.value) }))}
                required
              >
                {countries.map((c) => (
                  <option key={c.id} value={c.id}>
                    {c.flagEmoji} {c.name}
                  </option>
                ))}
              </select>
            </div>

            <div className="field">
              <label htmlFor="language">Langue</label>
              <select
                id="language"
                value={form.language}
                onChange={(e) => setForm((prev) => ({ ...prev, language: e.target.value }))}
                required
              >
                {LANGUAGES.map((l) => (
                  <option key={l.value} value={l.value}>
                    {l.label}
                  </option>
                ))}
              </select>
            </div>

            <div className="field">
              <label htmlFor="question">Question</label>
              <textarea
                id="question"
                rows={2}
                value={form.question}
                onChange={(e) => setForm((prev) => ({ ...prev, question: e.target.value }))}
                required
              />
            </div>

            <div className="field">
              <label>Réponses (au moins 2)</label>
              {form.answers.map((answer, index) => (
                <div key={index} style={{ display: 'flex', gap: 8, marginBottom: 8 }}>
                  <input
                    type="text"
                    value={answer}
                    onChange={(e) => updateAnswer(index, e.target.value)}
                    required
                  />
                  {form.answers.length > 2 && (
                    <button type="button" className="btn btn-outline btn-sm" onClick={() => removeAnswer(index)}>
                      ✕
                    </button>
                  )}
                </div>
              ))}
              <button type="button" className="btn btn-outline btn-sm" onClick={addAnswer}>
                + Ajouter une réponse
              </button>
            </div>

            <div className="field">
              <label htmlFor="correctIndex">Réponse correcte</label>
              <select
                id="correctIndex"
                value={form.correctIndex}
                onChange={(e) => setForm((prev) => ({ ...prev, correctIndex: Number(e.target.value) }))}
                required
              >
                {form.answers.map((answer, index) => (
                  <option key={index} value={index}>
                    {answer || `Réponse ${index + 1}`}
                  </option>
                ))}
              </select>
            </div>

            <div className="field">
              <label htmlFor="audioResource">Ressource audio (optionnel)</label>
              <input
                id="audioResource"
                type="text"
                placeholder="voices/ne-10-ar"
                value={form.audioResource}
                onChange={(e) => setForm((prev) => ({ ...prev, audioResource: e.target.value }))}
              />
            </div>

            <div className="modal-actions">
              <button type="button" className="btn btn-outline" onClick={closeModal}>
                Annuler
              </button>
              <button type="submit" className="btn btn-primary" disabled={saving}>
                {saving ? 'Enregistrement...' : 'Enregistrer'}
              </button>
            </div>
          </form>
        </Modal>
      )}
    </div>
  )
}
