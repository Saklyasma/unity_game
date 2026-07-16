import { Link } from 'react-router-dom'

const FEATURES = [
  {
    icon: '🧠',
    title: 'Quiz géographiques',
    text: "Réponds à des quiz sur les pays, drapeaux et capitales pour débloquer de nouveaux terrains."
  },
  {
    icon: '🌍',
    title: 'Niveaux autour du monde',
    text: 'Chaque niveau de jeu se déroule dans un pays différent, avec son propre décor et ses défis.'
  },
  {
    icon: '⚡',
    title: 'Power-ups à débloquer',
    text: "Gagne des super-pouvoirs (tir puissant, super vitesse...) en progressant dans l'aventure."
  },
  {
    icon: '📚',
    title: 'Contenu éducatif',
    text: 'Des fiches et anecdotes culturelles apparaissent entre les matchs pour apprendre en jouant.'
  },
  {
    icon: '🏆',
    title: 'Statistiques de jeu',
    text: 'Suis ta progression, tes scores et compare-toi aux autres jeunes joueurs.'
  },
  {
    icon: '👤',
    title: 'Profil personnalisé',
    text: 'Crée ton profil joueur et retrouve ta progression sur tous tes appareils.'
  }
]

export default function Home() {
  return (
    <>
      <section className="hero">
        <div className="container hero-grid" style={{ display: 'contents' }}>
          <div>
            <span className="hero-eyebrow">⚽ Jeu éducatif de football</span>
            <h1>Fais le tour du monde, un match à la fois.</h1>
            <p>
              Ferid Around the World est un jeu de football éducatif : découvre des pays, réponds à des
              quiz de géographie et débloque des niveaux partout dans le monde.
            </p>
            <div className="hero-actions">
              <Link to="/sign-up" className="btn btn-primary">
                🚀 Devenir joueur
              </Link>
              <a href="#discover" className="btn btn-outline">
                🔎 Découvrir le jeu
              </a>
            </div>
          </div>
          <div className="hero-art" aria-hidden="true">
            🌍⚽
          </div>
        </div>
      </section>

      <section id="discover" className="section container">
        <div className="section-header">
          <span className="badge badge-blue">Discover the Game Features</span>
          <h2>Ce que tu vas découvrir en jouant</h2>
          <p>Un mélange de football, de géographie et de petits défis pour apprendre en s'amusant.</p>
        </div>

        <div className="card-grid">
          {FEATURES.map((feature) => (
            <div className="card" key={feature.title}>
              <div className="card-icon">{feature.icon}</div>
              <h3>{feature.title}</h3>
              <p>{feature.text}</p>
            </div>
          ))}
        </div>
      </section>

      <section className="section container">
        <div className="download-strip" id="download">
          <div>
            <h2>Télécharge l'application dès maintenant</h2>
            <p>Disponible sur mobile — crée ton compte joueur et commence ton aventure autour du monde.</p>
          </div>
          <div className="store-buttons">
            <a className="store-btn" href="#download" onClick={(e) => e.preventDefault()}>
              📱 Google Play
            </a>
            <a className="store-btn" href="#download" onClick={(e) => e.preventDefault()}>
              🍎 App Store
            </a>
          </div>
        </div>
      </section>
    </>
  )
}
