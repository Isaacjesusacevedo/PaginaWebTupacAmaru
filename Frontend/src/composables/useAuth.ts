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
  /**
   * Obtiene el token de sessionStorage.
   * Filtra tokens inválidos ("undefined", "null", vacíos o demasiado cortos).
   */
  const getToken = (): string | null => {
    const token = sessionStorage.getItem(TOKEN_KEY)
    if (!token || token === 'undefined' || token === 'null' || token.length < 20) {
      return null
    }
    return token
  }

  const getAdmin = (): AdminSession | null => {
    const raw = sessionStorage.getItem(ADMIN_KEY)
    if (!raw || raw === 'undefined' || raw === 'null') return null
    try {
      return JSON.parse(raw) as AdminSession
    } catch {
      return null
    }
  }

  const isAuthenticated = (): boolean => !!getToken()

  const guardarSesion = (token: string, admin: AdminSession): void => {
    // Validación defensiva: no guardar si el token es inválido
    if (!token || token === 'undefined' || token === 'null' || token.length < 20) {
      console.warn('[useAuth] Intentando guardar un token inválido:', token)
      return
    }
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
