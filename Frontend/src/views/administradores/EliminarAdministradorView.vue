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
        <h1>Eliminar Administrador</h1>
        <p class="subtitle">
          ¿Estás seguro de que deseas eliminar este administrador?
        </p>
      </header>

      <!-- TABLE -->
      <div v-if="admin" class="table">

        <!-- TABLE HEADER -->
        <div class="table-header table-cols-admin">
          <span>Nombre</span>
          <span>Apellido</span>
          <span>Email</span>
          <span>Rol</span>
          <span>Acciones</span>
        </div>

        <!-- TABLE ROW -->
        <div class="table-row table-cols-admin">
          <span>{{ admin.nombre }}</span>
          <span>{{ admin.apellido }}</span>
          <span>{{ admin.email }}</span>
          <span>{{ admin.roll }}</span>

          <div class="table-actions center">
            <button
              class="btn btn-danger"
              @click="eliminarAdministrador"
              :disabled="loading"
            >
              {{ loading ? 'Eliminando...' : 'Eliminar' }}
            </button>

            <router-link to="/administracion" class="btn btn-secondary">
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

  interface Administrador {
    id: number
    nombre: string
    apellido: string
    email: string
    roll: string
  }

  const router = useRouter()
  const route = useRoute()

  const loading = ref(false)
  const error = ref<string | null>(null)
  const admin = ref<Administrador | null>(null)
  const adminId = ref<number>(0)

  // Obtener ID
  onMounted(() => {
    if (route.params.id) {
      adminId.value = Number(route.params.id)
      cargarAdministrador()
    }
  })

  // Cargar administrador
  const cargarAdministrador = async () => {
    try {
      const res = await fetch(
        `http://localhost:5089/api/administradores/${adminId.value}`
      )

      if (!res.ok) throw new Error(`Error HTTP ${res.status}`)
      admin.value = await res.json()
    } catch (err) {
      error.value = err instanceof Error
        ? err.message
        : 'No se pudo cargar el administrador'
    }
  }

  // Eliminar administrador
  const eliminarAdministrador = async () => {
    if (!adminId.value) return

    loading.value = true
    error.value = null

    try {
      const res = await fetch(
        `http://localhost:5089/api/administradores/${adminId.value}`,
        { method: 'DELETE' }
      )

      if (!res.ok) {
        const msg = await res.text()
        throw new Error(msg)
      }

      router.push('/administracion')
    } catch (err) {
      error.value = err instanceof Error
        ? err.message
        : 'Error al eliminar el administrador'
    } finally {
      loading.value = false
    }
  }
  </script>
