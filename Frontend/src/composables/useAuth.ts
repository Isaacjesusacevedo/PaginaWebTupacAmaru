const TOKEN_KEY = 'auth_token'
const ADMIN_KEY = 'auth_admin'

export interface AdminSession {
  id: number
  nombre: string
  apellido: string
  email: string
  role: string
}

export function useAuth() {
  const getToken = (): string | null =>
    sessionStorage.getItem(TOKEN_KEY)

  const getAdmin = (): AdminSession | null => {
    const raw = sessionStorage.getItem(ADMIN_KEY)
    return raw ? (JSON.parse(raw) as AdminSession) : null
  }

  const isAuthenticated = (): boolean => !!getToken()

  const guardarSesion = (token: string, admin: AdminSession): void => {
    sessionStorage.setItem(TOKEN_KEY, token)
    sessionStorage.setItem(ADMIN_KEY, JSON.stringify(admin))
  }

  const cerrarSesion = (): void => {
    sessionStorage.removeItem(TOKEN_KEY)
    sessionStorage.removeItem(ADMIN_KEY)
  }

  /** Devuelve los headers necesarios para llamadas autenticadas. */
  const authHeaders = (): Record<string, string> => {
    const token = getToken()
    return token
      ? { 'Content-Type': 'application/json', Authorization: `Bearer ${token}` }
      : { 'Content-Type': 'application/json' }
  }

  return { getToken, getAdmin, isAuthenticated, guardarSesion, cerrarSesion, authHeaders }
}
