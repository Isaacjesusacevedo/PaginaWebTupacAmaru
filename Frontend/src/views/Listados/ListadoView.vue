<template>
  <main class="section">
    <div class="card card-center card-xl">

      <img
        class="card-media"
        src="@/components/Banner/bannerProfesor.jpg"
        alt="Panel de administración"
      />

      <section class="text-center">
        <h1>Listas de Carreras con alumnos inscriptos</h1>
        <p class="subtitle">
          Revisá los alumnos inscriptos por carrera
        </p>
      </section>

      <div class="table">

        <div class="table-header table-cols-listado">
  <span>Alumno</span>
  <span>DNI</span>
  <span>Edad</span>
  <span>Carrera</span>
  <span>Turno</span>
  <span>Fecha de egreso</span>
  <span>Info académica</span>
</div>

<div
  v-for="item in listado"
  :key="item.alumnoId"
  class="table-row table-cols-listado"
>
  <span>{{ item.nombreCompleto }}</span>
  <span>{{ item.dni }}</span>
  <span>{{ item.edad }}</span>
  <span>{{ item.carrera }}</span>
  <span>{{ item.turno }}</span>
  <span>{{ formatFecha(item.fechaEgreso) || '—' }}</span>
  <span>{{ formatInfoAcademica(item) }}</span>
</div>
        <div v-if="listado.length === 0" class="table-row table-cols-listado">
          <span class="text-center" style="grid-column: 1 / -1;">No hay alumnos inscriptos</span>
        </div>

      </div>

      <p v-if="error" class="error-msg text-center">
        {{ error }}
      </p>

    </div>
  </main>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { listadoApi } from '@/services/api'

const listado = ref([])
const loading = ref(true)
const error = ref(null)

function formatFecha(fechaIso) {
  if (!fechaIso) return ''
  const fecha = new Date(fechaIso)
  return Number.isNaN(fecha.getTime()) ? '' : fecha.toLocaleDateString('es-AR')
}

function formatInfoAcademica(item) {
  const flags = []
  if (item.poseeTitulo) flags.push('Título')
  if (item.tituloEnTramite) flags.push('En trámite')
  if (item.consMaterias) flags.push('Const. materias')
  if (item.consAlumnoRegular) flags.push('Const. regular')
  return flags.length ? flags.join(' · ') : '—'
}

const cargarListado = async () => {
  try {
    listado.value = await listadoApi.getAll()
  } catch (err) {
    console.error(err)
    error.value = err.message ?? 'No se pudo cargar el listado'
  } finally {
    loading.value = false
  }
}

onMounted(cargarListado)
</script>
