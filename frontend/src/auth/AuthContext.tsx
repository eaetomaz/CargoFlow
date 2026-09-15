import { createContext, useCallback, useContext, useEffect, useState, type ReactNode } from 'react'
import { api, auth } from '../api/client'
import type { User } from '../api/types'

interface AuthContextValue {
  user: User | null
  loading: boolean
  login: (username: string, password: string) => Promise<void>
  logout: () => void
}

const AuthContext = createContext<AuthContextValue | null>(null)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    if (!auth.getToken()) {
      setLoading(false)
      return
    }

    api
      .me()
      .then(setUser)
      .catch(() => auth.clearToken())
      .finally(() => setLoading(false))
  }, [])

  const login = useCallback(async (username: string, password: string) => {
    const response = await api.login(username, password)
    auth.setToken(response.token)
    setUser(response.user)
  }, [])

  const logout = useCallback(() => {
    auth.clearToken()
    setUser(null)
  }, [])

  return <AuthContext.Provider value={{ user, loading, login, logout }}>{children}</AuthContext.Provider>
}

export function useAuth() {
  const context = useContext(AuthContext)
  if (!context) throw new Error('useAuth precisa estar dentro de um AuthProvider.')
  return context
}
