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
        <h1>Editar Administrador</h1>
        <p class="subtitle">
          Modificá los datos del administrador
        </p>
      </header>

      <!-- FORM -->
      <form class="form" @submit.prevent="guardarAdministrador">

        <div class="form-row">
          <div class="field">
            <label for="nombre">Nombre</label>
            <input
              id="nombre"
              v-model="admin.nombre"
              type="text"
              required
            />
          </div>

          <div class="field">
            <label for="apellido">Apellido</label>
            <input
              id="apellido"
              v-model="admin.apellido"
              type="text"
              required
            />
          </div>
        </div>

        <div class="form-row">
          <div class="field">
            <label for="email">Email</label>
            <input
              id="email"
              v-model="admin.email"
              type="email"
              required
            />
          </div>

          <div class="field">
            <label for="rol">Rol</label>
            <select id="rol" v-model="admin.roll" required>
              <option>Admin</option>
              <option>SuperAdmin</option>
            </select>
          </div>
        </div>

        <!-- ACTIONS -->
        <div class="form-actions">
          <button
            class="btn btn-success"
            type="submit"
            :disabled="loading"
          >
            {{ loading ? 'Guardando...' : 'Guardar cambios' }}
          </button>

          <router-link to="/administracion" class="btn btn-secondary">
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

interface Administrador {
  id: number
  nombre: string
  apellido: string
  email: string
  roll: string
}

const admin = reactive<Administrador>({
  id: 0,
  nombre: '',
  apellido: '',
  email: '',
  roll: 'Admin'
})

// 🔹 cargar administrador por ID
onMounted(async () => {
  const id = Number(route.params.id)

  if (!id || isNaN(id)) {
    error.value = 'ID inválido'
    return
  }

  try {
    const res = await fetch(`http://localhost:5089/api/administradores/${id}`)
    if (!res.ok) throw new Error(`Error HTTP ${res.status}`)

    const data = await res.json()
    Object.assign(admin, data)
  } catch {
    error.value = 'No se pudieron cargar los datos del administrador'
  }
})

// 🔹 guardar cambios
const guardarAdministrador = async () => {
  loading.value = true
  error.value = null

  try {
    const res = await fetch(
      `http://localhost:5089/api/administradores/${admin.id}`,
      {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(admin)
      }
    )

    if (!res.ok) {
      const msg = await res.text()
      throw new Error(msg || 'Error al actualizar')
    }

    router.push('/administracion')
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Error al actualizar administrador'
  } finally {
    loading.value = false
  }
}
</script>
