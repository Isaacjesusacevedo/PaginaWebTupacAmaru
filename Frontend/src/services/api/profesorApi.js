import { apiGet, apiPost, apiPut, apiDelete } from '@/composables/useApiFetch'

export const profesorApi = {
  getAll: () => apiGet('/api/profesores'),
  getById: (id) => apiGet(`/api/profesores/${id}`),
  create: (data) => apiPost('/api/profesores', data),
  update: (id, data) => apiPut(`/api/profesores/${id}`, data),
  delete: (id) => apiDelete(`/api/profesores/${id}`)
}