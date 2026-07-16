import { Link } from 'react-router-dom'
import { useAuth } from '../../context/AuthContext.jsx'
import { useLocalStorageCollection } from '../../hooks/useLocalStorageCollection.js'
import { useApiResource } from '../../hooks/useApiResource.js'
import { countriesApi } from '../../api/countries.js'
import { quizQuestionsApi } from '../../api/quizQuestions.js'
import { botStatsApi } from '../../api/botStats.js'
import {
  CONTENT_KEY,
  CONTENT_SEED,
  GAME_LEVELS_KEY,
  GAME_LEVELS_SEED,
  POWER_UPS_KEY,
  POWER_UPS_SEED
} from '../../data/seeds.js'

const SHORTCUTS = [
  { to: '/admin/countries', icon: '🌐', label: 'Gérer les pays' },
  { to: '/admin/quizzes', icon: '🧠', label: 'Gérer les quiz' },
  { to: '/admin/bot-stats', icon: '🤖', label: 'IA adverses (Bot Stats)' },
  { to: '/admin/matches', icon: '📅', label: 'Matchs' },
  { to: '/admin/predictions', icon: '🔮', label: 'Prédictions' },
  { to: '/admin/game-levels', icon: '🌍', label: 'Gérer les niveaux' },
  { to: '/admin/power-ups', icon: '⚡', label: 'Configurer les power-ups' },
  { to: '/admin/statistics', icon: '📊', label: 'Statistiques joueurs' },
  { to: '/admin/users', icon: '👥', label: 'Comptes utilisateurs' },
  { to: '/admin/content', icon: '📚', label: 'Contenu éducatif' }
]

export default function AdminDashboard() {
  const { user, users } = useAuth()
  const { items: countries } = useApiResource(countriesApi)
  const { items: quizQuestions } = useApiResource(quizQuestionsApi)
  const { items: botStats } = useApiResource(botStatsApi)
  const [levels] = useLocalStorageCollection(GAME_LEVELS_KEY, GAME_LEVELS_SEED)
  const [powerUps] = useLocalStorageCollection(POWER_UPS_KEY, POWER_UPS_SEED)
  const [content] = useLocalStorageCollection(CONTENT_KEY, CONTENT_SEED)
  const playerCount = users.filter((u) => u.role === 'player').length

  return (
    <div>
      <div className="admin-header">
        <div>
          <h1>Bonjour, {user.name} {user.avatar}</h1>
          <p>Voici un aperçu de Ferid Around the World.</p>
        </div>
      </div>

      <div className="stat-grid">
        <div className="stat-card">
          <div className="value">{countries.length}</div>
          <div className="label">Pays (API)</div>
        </div>
        <div className="stat-card">
          <div className="value">{quizQuestions.length}</div>
          <div className="label">Questions de quiz (API)</div>
        </div>
        <div className="stat-card">
          <div className="value">{botStats.length}</div>
          <div className="label">Profils IA (API)</div>
        </div>
        <div className="stat-card">
          <div className="value">{levels.length}</div>
          <div className="label">Niveaux de jeu</div>
        </div>
        <div className="stat-card">
          <div className="value">{powerUps.length}</div>
          <div className="label">Power-ups</div>
        </div>
        <div className="stat-card">
          <div className="value">{content.length}</div>
          <div className="label">Fiches éducatives</div>
        </div>
        <div className="stat-card">
          <div className="value">{playerCount}</div>
          <div className="label">Joueurs inscrits</div>
        </div>
      </div>

      <h2>Accès rapide</h2>
      <div className="card-grid">
        {SHORTCUTS.map((shortcut) => (
          <Link to={shortcut.to} className="card" key={shortcut.to} style={{ textDecoration: 'none' }}>
            <div className="card-icon">{shortcut.icon}</div>
            <h3 style={{ color: 'var(--color-text)' }}>{shortcut.label}</h3>
          </Link>
        ))}
      </div>
    </div>
  )
}
