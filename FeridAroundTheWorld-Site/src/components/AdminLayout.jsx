import { NavLink, Outlet } from 'react-router-dom'
import { useAuth } from '../context/AuthContext.jsx'

const LINKS = [
  { to: '/admin', label: 'Tableau de bord', icon: '🏠', end: true },
  { to: '/admin/countries', label: 'Gérer les pays', icon: '🌐' },
  { to: '/admin/quizzes', label: 'Gérer les quiz', icon: '🧠' },
  { to: '/admin/bot-stats', label: 'IA adverses (Bot Stats)', icon: '🤖' },
  { to: '/admin/teams', label: 'Équipes', icon: '🏆' },
  { to: '/admin/matches', label: 'Matchs', icon: '📅' },
  { to: '/admin/predictions', label: 'Prédictions', icon: '🔮' },
  { to: '/admin/game-levels', label: 'Gérer les niveaux', icon: '🌍' },
  { to: '/admin/power-ups', label: 'Configurer les power-ups', icon: '⚡' },
  { to: '/admin/statistics', label: 'Statistiques joueurs', icon: '📊' },
  { to: '/admin/users', label: 'Comptes utilisateurs', icon: '👥' },
  { to: '/admin/content', label: 'Contenu éducatif', icon: '📚' },
  { to: '/admin/profile', label: 'Mon profil', icon: '🙍' }
]

export default function AdminLayout() {
  const { user } = useAuth()

  return (
    <div className="admin-shell">
      <aside className="admin-sidebar">
        <div className="admin-sidebar-title">Espace admin — {user?.name}</div>
        {LINKS.map((link) => (
          <NavLink
            key={link.to}
            to={link.to}
            end={link.end}
            className={({ isActive }) => `admin-nav-link${isActive ? ' active' : ''}`}
          >
            <span>{link.icon}</span>
            {link.label}
          </NavLink>
        ))}
      </aside>

      <main className="admin-main">
        <Outlet />
      </main>
    </div>
  )
}
