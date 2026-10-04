import { apiGet, apiPost, apiPut, apiDelete } from '@/composables/useApiFetch'

export const formularioApi = {
  getAll: () => apiGet('/api/formularios'),
  getById: (id) => apiGet(`/api/formularios/${id}`),
  create: (data) => apiPost('/api/formularios', data),
  update: (id, data) => apiPut(`/api/formularios/${id}`, data),
  delete: (id) => apiDelete(`/api/formularios/${id}`)
}