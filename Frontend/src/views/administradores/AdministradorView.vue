<template>
  <main class="section">
    <div class="card card-center">

      <!-- Banner -->
      <img
        class="card-media"
        src="@/components/Banner/bannerProfesor.jpg"
        alt="Banner profesor"
      />

      <!-- Header -->
      <header class="text-center">
        <h1>Gestión de Administradores</h1>
        <p class="subtitle">
          Administrá los administradores del sistema
        </p>
      </header>

      <!-- TABLE -->
      <div class="table">

        <!-- TABLE HEADER -->
        <div class="table-header table-cols-admin">
          <span>Nombre</span>
          <span>Apellido</span>
          <span>Email</span>
          <span>Rol</span>
          <span>Acciones</span>
        </div>

        <!-- TABLE ROWS -->
        <div
          v-for="admin in administradores"
          :key="admin.id"
          class="table-row table-cols-admin"
        >
          <span>{{ admin.nombre }}</span>
          <span>{{ admin.apellido }}</span>
          <span>{{ admin.email }}</span>
          <span>{{ admin.role }}</span>

          <div class="table-actions center">
            <router-link
              class="btn btn-success"
              :to="`/editaradministrador/${admin.id}`"
            >
              <el-icon><Edit /></el-icon>
            </router-link>

            <router-link
              class="btn btn-danger"
              :to="`/eliminaradministrador/${admin.id}`"
            >
              <el-icon><Delete /></el-icon>
            </router-link>
          </div>
        </div>

        <!-- EMPTY STATE -->
        <div v-if="!administradores.length && !error" class="table-row table-cols-admin">
          <span class="table-empty-state">No hay administradores registrados</span>
        </div>

        <!-- ERROR STATE -->
        <div v-if="error" class="table-row table-cols-admin">
          <span class="table-empty-state" style="color: var(--color-danger);">{{ error }}</span>
        </div>

      </div>

      <!-- FOOTER CTA -->
      <footer class="table-actions center">
        <router-link
          to="/agregaradministracion"
          class="btn btn-primary"
        >
          Agregar administrador
        </router-link>
      </footer>

    </div>
  </main>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useAuth } from '@/composables/useAuth'

interface Administrador {
  id: number
  nombre: string
  apellido: string
  email: string
  role: string   // corregido: era "roll"
}

const { authHeaders } = useAuth()
const API = import.meta.env.VITE_API_URL

const administradores = ref<Administrador[]>([])
const loading = ref(true)
const error   = ref<string | null>(null)

const cargarAdministradores = async () => {
  try {
    const res = await fetch(`${API}/api/administradores`, { headers: authHeaders() })
    if (!res.ok) throw new Error(`Error HTTP ${res.status}`)
    administradores.value = await res.json()
  } catch (err) {
    console.error(err)
    error.value = 'No se pudieron cargar los administradores'
  } finally {
    loading.value = false
  }
}

onMounted(cargarAdministradores)
</script>
