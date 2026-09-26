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
        <h1>Agregar Administrador</h1>
        <p class="subtitle">
          Completá los datos del nuevo administrador
        </p>
      </header>

      <!-- FORM -->
      <form class="form" @submit.prevent="guardarAdministrador">

        <!-- ROW 1 -->
        <div class="form-row">
          <div class="field">
            <label for="nombre">Nombre</label>
            <input id="nombre" v-model="admin.nombre" type="text" required />
          </div>

          <div class="field">
            <label for="apellido">Apellido</label>
            <input id="apellido" v-model="admin.apellido" type="text" required />
          </div>
        </div>

        <!-- ROW 2 -->
        <div class="form-row">
          <div class="field">
            <label for="email">Email</label>
            <input id="email" v-model="admin.email" type="email" required />
          </div>

          <div class="field">
            <label for="rol">Rol</label>
            <select id="rol" v-model="admin.role" required>
              <option>Admin</option>
              <option>Secretaria</option>
              <option>Director</option>
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
            {{ loading ? 'Guardando...' : 'Guardar administrador' }}
          </button>

          <router-link
            to="/administracion"
            class="btn btn-secondary"
          >
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
import { useAuth } from '@/composables/useAuth'

const router = useRouter()
const { authHeaders } = useAuth()
const API = import.meta.env.VITE_API_URL

const loading = ref(false)
const error   = ref<string | null>(null)

const admin = reactive({
  nombre:      '',
  apellido:    '',
  email:       '',
  role:        'Admin',
  passwordTemp: 'Cambiar1234!'   // contraseña temporal que el admin debe cambiar
})

const guardarAdministrador = async () => {
  error.value = null

  if (!admin.nombre.trim() || !admin.apellido.trim()) {
    error.value = 'Nombre y apellido son obligatorios'
    return
  }
  if (!admin.email.trim()) {
    error.value = 'El email es obligatorio'
    return
  }

  loading.value = true

  try {
    const res = await fetch(`${API}/api/administradores`, {
      method:  'POST',
      headers: authHeaders(),
      body:    JSON.stringify(admin)
    })

    if (!res.ok) {
      const data = await res.json().catch(() => ({}))
      throw new Error(data.error ?? `Error HTTP ${res.status}`)
    }

    router.push('/administracion')
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'No se pudo guardar el administrador'
  } finally {
    loading.value = false
  }
}
</script>
