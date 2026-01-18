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
            <input id="rol" v-model="admin.roll" type="text" required />
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

  const router = useRouter()
  const loading = ref(false)
  const error = ref<string | null>(null)

  const admin = reactive({
    nombre: '',
    apellido: '',
    email: '',
    roll: ''
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
      const res = await fetch('http://localhost:5089/api/administradores', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(admin)
      })

      if (!res.ok) {
        throw new Error(`Error HTTP ${res.status}`)
      }

      alert('Administrador guardado correctamente')
      router.push('/administracion')
    } catch (err) {
      console.error(err)
      error.value = 'No se pudo guardar el administrador'
    } finally {
      loading.value = false
    }
  }
  </script>
