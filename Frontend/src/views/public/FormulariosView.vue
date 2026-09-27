<template>
  <main class="section">
    <div class="card card-center">

      <!-- MEDIA -->
      <img
        class="card-media"
        src="@/components/Banner/bannerProfesor.jpg"
        alt="Banner formularios"
      />

      <!-- HEADER -->
      <header class="text-center">
        <h1>Lista de Formularios</h1>
        <p class="subtitle">
          Gestión de formularios académicos
        </p>
      </header>

      <!-- TABLE -->
      <div class="table">

        <!-- TABLE HEADER -->
        <div class="table-header table-cols-formularios">
          <span>Nombre</span>
          <span>Estado</span>
          <span>Fecha de apertura</span>
          <span>Fecha de cierre</span>
          <span>Acciones</span>
        </div>

        <!-- TABLE ROWS -->
        <div
          v-for="formulario in formularios"
          :key="formulario.id"
          class="table-row table-cols-formularios"
        >
          <span>{{ formulario.nombre }}</span>
          <span>
            <span class="estado-badge" :class="getEstadoClass(formulario.estado)">
              {{ formulario.estado }}
            </span>
          </span>
          <span>{{ formulario.fechaApertura }}</span>
          <span>{{ formulario.fechaCierre }}</span>

          <div class="table-actions center">
            <router-link
              class="btn btn-primary"
              :to="`/editarformulario/${formulario.id}`"
            >
              <el-icon><Edit /></el-icon>
            </router-link>

            <button
              class="btn btn-danger"
              @click="eliminar(formulario.id)"
            >
              <el-icon><Delete /></el-icon>
            </button>
          </div>
        </div>

        <div v-if="formularios.length === 0" class="table-row table-cols-formularios">
          <span class="text-center" style="grid-column: 1 / -1;">No hay formularios registrados</span>
        </div>

      </div>

      <!-- ERROR -->
      <p v-if="error" class="text-center" style="color: var(--color-danger);">
        {{ error }}
      </p>

      <!-- FOOTER CTA -->
      <footer class="table-actions center">
        <router-link class="btn btn-primary" to="/agregarformulario">
          Agregar formulario
        </router-link>
      </footer>

    </div>
  </main>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useAuth } from '@/composables/useAuth'

interface Formulario {
  id: number
  nombre: string
  estado: 'Abierto' | 'Cerrado' | 'Borrador'
  fechaApertura: string
  fechaCierre: string
}

const { authHeaders } = useAuth()
const API = import.meta.env.VITE_API_URL

const formularios = ref<Formulario[]>([])
const error = ref<string | null>(null)

const cargarFormularios = async () => {
  try {
    const res = await fetch(`${API}/api/formularios`, { headers: authHeaders() })
    if (!res.ok) throw new Error(`Error HTTP ${res.status}`)
    formularios.value = await res.json()
  } catch (err: unknown) {
    console.error('Error al cargar formularios:', err)
    if (err instanceof Error) {
      error.value = `No se pudieron cargar los formularios: ${err.message}`
    } else {
      error.value = 'No se pudieron cargar los formularios'
    }
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

const eliminar = (id: number) => {
  if (confirm('¿Eliminar este formulario?')) {
    alert(`Eliminar formulario ${id} - pendiente`)
  }
}

onMounted(cargarFormularios)
</script>