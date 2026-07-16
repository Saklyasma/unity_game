import { useCallback, useEffect, useState } from 'react'

/** Loads a list from the given API service and exposes CRUD helpers that refetch after each write. */
export function useApiResource(api) {
  const [items, setItems] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(null)

  const refetch = useCallback(async () => {
    setLoading(true)
    setError(null)
    try {
      setItems(await api.list())
    } catch (err) {
      setError(err.message)
    } finally {
      setLoading(false)
    }
  }, [api])

  useEffect(() => {
    refetch()
  }, [refetch])

  const create = useCallback(
    async (payload) => {
      await api.create(payload)
      await refetch()
    },
    [api, refetch]
  )

  const update = useCallback(
    async (id, payload) => {
      await api.update(id, payload)
      await refetch()
    },
    [api, refetch]
  )

  const remove = useCallback(
    async (id) => {
      await api.remove(id)
      await refetch()
    },
    [api, refetch]
  )

  return { items, loading, error, refetch, create, update, remove }
}
