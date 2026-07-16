import ApiCrudSection from '../../components/ui/ApiCrudSection.jsx'
import { countriesApi } from '../../api/countries.js'

const CONTINENTS = ['Africa', 'America', 'Oceania', 'Europe', 'Asia']

const COLUMNS = [
  { key: 'name', label: 'Nom' },
  { key: 'code', label: 'Code' },
  { key: 'flagEmoji', label: 'Drapeau' },
  { key: 'continent', label: 'Continent' },
  {
    key: 'isUnlockedByDefault',
    label: 'Débloqué par défaut',
    render: (item) => (
      <span className={`badge ${item.isUnlockedByDefault ? 'badge-blue' : 'badge-gray'}`}>
        {item.isUnlockedByDefault ? 'Oui' : 'Non'}
      </span>
    )
  }
]

const FIELDS = [
  { name: 'name', label: 'Nom du pays', type: 'text' },
  { name: 'code', label: 'Code (3 lettres)', type: 'text' },
  { name: 'flagEmoji', label: 'Drapeau (emoji)', type: 'text' },
  {
    name: 'continent',
    label: 'Continent',
    type: 'select',
    options: CONTINENTS.map((c) => ({ value: c, label: c }))
  },
  { name: 'isUnlockedByDefault', label: 'Débloqué par défaut', type: 'checkbox' }
]

export default function ManageCountries() {
  return (
    <ApiCrudSection
      icon="🌐"
      title="Gérer les pays"
      description="Pays jouables (progression Ferid) — CRUD complet consommant l'API Countries (MongoDB WorldCupApiDb)."
      api={countriesApi}
      columns={COLUMNS}
      fields={FIELDS}
    />
  )
}
