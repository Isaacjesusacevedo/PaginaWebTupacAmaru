/**
 * Helper global para fetch con:
 * - Headers de autenticación automáticos
 * - Manejo centralizado de 401/403
 * - Auto-unwrap de respuestas { isSuccess, message, data }
 */

import { useRouter } from 'vue-router'

let router: ReturnType<typeof useRouter> | null = null

export function setRouter(r: ReturnType<typeof useRouter>) {
  router = r
}

/**
 * Obtiene el token de sessionStorage.
 * Filtra tokens inválidos ("undefined", "null", vacíos o demasiado cortos).
 */
function getToken(): string | null {
  const token = sessionStorage.getItem('auth_token')
  if (!token || token === 'undefined' || token === 'null' || token.length < 20) {
    return null
  }
  return token
}

function getApiBaseUrl(): string {
  return import.meta.env.VITE_API_URL ?? 'http://localhost:5127'
}

export async function apiFetch(endpoint: string, options: RequestInit = {}): Promise<Response> {
  const token = getToken()

  const headers: Record<string, string> = {
    'Content-Type': 'application/json',
    ...(options.headers as Record<string, string>),
  }

  if (token) {
    headers.Authorization = `Bearer ${token}`
  }

  const response = await fetch(`${getApiBaseUrl()}${endpoint}`, {
    ...options,
    headers,
  })

  // Manejo de 401/403: solo limpiar sesión si HABÍA un token y NO estamos en login
  if (response.status === 401 || response.status === 403) {
    console.warn(`[apiFetch] ${response.status} en ${endpoint}`)

    const estabaAutenticado = !!token
    const enLogin = router?.currentRoute.value.name === 'login'

    if (estabaAutenticado && !enLogin) {
      sessionStorage.removeItem('auth_token')
      sessionStorage.removeItem('auth_admin')

      try {
        const { ElMessage } = await import('element-plus')
        ElMessage.warning('Sesión expirada. Iniciá sesión nuevamente.')
      } catch { /* noop */ }

      router?.push({ name: 'login', query: { redirect: router.currentRoute.value.fullPath } })
    }
  }

  return response
}

/**
 * Interfaz del wrapper que devuelve el backend.
 */
interface ApiWrapper<T> {
  isSuccess: boolean
  message?: string
  data: T
}

/**
 * Desenvuelve una respuesta del backend.
 * Si viene envuelta como { isSuccess, message, data }, devuelve solo .data.
 */
function unwrap<T>(json: unknown): T {
  if (
    json !== null &&
    typeof json === 'object' &&
    'isSuccess' in json &&
    'data' in json
  ) {
    return (json as ApiWrapper<T>).data
  }
  return json as T
}

/**
 * Extrae el mensaje de error de una respuesta fallida.
 */
async function extractError(response: Response): Promise<string> {
  try {
    const err = (await response.json()) as { error?: string; message?: string }
    return err.error ?? err.message ?? `Error HTTP ${response.status}`
  } catch {
    return `Error HTTP ${response.status}`
  }
}

export async function apiGet<T>(endpoint: string): Promise<T> {
  const response = await apiFetch(endpoint)
  if (!response.ok) {
    throw new Error(await extractError(response))
  }
  const json = await response.json()
  return unwrap<T>(json)
}

export async function apiPost<T, B>(endpoint: string, body: B): Promise<T> {
  const response = await apiFetch(endpoint, {
    method: 'POST',
    body: JSON.stringify(body),
  })
  if (!response.ok) {
    throw new Error(await extractError(response))
  }
  const json = await response.json()
  return unwrap<T>(json)
}

export async function apiPut<T, B>(endpoint: string, body: B): Promise<T> {
  const response = await apiFetch(endpoint, {
    method: 'PUT',
    body: JSON.stringify(body),
  })
  if (!response.ok) {
    throw new Error(await extractError(response))
  }
  const json = await response.json()
  return unwrap<T>(json)
}

export async function apiDelete(endpoint: string): Promise<void> {
  const response = await apiFetch(endpoint, {
    method: 'DELETE',
  })
  if (!response.ok) {
    throw new Error(await extractError(response))
  }
}
