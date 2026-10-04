import { apiGet } from '@/composables/useApiFetch'

export const statsApi = {
  getAll: () => apiGet('/api/stats')
}