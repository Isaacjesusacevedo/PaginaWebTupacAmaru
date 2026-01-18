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
      <header class="card-header text-center">
        <h1>Agregar Carrera</h1>
        <p class="subtitle">
          Completá los datos de la nueva carrera
        </p>
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
            {{ loading ? 'Guardando...' : 'Guardar carrera' }}
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
  import { reactive, ref } from 'vue'
  import { useRouter } from 'vue-router'

  const router = useRouter()
  const loading = ref(false)
  const error = ref<string | null>(null)

  const carrera = reactive({
    nombre: '',
    duracionAnios: 0,
    turno: 'Mañana',
    modalidad: 'Presencial',
    horario: '',
    estado: 'Activa'
  })

  const guardarCarrera = async () => {
    error.value = null

    // Validación simple
    if (!carrera.nombre.trim()) {
      error.value = 'El nombre no puede estar vacío'
      return
    }
    if (carrera.duracionAnios <= 0) {
      error.value = 'La duración debe ser mayor a 0'
      return
    }

    loading.value = true

    try {
      const res = await fetch('http://localhost:5089/api/carreras', { // <- URL CORRECTA
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(carrera)
      })

      if (!res.ok) throw new Error(`Error HTTP ${res.status}`)

      alert('Carrera guardada correctamente')
      router.push('/carreras')
    } catch (err: unknown) {
      console.error(err)
      if (err instanceof Error) {
        error.value = err.message
      } else {
        error.value = 'Error al guardar la carrera'
      }
    } finally {
      loading.value = false
    }
  }
  </script>
