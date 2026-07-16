// Tiny, dependency-free NLP: TF-IDF vectorization + cosine similarity.
// This is the whole "AI" in the chatbot — no external API, no network call, runs entirely
// in the browser. It's a classic information-retrieval algorithm, not a trained model.

const STOPWORDS = new Set([
  'le', 'la', 'les', 'de', 'des', 'du', 'un', 'une', 'et', 'ou', 'a', 'à', 'est', 'es',
  'tu', 'je', 'qu', 'que', 'qui', 'ce', 'cette', 'ca', 'ça', 'pour', 'avec', 'mon', 'ma',
  'mes', 'ton', 'ta', 'tes', 'il', 'elle', 'on', 'nous', 'vous', 'ils', 'elles', 'se', 'sur'
])

function stripAccents(text) {
  return text.normalize('NFD').replace(/[̀-ͯ]/g, '')
}

export function tokenize(text) {
  return stripAccents(text.toLowerCase())
    .replace(/[^a-z0-9\s]/g, ' ')
    .split(/\s+/)
    .filter((word) => word.length > 1 && !STOPWORDS.has(word))
}

function termFrequencies(tokens) {
  const tf = new Map()
  tokens.forEach((token) => tf.set(token, (tf.get(token) ?? 0) + 1))
  return tf
}

/** Builds IDF weights from a corpus of documents (each document = array of tokens). */
function buildIdf(documents) {
  const idf = new Map()
  const docCount = documents.length
  const vocabulary = new Set(documents.flat())

  vocabulary.forEach((term) => {
    const containing = documents.filter((doc) => doc.includes(term)).length
    idf.set(term, Math.log((docCount + 1) / (containing + 1)) + 1)
  })

  return idf
}

function tfIdfVector(tokens, idf) {
  const tf = termFrequencies(tokens)
  const vector = new Map()
  tf.forEach((count, term) => {
    if (idf.has(term)) {
      vector.set(term, count * idf.get(term))
    }
  })
  return vector
}

function cosineSimilarity(vectorA, vectorB) {
  let dot = 0
  let normA = 0
  let normB = 0

  vectorA.forEach((weight, term) => {
    normA += weight * weight
    if (vectorB.has(term)) {
      dot += weight * vectorB.get(term)
    }
  })
  vectorB.forEach((weight) => {
    normB += weight * weight
  })

  if (normA === 0 || normB === 0) return 0
  return dot / (Math.sqrt(normA) * Math.sqrt(normB))
}

/**
 * Finds the best-matching document for a query among a list of raw-text documents.
 * Returns { index, score } — index is -1 if nothing scores above zero.
 */
export function findBestMatch(query, documents) {
  const tokenizedDocs = documents.map(tokenize)
  const idf = buildIdf(tokenizedDocs)
  const queryVector = tfIdfVector(tokenize(query), idf)

  let bestIndex = -1
  let bestScore = 0

  tokenizedDocs.forEach((docTokens, index) => {
    const docVector = tfIdfVector(docTokens, idf)
    const score = cosineSimilarity(queryVector, docVector)
    if (score > bestScore) {
      bestScore = score
      bestIndex = index
    }
  })

  return { index: bestIndex, score: bestScore }
}
