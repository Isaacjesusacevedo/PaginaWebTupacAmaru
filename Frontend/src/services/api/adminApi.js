import { apiGet, apiPost, apiPut, apiDelete } from '@/composables/useApiFetch'

export const adminApi = {
  getAll: () => apiGet('/api/administradores'),
  getById: (id) => apiGet(`/api/administradores/${id}`),
  create: (dto) => apiPost('/api/administradores/with-password', dto),
  update: (id, data) => apiPut(`/api/administradores/${id}`, data),
  changePassword: (id, dto) => apiPut(`/api/administradores/${id}/password`, dto),
  delete: (id) => apiDelete(`/api/administradores/${id}`)
}