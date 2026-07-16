import { useCallback, useEffect, useState } from 'react'

function readCollection(key, seed) {
  const raw = localStorage.getItem(key)
  if (raw) {
    try {
      return JSON.parse(raw)
    } catch {
      return seed
    }
  }
  localStorage.setItem(key, JSON.stringify(seed))
  return seed
}

/** Mocked CRUD storage — every "admin" entity in this demo lives in localStorage, no backend involved. */
export function useLocalStorageCollection(key, seed) {
  const [items, setItems] = useState(() => readCollection(key, seed))

  useEffect(() => {
    localStorage.setItem(key, JSON.stringify(items))
  }, [key, items])

  const add = useCallback((item) => {
    setItems((prev) => {
      const nextId = prev.length ? Math.max(...prev.map((i) => i.id)) + 1 : 1
      return [...prev, { id: nextId, ...item }]
    })
  }, [])

  const update = useCallback((id, patch) => {
    setItems((prev) => prev.map((item) => (item.id === id ? { ...item, ...patch } : item)))
  }, [])

  const remove = useCallback((id) => {
    setItems((prev) => prev.filter((item) => item.id !== id))
  }, [])

  return [items, { add, update, remove }]
}
