<template>
  <main class="section">
    <div class="card card-center">

      <!-- MEDIA -->
      <img
        class="card-media"
        src="@/components/Banner/bannerProfesor.jpg"
        alt="Banner profesor"
      />

      <!-- HEADER -->
      <header class="text-center">
        <h1>Eliminar Carrera</h1>
        <p class="subtitle">
          ¿Estás seguro de que deseas eliminar esta carrera?
        </p>
      </header>

      <!-- TABLE -->
      <div v-if="carrera" class="table">

        <!-- TABLE HEADER -->
        <div class="table-header table-cols-default">
          <span>Nombre</span>
          <span>Duración</span>
          <span>Turno</span>
          <span>Modalidad</span>
          <span>Horario</span>
          <span>Estado</span>
          <span>Acciones</span>
        </div>

        <!-- TABLE ROW -->
        <div class="table-row table-cols-default">
          <span>{{ carrera.nombre }}</span>
          <span>{{ carrera.duracionAnios }} años</span>
          <span>{{ carrera.turno }}</span>
          <span>{{ carrera.modalidad }}</span>
          <span>{{ carrera.horario }}</span>
          <span>{{ carrera.estado }}</span>

          <div class="table-actions center">
            <button
              class="btn btn-danger"
              @click="eliminarCarrera"
              :disabled="loading"
            >
              {{ loading ? 'Eliminando...' : 'Eliminar' }}
            </button>

            <router-link to="/carreras" class="btn btn-secondary">
              Cancelar
            </router-link>
          </div>
        </div>

      </div>

      <!-- ERROR -->
      <p v-if="error" class="text-center text-danger">
        {{ error }}
      </p>

    </div>
  </main>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuth } from '@/composables/useAuth'

interface Carrera {
  id: number
  nombre: string
  duracionAnios: number
  turno: string
  modalidad: string
  horario: string
  estado: string
}

const router = useRouter()
const route  = useRoute()
const { authHeaders } = useAuth()
const API = import.meta.env.VITE_API_URL

const loading = ref(false)
const error = ref<string | null>(null)
const carrera = ref<Carrera | null>(null)
const carreraId = ref<number>(0)

onMounted(() => {
  if (route.params.id) {
    carreraId.value = Number(route.params.id)
    cargarCarrera()
  }
})

// Cargar la carrera
const cargarCarrera = async () => {
  try {
    const res = await fetch(`${API}/api/carreras/${carreraId.value}`)
    if (!res.ok) throw new Error(`Error HTTP ${res.status}`)
    carrera.value = await res.json()
  } catch (err: unknown) {
    console.error(err)
    if (err instanceof Error) {
      error.value = err.message
    } else {
      error.value = 'No se pudo cargar la carrera'
    }
  }
}

// Eliminar la carrera
const eliminarCarrera = async () => {
  if (!carreraId.value) return

  loading.value = true
  error.value = null

  try {
    const res = await fetch(`${API}/api/carreras/${carreraId.value}`, {
      method:  'DELETE',
      headers: authHeaders()
    })

    if (!res.ok) {
      const msg = await res.text()
      throw new Error(msg)
    }

    router.push('/carreras')
  } catch (err) {
    error.value = err instanceof Error
      ? err.message
      : 'Error al eliminar la carrera'
  } finally {
    loading.value = false
  }
}

</script>
