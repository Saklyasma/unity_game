import { useState } from 'react'
import { Link, useLocation, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext.jsx'

export default function SignIn() {
  const { signIn } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()
  const [form, setForm] = useState({ email: '', password: '' })
  const [error, setError] = useState('')

  function handleChange(e) {
    setForm((prev) => ({ ...prev, [e.target.name]: e.target.value }))
  }

  function handleSubmit(e) {
    e.preventDefault()
    setError('')

    const result = signIn(form)
    if (!result.ok) {
      setError(result.error)
      return
    }

    const redirectTo = location.state?.from?.pathname ?? (result.user.role === 'admin' ? '/admin' : '/')
    navigate(redirectTo, { replace: true })
  }

  return (
    <div className="auth-shell">
      <div className="auth-card">
        <h1>Se connecter 🔐</h1>
        <p>Authentifie-toi pour accéder à ton espace joueur ou admin.</p>

        {error && <div className="form-error">{error}</div>}

        <form onSubmit={handleSubmit}>
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
              placeholder="••••••••"
            />
          </div>
          <button type="submit" className="btn btn-primary btn-block">
            Se connecter
          </button>
        </form>

        <p className="auth-switch">
          Pas encore de compte ? <Link to="/sign-up">Devenir joueur</Link>
        </p>

        <div className="demo-hint">
          Compte admin de démo — email : <strong>admin@ferid.app</strong> / mot de passe :{' '}
          <strong>Admin123!</strong>
        </div>
      </div>
    </div>
  )
}
