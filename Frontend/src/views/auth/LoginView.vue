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
        <p class="subtitle">Accedé al panel de administración</p>
      </header>

      <!-- FORM -->
      <form class="form" @submit.prevent="login">

        <div class="form-row">
          <div class="field">
            <label for="email">Email</label>
            <input
              id="email"
              type="email"
              v-model="email"
              autocomplete="email"
              required
              :disabled="loading"
            />
          </div>

          <div class="field">
            <label for="password">Contraseña</label>
            <input
              id="password"
              type="password"
              v-model="password"
              autocomplete="current-password"
              required
              :disabled="loading"
            />
          </div>
        </div>

        <button
          type="submit"
          class="btn btn-primary btn-block"
          :disabled="loading"
        >
          {{ loading ? 'Ingresando...' : 'Ingresar' }}
        </button>

        <p v-if="error" class="error-msg text-center" role="alert">
          {{ error }}
        </p>

      </form>
    </div>
  </main>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuth } from '@/composables/useAuth'

const router   = useRouter()
const { guardarSesion } = useAuth()

const email    = ref('')
const password = ref('')
const loading  = ref(false)
const error    = ref<string | null>(null)

const API = import.meta.env.VITE_API_URL

const login = async () => {
  error.value   = null
  loading.value = true

  try {
    const res = await fetch(`${API}/api/auth/login`, {
      method:  'POST',
      headers: { 'Content-Type': 'application/json' },
      body:    JSON.stringify({ email: email.value, password: password.value })
    })

    const data = await res.json()

    if (!res.ok) {
      error.value = data.error ?? 'No se pudo iniciar sesión.'
      return
    }

    guardarSesion(data.token, data.admin)
    router.push('/')

  } catch {
    error.value = 'No se pudo conectar con el servidor.'
  } finally {
    loading.value = false
  }
}
</script>
