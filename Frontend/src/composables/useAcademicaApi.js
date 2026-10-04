/**
 * Cliente para la API de información académica e inscripción (Instituto.MinimalAPI.Academica).
 * Desenvuelve { isSuccess, message, data } y lanza el message cuando la respuesta falla.
 * Maneja 401/403 redirigiendo al login (igual que useApiFetch).
 */

import { useRouter } from 'vue-router'

let router = null

export function setRouter(r) {
  router = r
}

function getApiBaseUrl() {
  return import.meta.env.VITE_API_ACADEMICA_URL ?? 'http://localhost:5128'
}

function getToken() {
  const token = sessionStorage.getItem('auth_token')
  if (!token || token === 'undefined' || token === 'null' || token.length < 20) {
    return null
  }
  return token
}

async function academicaFetch(endpoint, options = {}) {
  const token = getToken()

  const headers = {
    'Content-Type': 'application/json',
    ...(options.headers || {}),
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
    console.warn(`[academicaFetch] ${response.status} en ${endpoint}`)

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

  let json = null
  try {
    json = await response.json()
  } catch {
    json = null
  }

  if (!response.ok || json?.isSuccess === false) {
    throw new Error(json?.message ?? `Error HTTP ${response.status}`)
  }

  return json?.data
}

export function academicaGet(endpoint) {
  return academicaFetch(endpoint)
}

export function academicaPost(endpoint, body) {
  return academicaFetch(endpoint, {
    method: 'POST',
    body: JSON.stringify(body),
  })
}

export function academicaPut(endpoint, body) {
  return academicaFetch(endpoint, {
    method: 'PUT',
    body: JSON.stringify(body),
  })
}

export function academicaDelete(endpoint) {
  return academicaFetch(endpoint, {
    method: 'DELETE',
  })
}