import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext.jsx'

export default function SignUp() {
  const { signUp } = useAuth()
  const navigate = useNavigate()
  const [form, setForm] = useState({ name: '', email: '', password: '' })
  const [error, setError] = useState('')
  const [done, setDone] = useState(false)

  function handleChange(e) {
    setForm((prev) => ({ ...prev, [e.target.name]: e.target.value }))
  }

  function handleSubmit(e) {
    e.preventDefault()
    setError('')

    if (form.password.length < 6) {
      setError('Le mot de passe doit contenir au moins 6 caractères.')
      return
    }

    const result = signUp(form)
    if (!result.ok) {
      setError(result.error)
      return
    }

    setDone(true)
  }

  if (done) {
    return (
      <div className="auth-shell">
        <div className="auth-card" style={{ textAlign: 'center' }}>
          <div style={{ fontSize: '3rem' }}>🎉</div>
          <h1>Bienvenue, {form.name} !</h1>
          <p>Ton compte joueur est prêt. La dernière étape : télécharge l'application pour commencer à jouer.</p>
          <div style={{ display: 'flex', flexDirection: 'column', gap: 10, marginTop: 20 }}>
            <button className="btn btn-primary btn-block" onClick={() => navigate('/#download')}>
              ⬇️ Télécharger l'application
            </button>
            <button className="btn btn-outline btn-block" onClick={() => navigate('/#discover')}>
              🔎 Découvrir les fonctionnalités du jeu
            </button>
          </div>
        </div>
      </div>
    )
  }

  return (
    <div className="auth-shell">
      <div className="auth-card">
        <h1>Devenir joueur ⚽</h1>
        <p>Crée ton compte pour commencer ton tour du monde.</p>

        {error && <div className="form-error">{error}</div>}

        <form onSubmit={handleSubmit}>
          <div className="field">
            <label htmlFor="name">Nom</label>
            <input id="name" name="name" required value={form.name} onChange={handleChange} placeholder="Ton prénom" />
          </div>
          <div className="field">
            <label htmlFor="email">Email</label>
            <input
              id="email"
              name="email"
              type="email"
              required
              value={form.email}
              onChange={handleChange}
              placeholder="toi@exemple.com"
            />
          </div>
          <div className="field">
            <label htmlFor="password">Mot de passe</label>
            <input
              id="password"
              name="password"
              type="password"
              required
              value={form.password}
              onChange={handleChange}
              placeholder="6 caractères minimum"
            />
          </div>
          <button type="submit" className="btn btn-primary btn-block">
            Créer mon compte
          </button>
        </form>

        <p className="auth-switch">
          Déjà un compte ? <Link to="/sign-in">Se connecter</Link>
        </p>
      </div>
    </div>
  )
}
