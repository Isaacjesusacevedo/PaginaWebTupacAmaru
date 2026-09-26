<template>
  <main class="section">
    <div class="card card-center">

      <!-- MEDIA -->
      <img
        class="card-media"
        src="@/components/Banner/bannerProfesor.jpg"
        alt="Banner formulario"
      />

      <!-- HEADER -->
      <header class="text-center">
        <h1>Eliminar Formulario</h1>
        <p class="subtitle">
          ¿Estás seguro de que deseas eliminar este formulario?
        </p>
      </header>

      <!-- TABLE -->
      <div v-if="formulario" class="table">

        <!-- TABLE HEADER -->
        <div class="table-header table-cols-formularios">
          <span>Nombre</span>
          <span>Estado</span>
          <span>Fecha de apertura</span>
          <span>Fecha de cierre</span>
          <span>Acciones</span>
        </div>

        <!-- TABLE ROW -->
        <div class="table-row table-cols-formularios">
          <span>{{ formulario.nombre }}</span>
          <span>
            <span class="estado-badge" :class="getEstadoClass(formulario.estado)">
              {{ formulario.estado }}
            </span>
          </span>
          <span>{{ formulario.fechaApertura }}</span>
          <span>{{ formulario.fechaCierre }}</span>

          <div class="table-actions center">
            <button
              class="btn btn-danger"
              @click="eliminarFormulario"
              :disabled="loading"
            >
              {{ loading ? 'Eliminando...' : 'Eliminar' }}
            </button>

            <router-link to="/formularios" class="btn btn-secondary">
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

interface Formulario {
  id: number
  nombre: string
  estado: 'Abierto' | 'Cerrado' | 'Borrador'
  fechaApertura: string
  fechaCierre: string
  descripcion: string
}

const router = useRouter()
const route  = useRoute()
const { authHeaders } = useAuth()
const API = import.meta.env.VITE_API_URL

const loading = ref(false)
const error = ref<string | null>(null)
const formulario = ref<Formulario | null>(null)
const formularioId = ref<number>(0)

onMounted(() => {
  if (route.params.id) {
    formularioId.value = Number(route.params.id)
    cargarFormulario()
  }
})

// Cargar el formulario
const cargarFormulario = async () => {
  try {
    const res = await fetch(`${API}/api/formularios/${formularioId.value}`)
    if (!res.ok) throw new Error(`Error HTTP ${res.status}`)
    formulario.value = await res.json()
  } catch (err: unknown) {
    console.error(err)
    if (err instanceof Error) {
      error.value = err.message
    } else {
      error.value = 'No se pudo cargar el formulario'
    }
  }
}

// Eliminar el formulario
const eliminarFormulario = async () => {
  if (!formularioId.value) return

  loading.value = true
  error.value = null

  try {
    const res = await fetch(`${API}/api/formularios/${formularioId.value}`, {
      method:  'DELETE',
      headers: authHeaders()
    })

    if (!res.ok) {
      const msg = await res.text()
      throw new Error(msg)
    }

    router.push('/formularios')
  } catch (err) {
    error.value = err instanceof Error
      ? err.message
      : 'Error al eliminar el formulario'
  } finally {
    loading.value = false
  }
}

const getEstadoClass = (estado: string): string => {
  switch (estado) {
    case 'Abierto': return 'estado-abierto'
    case 'Cerrado': return 'estado-cerrado'
    case 'Borrador': return 'estado-borrador'
    default: return 'estado-cerrado'
  }
}
</script>