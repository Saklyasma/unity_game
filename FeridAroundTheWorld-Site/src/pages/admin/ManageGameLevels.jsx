import CrudSection from '../../components/ui/CrudSection.jsx'
import { GAME_LEVELS_KEY, GAME_LEVELS_SEED } from '../../data/seeds.js'

const COLUMNS = [
  { key: 'name', label: 'Niveau' },
  { key: 'worldRegion', label: 'Région du monde' },
  {
    key: 'difficulty',
    label: 'Difficulté',
    render: (item) => <span className="badge badge-yellow">{item.difficulty}</span>
  },
  { key: 'unlockScore', label: 'Score requis' }
]

const FIELDS = [
  { name: 'name', label: 'Nom du niveau', type: 'text' },
  { name: 'worldRegion', label: 'Région du monde', type: 'text' },
  { name: 'difficulty', label: 'Difficulté', type: 'select', options: ['Facile', 'Moyen', 'Difficile'] },
  { name: 'unlockScore', label: 'Score requis pour débloquer', type: 'number' }
]

export default function ManageGameLevels() {
  return (
    <CrudSection
      icon="🌍"
      title="Gérer les niveaux de jeu"
      description="Ajoute ou ajuste les niveaux du jeu, répartis dans différentes régions du monde."
      storageKey={GAME_LEVELS_KEY}
      seed={GAME_LEVELS_SEED}
      columns={COLUMNS}
      fields={FIELDS}
    />
  )
}
