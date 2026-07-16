// Intent bank for the chatbot's NLP matching (see src/lib/textSimilarity.js).
// Each intent has example phrases used only to train the TF-IDF matcher — the user's
// free-text message is compared against every example across every intent, and whichever
// example scores highest (cosine similarity) decides the intent.
export const CHAT_INTENTS = [
  {
    id: 'greeting',
    examples: ['bonjour', 'salut', 'hello', 'coucou', 'bonsoir', 'salut ça va', 'hey']
  },
  {
    id: 'start_quiz',
    examples: [
      'pose moi une question',
      'quiz',
      'quiz surprise',
      'teste moi',
      'je veux jouer',
      'interroge moi',
      'donne moi une question',
      'on joue'
    ]
  },
  {
    id: 'ask_progress',
    examples: [
      'quel est mon niveau',
      'montre moi ma progression',
      'où j en suis',
      'mon score',
      'mes statistiques',
      'comment je progresse',
      'affiche mes stats'
    ]
  },
  {
    id: 'ask_weakness',
    examples: [
      'qu est ce que je dois réviser',
      'mon point faible',
      'je suis nul en quoi',
      'que dois je améliorer',
      'sur quoi je suis mauvais',
      'donne moi un conseil de révision'
    ]
  },
  {
    id: 'ask_hint',
    examples: [
      'j ai besoin d un indice',
      'aide moi',
      'je suis bloqué',
      'donne moi un indice',
      'un indice s il te plait',
      'je ne sais pas répondre'
    ]
  },
  {
    id: 'thanks',
    examples: ['merci', 'merci beaucoup', 'cool merci', 'super merci', 'top']
  }
]

export const FALLBACK_SCORE_THRESHOLD = 0.12
