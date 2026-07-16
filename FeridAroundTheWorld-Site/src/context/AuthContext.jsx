import { createContext, useCallback, useContext, useEffect, useState } from 'react'

const AuthContext = createContext(null)

const USERS_KEY = 'fatw_users'
const SESSION_KEY = 'fatw_session'

const SEED_USERS = [
  {
    id: 1,
    name: 'Admin Ferid',
    email: 'admin@ferid.app',
    password: 'Admin123!',
    role: 'admin',
    bio: "Je veille sur les quiz, les niveaux et les joueurs de Ferid Around the World.",
    avatar: '🧑‍💼'
  },
  {
    id: 2,
    name: 'Souhaila',
    email: 'souhaila@ferid.app',
    password: 'Player123!',
    role: 'player',
    bio: '',
    avatar: '⚽'
  },
  {
    id: 3,
    name: 'Yassine',
    email: 'yassine@ferid.app',
    password: 'Player123!',
    role: 'player',
    bio: '',
    avatar: '⚽'
  },
  {
    id: 4,
    name: 'Amira',
    email: 'amira@ferid.app',
    password: 'Player123!',
    role: 'player',
    bio: '',
    avatar: '⚽'
  },
  {
    id: 5,
    name: 'Karim',
    email: 'karim@ferid.app',
    password: 'Player123!',
    role: 'player',
    bio: '',
    avatar: '⚽'
  }
]

function loadUsers() {
  const raw = localStorage.getItem(USERS_KEY)
  if (raw) {
    try {
      return JSON.parse(raw)
    } catch {
      return SEED_USERS
    }
  }
  localStorage.setItem(USERS_KEY, JSON.stringify(SEED_USERS))
  return SEED_USERS
}

function saveUsers(users) {
  localStorage.setItem(USERS_KEY, JSON.stringify(users))
}

export function AuthProvider({ children }) {
  const [users, setUsers] = useState(loadUsers)
  const [currentUserId, setCurrentUserId] = useState(() => {
    const raw = localStorage.getItem(SESSION_KEY)
    return raw ? Number(raw) : null
  })

  useEffect(() => {
    saveUsers(users)
  }, [users])

  useEffect(() => {
    if (currentUserId === null) {
      localStorage.removeItem(SESSION_KEY)
    } else {
      localStorage.setItem(SESSION_KEY, String(currentUserId))
    }
  }, [currentUserId])

  const user = users.find((u) => u.id === currentUserId) ?? null

  const signUp = useCallback(({ name, email, password }) => {
    const emailTaken = users.some((u) => u.email.toLowerCase() === email.toLowerCase())
    if (emailTaken) {
      return { ok: false, error: 'Un compte existe déjà avec cet email.' }
    }
    const nextId = users.length ? Math.max(...users.map((u) => u.id)) + 1 : 1
    const newUser = {
      id: nextId,
      name,
      email,
      password,
      role: 'player',
      bio: '',
      avatar: '⚽'
    }
    setUsers((prev) => [...prev, newUser])
    setCurrentUserId(nextId)
    return { ok: true, user: newUser }
  }, [users])

  const signIn = useCallback(({ email, password }) => {
    const found = users.find(
      (u) => u.email.toLowerCase() === email.toLowerCase() && u.password === password
    )
    if (!found) {
      return { ok: false, error: 'Email ou mot de passe incorrect.' }
    }
    setCurrentUserId(found.id)
    return { ok: true, user: found }
  }, [users])

  const signOut = useCallback(() => {
    setCurrentUserId(null)
  }, [])

  const updateProfile = useCallback((patch) => {
    if (!currentUserId) return
    setUsers((prev) => prev.map((u) => (u.id === currentUserId ? { ...u, ...patch } : u)))
  }, [currentUserId])

  const value = {
    user,
    users,
    setUsers,
    isAuthenticated: Boolean(user),
    isAdmin: user?.role === 'admin',
    signUp,
    signIn,
    signOut,
    updateProfile
  }

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth() {
  const ctx = useContext(AuthContext)
  if (!ctx) {
    throw new Error('useAuth must be used within an AuthProvider')
  }
  return ctx
}
