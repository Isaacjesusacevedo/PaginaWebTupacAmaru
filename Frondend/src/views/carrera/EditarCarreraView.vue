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
        <h1>Editar Carrera</h1>
        <p class="subtitle">Modificá los datos de la carrera</p>
      </header>

      <!-- FORM -->
      <form class="form" @submit.prevent="guardarCarrera">

        <div class="form-row">
          <div class="field">
            <label for="nombre">Nombre</label>
            <input
              id="nombre"
              v-model="carrera.nombre"
              type="text"
              required
            />
          </div>

          <div class="field">
            <label for="duracion">Duración (años)</label>
            <input
              id="duracion"
              v-model.number="carrera.duracionAnios"
              type="number"
              min="1"
              required
            />
          </div>
        </div>

        <div class="form-row">
          <div class="field">
            <label for="turno">Turno</label>
            <select id="turno" v-model="carrera.turno" required>
              <option>Mañana</option>
              <option>Tarde</option>
              <option>Noche</option>
            </select>
          </div>

          <div class="field">
            <label for="modalidad">Modalidad</label>
            <select id="modalidad" v-model="carrera.modalidad" required>
              <option>Presencial</option>
              <option>Virtual</option>
              <option>Mixta</option>
            </select>
          </div>
        </div>

        <div class="form-row">
          <div class="field">
            <label for="horario">Horario</label>
            <input
              id="horario"
              v-model="carrera.horario"
              type="text"
              placeholder="Ej: 18:00 a 22:00"
            />
          </div>

          <div class="field">
            <label for="estado">Estado</label>
            <select id="estado" v-model="carrera.estado" required>
              <option>Activa</option>
              <option>Inactiva</option>
            </select>
          </div>
        </div>

        <!-- ACTIONS -->
        <div class="form-actions">
          <button
            type="submit"
            class="btn btn-success"
            :disabled="loading"
          >
            {{ loading ? 'Guardando...' : 'Guardar cambios' }}
          </button>

          <router-link to="/carreras" class="btn btn-secondary">
            Cancelar
          </router-link>
        </div>

        <!-- ERROR -->
        <p v-if="error" class="form-error text-center">
          {{ error }}
        </p>

      </form>
    </div>
  </main>
</template>

<script setup lang="ts">
import { reactive, ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'

const route = useRoute()
const router = useRouter()

const loading = ref(false)
const error = ref<string | null>(null)

interface Carrera {
  id: number
  nombre: string
  duracionAnios: number
  turno: string
  modalidad: string
  horario: string
  estado: string
}

const carrera = reactive<Carrera>({
  id: 0,
  nombre: '',
  duracionAnios: 1,
  turno: 'Mañana',
  modalidad: 'Presencial',
  horario: '',
  estado: 'Activa'
})

// 🔹 cargar carrera por ID
onMounted(async () => {
  const id = Number(route.params.id)

  if (!id || isNaN(id)) {
    error.value = 'ID inválido'
    return
  }

  try {
    const res = await fetch(`http://localhost:5089/api/carreras/${id}`)
    if (!res.ok) throw new Error(`Error HTTP ${res.status}`)

    const data = await res.json()
    Object.assign(carrera, data)
  } catch {
    error.value = 'No se pudieron cargar los datos de la carrera'
  }
})


// 🔹 guardar cambios
const guardarCarrera = async () => {
  loading.value = true
  error.value = null

  const id = Number(route.params.id)

  try {
    const res = await fetch(
      `http://localhost:5089/api/carreras/${id}`,
      {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ ...carrera, id }) // aseguramos el id
      }
    )

    if (!res.ok) {
      const msg = await res.text()
      throw new Error(msg || `Error HTTP ${res.status}`)
    }

    router.push('/carreras')
  } catch (err) {
    error.value =
      err instanceof Error
        ? err.message
        : 'Error al actualizar la carrera'
  } finally {
    loading.value = false
  }
}
</script>
