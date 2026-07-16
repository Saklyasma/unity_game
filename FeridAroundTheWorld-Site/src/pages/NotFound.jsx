import { Link } from 'react-router-dom'

export default function NotFound() {
  return (
    <div className="auth-shell" style={{ flexDirection: 'column', gap: 16 }}>
      <div style={{ fontSize: '3rem' }}>🧭</div>
      <h1>Page introuvable</h1>
      <p>Ce terrain n'existe pas encore sur la carte.</p>
      <Link to="/" className="btn btn-primary">
        Retour à l'accueil
      </Link>
    </div>
  )
}
