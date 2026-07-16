import { useState } from 'react'
import { useApiResource } from '../../hooks/useApiResource.js'
import Modal from './Modal.jsx'

function emptyFormFrom(fields) {
  const form = {}
  fields.forEach((field) => {
    if (field.type === 'number') form[field.name] = field.defaultValue ?? 0
    else if (field.type === 'checkbox') form[field.name] = field.defaultValue ?? false
    else form[field.name] = field.defaultValue ?? ''
  })
  return form
}

/** Generic admin CRUD page backed by the WorldCupApi backend (MongoDB "WorldCupApiDb"). */
export default function ApiCrudSection({ icon, title, description, api, columns, fields, idKey = 'id' }) {
  const { items, loading, error, create, update, remove } = useApiResource(api)
  const [editingId, setEditingId] = useState(null)
  const [form, setForm] = useState(null)
  const [formError, setFormError] = useState(null)
  const [saving, setSaving] = useState(false)

  function openCreate() {
    setEditingId('new')
    setForm(emptyFormFrom(fields))
    setFormError(null)
  }

  function openEdit(item) {
    setEditingId(item[idKey])
    setForm(fields.reduce((acc, f) => ({ ...acc, [f.name]: item[f.name] }), {}))
    setFormError(null)
  }

  function closeModal() {
    setEditingId(null)
    setForm(null)
    setFormError(null)
  }

  function handleFieldChange(name, value) {
    setForm((prev) => ({ ...prev, [name]: value }))
  }

  async function handleSubmit(e) {
    e.preventDefault()
    setSaving(true)
    setFormError(null)
    try {
      if (editingId === 'new') {
        await create(form)
      } else {
        await update(editingId, form)
      }
      closeModal()
    } catch (err) {
      setFormError(err.message)
    } finally {
      setSaving(false)
    }
  }

  async function handleDelete(item) {
    if (!window.confirm(`Supprimer "${item[columns[0].key]}" ?`)) return
    try {
      await remove(item[idKey])
    } catch (err) {
      window.alert(err.message)
    }
  }

  return (
    <div>
      <div className="admin-header">
        <div>
          <h1>
            {icon} {title}
          </h1>
          <p>{description}</p>
        </div>
        <button type="button" className="btn btn-primary" onClick={openCreate}>
          + Ajouter
        </button>
      </div>

      {error && <div className="form-error">Impossible de charger les données depuis l'API : {error}</div>}

      <div className="table-wrap">
        {loading ? (
          <div className="empty-state">Chargement depuis l'API...</div>
        ) : items.length === 0 ? (
          <div className="empty-state">Aucun élément pour l'instant. Clique sur "Ajouter" pour commencer.</div>
        ) : (
          <table>
            <thead>
              <tr>
                {columns.map((col) => (
                  <th key={col.key}>{col.label}</th>
                ))}
                <th></th>
              </tr>
            </thead>
            <tbody>
              {items.map((item) => (
                <tr key={item[idKey]}>
                  {columns.map((col) => (
                    <td key={col.key}>{col.render ? col.render(item) : item[col.key]}</td>
                  ))}
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
        <Modal title={editingId === 'new' ? `Ajouter — ${title}` : `Modifier — ${title}`} onClose={closeModal}>
          <form onSubmit={handleSubmit}>
            {formError && <div className="form-error">{formError}</div>}
            {fields.map((field) => (
              <div className="field" key={field.name}>
                <label htmlFor={field.name}>{field.label}</label>
                {field.type === 'select' ? (
                  <select
                    id={field.name}
                    value={form[field.name]}
                    onChange={(e) =>
                      handleFieldChange(field.name, field.numeric ? Number(e.target.value) : e.target.value)
                    }
                    required
                  >
                    <option value="" disabled>
                      Choisir...
                    </option>
                    {field.options.map((opt) => (
                      <option key={opt.value} value={opt.value}>
                        {opt.label}
                      </option>
                    ))}
                  </select>
                ) : field.type === 'checkbox' ? (
                  <input
                    id={field.name}
                    type="checkbox"
                    checked={form[field.name]}
                    onChange={(e) => handleFieldChange(field.name, e.target.checked)}
                    style={{ width: 'auto' }}
                  />
                ) : field.type === 'textarea' ? (
                  <textarea
                    id={field.name}
                    rows={3}
                    value={form[field.name]}
                    onChange={(e) => handleFieldChange(field.name, e.target.value)}
                    required
                  />
                ) : (
                  <input
                    id={field.name}
                    type={field.type === 'number' ? 'number' : 'text'}
                    step={field.step}
                    min={field.min}
                    max={field.max}
                    value={form[field.name]}
                    onChange={(e) =>
                      handleFieldChange(field.name, field.type === 'number' ? Number(e.target.value) : e.target.value)
                    }
                    required={field.type !== 'number'}
                  />
                )}
              </div>
            ))}
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
