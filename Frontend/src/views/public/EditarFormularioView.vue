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
        <h1>Editar Formulario</h1>
        <p class="subtitle">Modificá los datos del formulario</p>
      </header>

      <!-- FORM -->
      <form class="form" @submit.prevent="guardarFormulario">

        <div class="form-row">
          <div class="field">
            <label for="nombre">Nombre</label>
            <input
              id="nombre"
              v-model="formulario.nombre"
              type="text"
              required
            />
          </div>
        </div>

        <div class="form-row">
          <div class="field">
            <label for="estado">Estado</label>
            <select id="estado" v-model="formulario.estado" required>
              <option value="Borrador">Borrador</option>
              <option value="Abierto">Abierto</option>
              <option value="Cerrado">Cerrado</option>
            </select>
          </div>
        </div>

        <div class="form-row">
          <div class="field">
            <label for="fechaApertura">Fecha de apertura</label>
            <input
              id="fechaApertura"
              v-model="formulario.fechaApertura"
              type="date"
              required
            />
          </div>

          <div class="field">
            <label for="fechaCierre">Fecha de cierre</label>
            <input
              id="fechaCierre"
              v-model="formulario.fechaCierre"
              type="date"
              required
            />
          </div>
        </div>

        <div class="form-row">
          <div class="field">
            <label for="descripcion">Descripción</label>
            <textarea
              id="descripcion"
              v-model="formulario.descripcion"
              rows="4"
            ></textarea>
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

          <router-link to="/formularios" class="btn btn-secondary">
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
import { useAuth } from '@/composables/useAuth'

const route  = useRoute()
const router = useRouter()
const { authHeaders } = useAuth()
const API = import.meta.env.VITE_API_URL

const loading = ref(false)
const error = ref<string | null>(null)

interface Formulario {
  id: number
  nombre: string
  estado: 'Borrador' | 'Abierto' | 'Cerrado'
  fechaApertura: string
  fechaCierre: string
  descripcion: string
}

const formulario = reactive<Formulario>({
  id: 0,
  nombre: '',
  estado: 'Borrador',
  fechaApertura: '',
  fechaCierre: '',
  descripcion: ''
})

// Cargar datos del formulario
onMounted(async () => {
  const id = Number(route.params.id)

  if (!id || isNaN(id)) {
    error.value = 'ID inválido'
    return
  }

  try {
    const res = await fetch(`${API}/api/formularios/${id}`)
    if (!res.ok) throw new Error(`Error HTTP ${res.status}`)

    const data = await res.json()
    Object.assign(formulario, data)
  } catch {
    error.value = 'No se pudieron cargar los datos del formulario'
  }
})

const guardarFormulario = async () => {
  loading.value = true
  error.value = null

  const id = Number(route.params.id)

  try {
    const res = await fetch(`${API}/api/formularios/${id}`, {
      method:  'PUT',
      headers: authHeaders(),
      body:    JSON.stringify({ ...formulario, id })
    })

    if (!res.ok) {
      const msg = await res.text()
      throw new Error(msg || `Error HTTP ${res.status}`)
    }

    router.push('/formularios')
  } catch (err) {
    error.value =
      err instanceof Error
        ? err.message
        : 'Error al actualizar el formulario'
  } finally {
    loading.value = false
  }
}
</script>