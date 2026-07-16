import { useEffect, useState } from 'react'
import ApiCrudSection from '../../components/ui/ApiCrudSection.jsx'
import { botStatsApi } from '../../api/botStats.js'
import { countriesApi } from '../../api/countries.js'

const COLUMNS = [
  { key: 'countryId', label: 'Pays (id)' },
  { key: 'difficulty', label: 'Difficulté' },
  { key: 'moveSpeed', label: 'Vitesse' },
  { key: 'kickForce', label: 'Force de tir' },
  { key: 'aggressive', label: 'Agressivité' }
]

/** Tuning for the Unity AI opponent (mirrors BotStatsData ScriptableObject) — full CRUD via /api/BotStats. */
export default function ManageBotStats() {
  const [countries, setCountries] = useState([])

  useEffect(() => {
    countriesApi.list().then(setCountries).catch(() => setCountries([]))
  }, [])

  const fields = [
    {
      name: 'countryId',
      label: 'Pays',
      type: 'select',
      numeric: true,
      options: countries.map((c) => ({ value: c.id, label: `${c.flagEmoji} ${c.name}` }))
    },
    { name: 'moveSpeed', label: 'Vitesse de déplacement', type: 'number', step: 0.1, min: 0.1, max: 50, defaultValue: 5.5 },
    { name: 'jumpForce', label: 'Force de saut', type: 'number', step: 0.1, min: 0.1, max: 50, defaultValue: 9 },
    { name: 'kickForce', label: 'Force de tir', type: 'number', step: 0.1, min: 0.1, max: 50, defaultValue: 14 },
    { name: 'kickRange', label: 'Portée de tir', type: 'number', step: 0.1, min: 0.1, max: 20, defaultValue: 2 },
    { name: 'difficulty', label: 'Difficulté (0 = facile, 1 = difficile)', type: 'number', step: 0.05, min: 0, max: 1, defaultValue: 0.7 },
    { name: 'pressureSpeedBoost', label: 'Bonus de vitesse sous pression', type: 'number', step: 0.05, min: 0.1, max: 5, defaultValue: 1.25 },
    { name: 'aggressive', label: 'Agressivité (0-1)', type: 'number', step: 0.05, min: 0, max: 1, defaultValue: 0.5 },
    { name: 'defensive', label: 'Esprit défensif (0-1)', type: 'number', step: 0.05, min: 0, max: 1, defaultValue: 0.3 },
    { name: 'possession', label: 'Rétention de balle (0-1)', type: 'number', step: 0.05, min: 0, max: 1, defaultValue: 0.4 }
  ]

  return (
    <ApiCrudSection
      icon="🤖"
      title="Configurer les IA adverses (Bot Stats)"
      description="Règle le comportement de l'adversaire contrôlé par l'IA pour chaque pays — CRUD complet consommant l'API BotStats."
      api={botStatsApi}
      columns={COLUMNS}
      fields={fields}
    />
  )
}
