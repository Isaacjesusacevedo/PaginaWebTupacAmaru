import { apiGet, apiPost, apiPut, apiDelete } from '@/composables/useApiFetch'

export const carreraApi = {
  getAll: () => apiGet('/api/carreras'),
  getById: (id) => apiGet(`/api/carreras/${id}`),
  create: (data) => apiPost('/api/carreras', data),
  update: (id, data) => apiPut(`/api/carreras/${id}`, data),
  delete: (id) => apiDelete(`/api/carreras/${id}`)
}