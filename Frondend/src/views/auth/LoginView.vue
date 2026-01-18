<template>
  <main class="section">
    <div class="card card-center card-lg">

      <!-- MEDIA -->
      <img
        class="card-media"
        src="@/components/Banner/bannerProfesor.jpg"
        alt="Banner profesor"
      />

      <!-- HEADER -->
      <header class="text-center">
        <h1>Iniciar sesión</h1>
        <p class="subtitle">
          Accedé al panel de administración
        </p>
      </header>

      <!-- FORM -->
      <form class="form" @submit.prevent="login">

        <div class="form-row">
          <div class="field">
            <label>Usuario</label>
            <input type="text" v-model="usuario" required />
          </div>

          <div class="field">
            <label>Contraseña</label>
            <input type="password" v-model="password" required />
          </div>
        </div>

        <button
          type="submit"
          class="btn btn-primary btn-block"
          :disabled="loading"
        >
          {{ loading ? 'Ingresando...' : 'Ingresar' }}
        </button>

        <p v-if="error" class="error-msg text-center">
          {{ error }}
        </p>

      </form>

    </div>
  </main>
</template>

<script setup lang="ts">
  import { ref } from 'vue'

  const usuario = ref('')
  const password = ref('')
  const loading = ref(false)
  const error = ref<string | null>(null)

  const login = () => {
    error.value = null
    loading.value = true

    setTimeout(() => {
      if (!usuario.value || !password.value) {
        error.value = 'Completá usuario y contraseña'
      }
      loading.value = false
    }, 800)
  }
  </script>
