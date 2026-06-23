import { createContext, useContext, useState, ReactNode } from 'react'

export type UserRole = 'Employee' | 'Manager' | 'HR'

export interface AuthUser {
  employeeId: string
  name: string
  email: string
  role: UserRole
}

interface AuthContextValue {
  user: AuthUser | null
  login: (username: string, password: string) => Promise<{ ok: boolean; error?: string }>
  logout: () => void
}

const AuthContext = createContext<AuthContextValue | null>(null)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(() => {
    const saved = sessionStorage.getItem('auth_user')
    return saved ? JSON.parse(saved) : null
  })

  const login = async (username: string, password: string) => {
    try {
      const res = await fetch('/api/v1/auth/login', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ username, password }),
      })
      const json = await res.json()
      if (!res.ok || !json.success) {
        return { ok: false, error: json.message ?? 'Username หรือ Password ไม่ถูกต้อง' }
      }
      const u: AuthUser = {
        employeeId: json.data.employeeId,
        name: json.data.fullNameTh,
        email: json.data.email,
        role: json.data.role,
      }
      sessionStorage.setItem('auth_user', JSON.stringify(u))
      setUser(u)
      return { ok: true }
    } catch {
      return { ok: false, error: 'ไม่สามารถเชื่อมต่อ Server ได้' }
    }
  }

  const logout = () => {
    sessionStorage.removeItem('auth_user')
    setUser(null)
  }

  return (
    <AuthContext.Provider value={{ user, login, logout }}>
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth() {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used inside AuthProvider')
  return ctx
}
