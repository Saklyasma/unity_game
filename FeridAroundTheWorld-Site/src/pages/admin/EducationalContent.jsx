import CrudSection from '../../components/ui/CrudSection.jsx'
import { CONTENT_KEY, CONTENT_SEED } from '../../data/seeds.js'

const COLUMNS = [
  { key: 'title', label: 'Titre' },
  { key: 'country', label: 'Pays' },
  { key: 'funFact', label: 'Anecdote' }
]

const FIELDS = [
  { name: 'title', label: 'Titre', type: 'text' },
  { name: 'country', label: 'Pays concerné', type: 'text' },
  { name: 'funFact', label: 'Anecdote éducative', type: 'textarea' }
]

export default function EducationalContent() {
  return (
    <CrudSection
      icon="📚"
      title="Mettre à jour le contenu éducatif"
      description="Gère les anecdotes et fiches culturelles affichées entre les matchs."
      storageKey={CONTENT_KEY}
      seed={CONTENT_SEED}
      columns={COLUMNS}
      fields={FIELDS}
    />
  )
}
