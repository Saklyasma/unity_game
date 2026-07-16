import { useState } from 'react'
import { useAuth } from '../../context/AuthContext.jsx'

const AVATAR_OPTIONS = ['🧑‍💼', '🧑‍🏫', '🧑‍💻', '⚽', '🌍']

export default function AdminProfile() {
  const { user, updateProfile } = useAuth()
  const [form, setForm] = useState({ name: user.name, bio: user.bio ?? '', avatar: user.avatar })
  const [saved, setSaved] = useState(false)

  function handleSubmit(e) {
    e.preventDefault()
    updateProfile(form)
    setSaved(true)
    setTimeout(() => setSaved(false), 2000)
  }

  return (
    <div>
      <div className="admin-header">
        <div>
          <h1>🙍 Mon profil</h1>
          <p>Gère les informations de ton compte administrateur.</p>
        </div>
      </div>

      <div className="card" style={{ maxWidth: 480 }}>
        {saved && <div className="form-success">Profil mis à jour !</div>}

        <form onSubmit={handleSubmit}>
          <div className="field">
            <label>Avatar</label>
            <div style={{ display: 'flex', gap: 8 }}>
              {AVATAR_OPTIONS.map((emoji) => (
                <button
                  type="button"
                  key={emoji}
                  onClick={() => setForm((prev) => ({ ...prev, avatar: emoji }))}
                  className="btn btn-outline btn-sm"
                  style={{
                    fontSize: '1.2rem',
                    borderColor: form.avatar === emoji ? 'var(--color-primary)' : undefined
                  }}
                >
                  {emoji}
                </button>
              ))}
            </div>
          </div>

          <div className="field">
            <label htmlFor="name">Nom</label>
            <input
              id="name"
              value={form.name}
              onChange={(e) => setForm((prev) => ({ ...prev, name: e.target.value }))}
              required
            />
          </div>

          <div className="field">
            <label htmlFor="email">Email</label>
            <input id="email" value={user.email} disabled />
          </div>

          <div className="field">
            <label htmlFor="bio">Bio</label>
            <textarea
              id="bio"
              rows={3}
              value={form.bio}
              onChange={(e) => setForm((prev) => ({ ...prev, bio: e.target.value }))}
            />
          </div>

          <button type="submit" className="btn btn-primary">
            Enregistrer les modifications
          </button>
        </form>
      </div>
    </div>
  )
}
