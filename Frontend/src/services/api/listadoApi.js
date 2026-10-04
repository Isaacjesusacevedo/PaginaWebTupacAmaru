import { apiGet } from '@/composables/useApiFetch'

export const listadoApi = {
  getAll: () => apiGet('/api/listado')
}