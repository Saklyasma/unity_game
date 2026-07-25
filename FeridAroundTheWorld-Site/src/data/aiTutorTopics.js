/**
 * Display labels/icons for the 9 fixed AI Tutor topics (must match Models/AiTutorTopics.cs keys
 * on the backend exactly). Client-side formatting only — mastery/scores are never computed here.
 */
export const AI_TUTOR_TOPICS = [
  { key: 'FifaWorldCup', label: 'FIFA World Cup', icon: '🏆' },
  { key: 'ChampionsLeague', label: 'Champions League', icon: '⭐' },
  { key: 'PremierLeague', label: 'Premier League', icon: '🏴' },
  { key: 'LaLiga', label: 'La Liga', icon: '🇪🇸' },
  { key: 'SerieA', label: 'Serie A', icon: '🇮🇹' },
  { key: 'Bundesliga', label: 'Bundesliga', icon: '🇩🇪' },
  { key: 'FootballLegends', label: 'Football Legends', icon: '👟' },
  { key: 'NationalTeams', label: 'National Teams', icon: '🌍' },
  { key: 'RulesOfFootball', label: 'Rules of Football', icon: '📖' }
]

export function topicLabel(topicKey) {
  return AI_TUTOR_TOPICS.find((t) => t.key === topicKey)?.label ?? topicKey
}
