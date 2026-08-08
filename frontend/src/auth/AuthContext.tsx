import { createContext, useContext, useMemo, useState, type ReactNode } from 'react'
import { api } from '../lib/api'
import type { AuthResponse, CurrentUser } from '../types'

type AuthContextValue = {
  user: CurrentUser | null
  isAuthenticated: boolean
  login: (email: string, password: string) => Promise<void>
  register: (email: string, password: string, firstName: string, lastName: string) => Promise<void>
  logout: () => void
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined)

function saveAuth(response: AuthResponse) {
  localStorage.setItem('edumind.accessToken', response.accessToken)
  localStorage.setItem('edumind.refreshToken', response.refreshToken)
  localStorage.setItem('edumind.auth', JSON.stringify(response))
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const stored = localStorage.getItem('edumind.auth')
  const [user, setUser] = useState<CurrentUser | null>(() => {
    if (!stored) return null
    try {
      const parsed = JSON.parse(stored) as AuthResponse
      return { userId: parsed.userId, email: parsed.email, firstName: parsed.firstName, lastName: parsed.lastName, roles: parsed.roles }
    } catch {
      return null
    }
  })

  const value = useMemo<AuthContextValue>(() => ({
    user,
    isAuthenticated: Boolean(user),
    async login(email, password) {
      const { data } = await api.post<AuthResponse>('/auth/login', { email, password })
      saveAuth(data)
      setUser({ userId: data.userId, email: data.email, firstName: data.firstName, lastName: data.lastName, roles: data.roles })
    },
    async register(email, password, firstName, lastName) {
      const { data } = await api.post<AuthResponse>('/auth/register', { email, password, firstName, lastName })
      saveAuth(data)
      setUser({ userId: data.userId, email: data.email, firstName: data.firstName, lastName: data.lastName, roles: data.roles })
    },
    logout() {
      localStorage.removeItem('edumind.accessToken')
      localStorage.removeItem('edumind.refreshToken')
      localStorage.removeItem('edumind.auth')
      setUser(null)
    },
  }), [user])

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth() {
  const context = useContext(AuthContext)
  if (!context) throw new Error('useAuth must be used inside AuthProvider')
  return context
}
