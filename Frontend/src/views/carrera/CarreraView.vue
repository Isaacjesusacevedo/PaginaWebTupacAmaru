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
        <h1>Gestión de Carreras</h1>
        <p class="subtitle">
          Administrá las carreras del sistema
        </p>
      </header>

      <!-- TABLE -->
      <div class="table">

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

        <!-- TABLE ROWS -->
        <div
          v-for="carrera in carreras"
          :key="carrera.id"
          class="table-row table-cols-default"
        >
          <span>{{ carrera.nombre }}</span>
          <span>{{ carrera.duracionAnios }} años</span>
          <span>{{ carrera.turno }}</span>
          <span>{{ carrera.modalidad }}</span>
          <span>{{ carrera.horario }}</span>
          <span>{{ carrera.estado }}</span>

          <div class="table-actions center">
            <router-link
              class="btn btn-success"
              :to="`/editarcarrera/${carrera.id}`"
            >
              <el-icon><Edit /></el-icon>
            </router-link>

            <router-link
              class="btn btn-danger"
              :to="`/eliminarcarreras/${carrera.id}`"
            >
              <el-icon><Delete /></el-icon>
            </router-link>
          </div>
        </div>

        <div v-if="carreras.length === 0" class="table-row table-cols-default">
          <span class="text-center" style="grid-column: 1 / -1;">No hay carreras registradas</span>
        </div>

      </div>

      <!-- ERROR -->
      <p v-if="error" class="text-center text-danger">
        {{ error }}
      </p>

      <!-- FOOTER CTA -->
      <footer class="table-actions center">
        <router-link class="btn btn-primary" to="/agregarcarreras">
          Agregar carrera
        </router-link>
      </footer>

    </div>
  </main>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { apiGet } from '@/composables/useApiFetch'

interface Carrera {
  id: number
  nombre: string
  duracionAnios: number
  turno: string
  modalidad: string
  horario: string
  estado: string
}

const carreras = ref<Carrera[]>([])
const error = ref<string | null>(null)

const cargarCarreras = async () => {
  try {
    carreras.value = await apiGet<Carrera[]>('/api/carreras')
  } catch (err: unknown) {
    console.error('Error al cargar carreras:', err)
    error.value = err instanceof Error
      ? `No se pudieron cargar las carreras: ${err.message}`
      : 'No se pudieron cargar las carreras'
  }
}

onMounted(cargarCarreras)
</script>
