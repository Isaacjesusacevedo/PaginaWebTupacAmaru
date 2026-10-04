import { apiFetch } from '@/composables/useApiFetch'

export const authApi = {
  login: (credentials) => apiFetch('/api/auth/login', {
    method: 'POST',
    body: JSON.stringify(credentials)
  }),
  
  verifyPassword: (credentials) => apiFetch('/api/auth/verify-password', {
    method: 'POST',
    body: JSON.stringify(credentials)
  }),

  setupAdmin: (data) => apiFetch('/api/setup/admin', {
    method: 'POST',
    body: JSON.stringify(data)
  }),

  setupStatus: () => apiFetch('/api/setup/status')
}