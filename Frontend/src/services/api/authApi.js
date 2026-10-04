import { apiFetch } from '@/composables/useApiFetch'

/**
 * POST helper que devuelve el JSON crudo (con el wrapper { isSuccess, data }).
 * Lanza Error si la respuesta HTTP no es OK o si isSuccess === false.
 */
async function postJson(endpoint, body) {
  const response = await apiFetch(endpoint, {
    method: 'POST',
    body: JSON.stringify(body)
  })

  let json = null
  try {
    json = await response.json()
  } catch {
    json = null
  }

  if (!response.ok || json?.isSuccess === false) {
    throw new Error(json?.message ?? `Error HTTP ${response.status}`)
  }

  return json
}

export const authApi = {
  login: (credentials) => postJson('/api/auth/login', credentials),

  verifyPassword: (credentials) => postJson('/api/auth/verify-password', credentials),

  setupAdmin: (data) => postJson('/api/setup/admin', data),

  setupStatus: async () => {
    const response = await apiFetch('/api/setup/status')
    return await response.json()
  }
}
