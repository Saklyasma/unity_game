// Small local quiz bank used by the chatbot's "Quiz surprise" feature.
// Grading happens client-side (deterministic), so accuracy tracked in
// useChatProgress is always exact — the AI is only used for commentary.
export const CHAT_QUIZ_BANK = [
  {
    id: 'geo-1',
    category: 'geography',
    question: "Quelle est la capitale du Brésil ?",
    options: ['Rio de Janeiro', 'Brasília', 'São Paulo', 'Salvador'],
    correctIndex: 1
  },
  {
    id: 'geo-2',
    category: 'geography',
    question: 'Le Colisée du jeu se trouve dans quel pays ?',
    options: ['Chine', 'Corée du Sud', 'Japon', 'Thaïlande'],
    correctIndex: 2
  },
  {
    id: 'geo-3',
    category: 'geography',
    question: "Combien d'îles compte approximativement le Japon ?",
    options: ['680', '1 800', '6 800', '68 000'],
    correctIndex: 2
  },
  {
    id: 'flags-1',
    category: 'flags',
    question: 'Le drapeau tricolore vert-jaune-bleu est celui de quel pays ?',
    options: ['Brésil', 'Sénégal', 'Jamaïque', 'Gabon'],
    correctIndex: 0
  },
  {
    id: 'flags-2',
    category: 'flags',
    question: "Quel pays d'Afrique du Nord a un drapeau rouge avec une étoile et un croissant blancs ?",
    options: ['Maroc', 'Algérie', 'Tunisie', 'Libye'],
    correctIndex: 2
  },
  {
    id: 'football-1',
    category: 'football',
    question: 'Combien de Coupes du Monde le Brésil a-t-il remportées ?',
    options: ['3', '4', '5', '6'],
    correctIndex: 2
  },
  {
    id: 'football-2',
    category: 'football',
    question: 'Un match de football dure normalement combien de minutes (hors prolongations) ?',
    options: ['60', '80', '90', '120'],
    correctIndex: 2
  }
]

export function pickRandomQuestion(excludeId) {
  const pool = excludeId
    ? CHAT_QUIZ_BANK.filter((q) => q.id !== excludeId)
    : CHAT_QUIZ_BANK
  return pool[Math.floor(Math.random() * pool.length)]
}
