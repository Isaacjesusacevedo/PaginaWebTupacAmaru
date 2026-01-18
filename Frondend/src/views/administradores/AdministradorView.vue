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
          <span>{{ admin.roll }}</span>

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

      </div>

      <!-- EMPTY STATE -->
      <p
        v-if="!administradores.length && !error"
        class="text-center text-muted"
      >
        No hay administradores registrados
      </p>

      <!-- ERROR -->
      <p
        v-if="error"
        class="text-center text-danger"
      >
        {{ error }}
      </p>

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

interface Administrador {
  id: number
  nombre: string
  apellido: string
  email: string
  roll: string
}

const administradores = ref<Administrador[]>([])
const error = ref<string | null>(null)

const cargarAdministradores = async () => {
  try {
    const res = await fetch('http://localhost:5089/api/administradores')
    if (!res.ok) throw new Error(`Error HTTP ${res.status}`)
    administradores.value = await res.json()
  } catch (err) {
    console.error(err)
    error.value = 'No se pudieron cargar los administradores'
  }
}

onMounted(cargarAdministradores)
</script>
