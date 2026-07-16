import { useState } from 'react'
import { useLocalStorageCollection } from '../../hooks/useLocalStorageCollection.js'
import Modal from './Modal.jsx'

function emptyFormFrom(fields) {
  const form = {}
  fields.forEach((field) => {
    form[field.name] = field.type === 'number' ? 0 : ''
  })
  return form
}

/** Generic admin CRUD page: header + table + add/edit modal, backed by localStorage. */
export default function CrudSection({ icon, title, description, storageKey, seed, columns, fields }) {
  const [items, { add, update, remove }] = useLocalStorageCollection(storageKey, seed)
  const [editingId, setEditingId] = useState(null)
  const [form, setForm] = useState(null)

  function openCreate() {
    setEditingId('new')
    setForm(emptyFormFrom(fields))
  }

  function openEdit(item) {
    setEditingId(item.id)
    setForm(fields.reduce((acc, f) => ({ ...acc, [f.name]: item[f.name] }), {}))
  }

  function closeModal() {
    setEditingId(null)
    setForm(null)
  }

  function handleFieldChange(name, value) {
    setForm((prev) => ({ ...prev, [name]: value }))
  }

  function handleSubmit(e) {
    e.preventDefault()
    if (editingId === 'new') {
      add(form)
    } else {
      update(editingId, form)
    }
    closeModal()
  }

  function handleDelete(item) {
    if (window.confirm(`Supprimer "${item[columns[0].key]}" ?`)) {
      remove(item.id)
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

      <div className="table-wrap">
        {items.length === 0 ? (
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
                <tr key={item.id}>
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
            {fields.map((field) => (
              <div className="field" key={field.name}>
                <label htmlFor={field.name}>{field.label}</label>
                {field.type === 'select' ? (
                  <select
                    id={field.name}
                    value={form[field.name]}
                    onChange={(e) => handleFieldChange(field.name, e.target.value)}
                    required
                  >
                    <option value="" disabled>
                      Choisir...
                    </option>
                    {field.options.map((opt) => (
                      <option key={opt} value={opt}>
                        {opt}
                      </option>
                    ))}
                  </select>
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
                    value={form[field.name]}
                    onChange={(e) =>
                      handleFieldChange(field.name, field.type === 'number' ? Number(e.target.value) : e.target.value)
                    }
                    required
                  />
                )}
              </div>
            ))}
            <div className="modal-actions">
              <button type="button" className="btn btn-outline" onClick={closeModal}>
                Annuler
              </button>
              <button type="submit" className="btn btn-primary">
                Enregistrer
              </button>
            </div>
          </form>
        </Modal>
      )}
    </div>
  )
}
