import CrudSection from '../../components/ui/CrudSection.jsx'
import { POWER_UPS_KEY, POWER_UPS_SEED } from '../../data/seeds.js'

const COLUMNS = [
  { key: 'name', label: 'Power-up' },
  { key: 'effect', label: 'Effet' },
  { key: 'duration', label: 'Durée' },
  {
    key: 'rarity',
    label: 'Rareté',
    render: (item) => <span className="badge badge-blue">{item.rarity}</span>
  }
]

const FIELDS = [
  { name: 'name', label: 'Nom du power-up', type: 'text' },
  { name: 'effect', label: 'Effet', type: 'text' },
  { name: 'duration', label: 'Durée (ex: 10s)', type: 'text' },
  { name: 'rarity', label: 'Rareté', type: 'select', options: ['Commun', 'Rare', 'Légendaire'] }
]

export default function ConfigurePowerUps() {
  return (
    <CrudSection
      icon="⚡"
      title="Configurer les power-ups"
      description="Définis les bonus temporaires que les joueurs peuvent débloquer en jouant."
      storageKey={POWER_UPS_KEY}
      seed={POWER_UPS_SEED}
      columns={COLUMNS}
      fields={FIELDS}
    />
  )
}
