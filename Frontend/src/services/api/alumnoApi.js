import { apiGet, apiPost, apiPut, apiDelete } from '@/composables/useApiFetch'

export const alumnoApi = {
  getAll: () => apiGet('/api/alumnos'),
  getById: (id) => apiGet(`/api/alumnos/${id}`),
  create: (data) => apiPost('/api/alumnos', data),
  update: (id, data) => apiPut(`/api/alumnos/${id}`, data),
  delete: (id) => apiDelete(`/api/alumnos/${id}`),
  inscribir: (data) => apiPost('/api/inscripcion', data),
  getCarreras: () => apiGet('/api/carreras')
}