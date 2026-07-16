import { NavLink, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext.jsx'

export default function Navbar() {
  const { user, signOut, isAdmin } = useAuth()
  const navigate = useNavigate()

  function handleSignOut() {
    signOut()
    navigate('/')
  }

  return (
    <header className="navbar">
      <div className="container navbar-inner">
        <NavLink to="/" className="brand">
          <span className="brand-badge">⚽</span>
          Ferid Around the World
        </NavLink>

        <nav className="nav-links">
          <NavLink to="/" end className={({ isActive }) => `nav-link${isActive ? ' active' : ''}`}>
            Accueil
          </NavLink>

          {!user && (
            <>
              <NavLink to="/sign-in" className={({ isActive }) => `nav-link${isActive ? ' active' : ''}`}>
                Se connecter
              </NavLink>
              <NavLink to="/sign-up" className="btn btn-primary btn-sm">
                Devenir joueur
              </NavLink>
            </>
          )}

          {user && isAdmin && (
            <NavLink to="/admin" className={({ isActive }) => `nav-link${isActive ? ' active' : ''}`}>
              Espace Admin
            </NavLink>
          )}

          {user && (
            <button type="button" className="btn btn-outline btn-sm" onClick={handleSignOut}>
              Déconnexion
            </button>
          )}
        </nav>
      </div>
    </header>
  )
}
