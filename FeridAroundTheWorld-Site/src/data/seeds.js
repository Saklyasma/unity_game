export const GAME_LEVELS_KEY = 'fatw_game_levels'
export const POWER_UPS_KEY = 'fatw_power_ups'
export const CONTENT_KEY = 'fatw_content'

export const GAME_LEVELS_SEED = [
  { id: 1, name: 'Stade de Tunis', worldRegion: 'Afrique du Nord', difficulty: 'Facile', unlockScore: 0 },
  { id: 2, name: 'Arène de Rio', worldRegion: 'Amérique du Sud', difficulty: 'Moyen', unlockScore: 500 },
  { id: 3, name: 'Colisée de Tokyo', worldRegion: 'Asie', difficulty: 'Difficile', unlockScore: 1500 }
]

export const POWER_UPS_SEED = [
  { id: 1, name: 'Tir Fusée', effect: 'Triple la puissance de tir', duration: '10s', rarity: 'Rare' },
  { id: 2, name: 'Super Vitesse', effect: 'Double la vitesse de course', duration: '8s', rarity: 'Commun' },
  { id: 3, name: 'Bouclier Ferid', effect: "Bloque un but adverse", duration: '15s', rarity: 'Légendaire' }
]

export const CONTENT_SEED = [
  { id: 1, title: 'Le saviez-vous : Brésil', country: 'Brésil', funFact: 'Le Brésil a remporté 5 Coupes du Monde.' },
  { id: 2, title: 'Le saviez-vous : Japon', country: 'Japon', funFact: "Le Japon compte plus de 6800 îles." }
]
