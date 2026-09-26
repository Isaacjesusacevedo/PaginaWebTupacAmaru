<template>
  <main class="section">
    <div class="card card-center card-lg">

      <!-- MEDIA -->
      <img
        class="card-media"
        src="@/components/Banner/bannerProfesor.jpg"
        alt="Panel de administración"
      />

      <!-- HEADER -->
      <section class="text-center">
        <h1>Listas de Carreras con alumnos inscriptos</h1>
        <p class="subtitle">
          Revisá los alumnos inscriptos por carrera
        </p>
      </section>

      <!-- TABLE -->
      <div class="table">

        <!-- TABLE HEADER -->
<div class="table-header table-cols-default">
  <span>Alumno</span>
  <span>DNI</span>
  <span>Edad</span>
  <span>Carrera</span>
  <span>Turno</span>
  <span></span>
  <span></span>
</div>

<!-- TABLE ROW -->
<div
  v-for="item in listado"
  :key="item.alumnoId"
  class="table-row table-cols-default"
>
  <span>{{ item.nombreCompleto }}</span>
  <span>{{ item.dni }}</span>
  <span>{{ item.edad }}</span>
  <span>{{ item.carrera }}</span>
  <span>{{ item.turno }}</span>
  <span></span>
  <span></span>
</div>


      </div>

      <!-- ERROR -->
      <p v-if="error" class="error-msg text-center">
        {{ error }}
      </p>

    </div>
  </main>
</template>


<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useAuth } from '@/composables/useAuth'

interface AlumnoListado {
  alumnoId: number
  nombreCompleto: string
  dni: number
  email: string
  carrera: string
  turno: string
  edad: number
}

const { authHeaders } = useAuth()
const API = import.meta.env.VITE_API_URL

const listado = ref<AlumnoListado[]>([])
const loading = ref(true)
const error   = ref<string | null>(null)

const cargarListado = async () => {
  try {
    const res = await fetch(`${API}/api/listado`, { headers: authHeaders() })
    if (!res.ok) throw new Error(`HTTP ${res.status}`)
    listado.value = await res.json()
  } catch (err) {
    console.error(err)
    error.value = 'No se pudo cargar el listado'
  } finally {
    loading.value = false
  }
}

onMounted(cargarListado)
</script>
