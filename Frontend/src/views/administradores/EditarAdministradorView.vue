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

          <div class="field password-field">
            <label for="nuevaPassword">Nueva Contraseña</label>
            <input
              id="nuevaPassword"
              v-model="nuevaPassword"
              :type="showNuevaPassword ? 'text' : 'password'"
              autocomplete="new-password"
              placeholder="Mínimo 8 caracteres (dejar vacío para no cambiar)"
              :disabled="loading"
            />
            <button
              type="button"
              class="password-toggle"
              @click="showNuevaPassword = !showNuevaPassword"
              :aria-label="showNuevaPassword ? 'Ocultar contraseña' : 'Mostrar contraseña'"
            >
              <el-icon v-if="showNuevaPassword"><Hide /></el-icon>
              <el-icon v-else><View /></el-icon>
            </button>
          </div>
        </div>

        <p v-if="errorPassword" class="form-error text-center">{{ errorPassword }}</p>

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

  <!-- RE-AUTENTICACIÓN MODAL (para entrar a editar) -->
  <el-dialog
    v-model="showReauthDialog"
    class="reauth-dialog"
    title="Verificación de Seguridad"
    :modal="true"
    :close-on-click-modal="false"
    :close-on-press-escape="false"
    :show-close="false"
    width="420"
    :before-close="handleReauthClose"
  >
    <template #header>
      <div class="dialog-header">
        <el-icon class="dialog-icon"><Lock /></el-icon>
        <span>Verificación de Seguridad</span>
      </div>
    </template>

    <div class="dialog-content">
      <p class="dialog-message">
        Para editar los datos de un administrador, por favor ingrese su contraseña actual.
      </p>

      <div class="form-row">
        <div class="field password-field">
          <label for="reauthPassword">Contraseña actual</label>
          <el-input
            id="reauthPassword"
            v-model="reauthPassword"
            :show-password="true"
            type="password"
            autocomplete="current-password"
            placeholder="Ingresa tu contraseña actual"
            :disabled="loadingReauth"
            @keyup.enter="confirmReauth"
            class="reauth-input"
            autofocus
          />
        </div>
      </div>

      <p v-if="reauthError" class="form-error text-center">{{ reauthError }}</p>

      <p class="dialog-hint text-muted">
        Por seguridad, debes confirmar tu identidad para realizar cambios.
      </p>
    </div>

    <template #footer>
      <div class="form-actions" style="justify-content: flex-end; gap: 12px;">
        <el-button @click="cancelReauth" :disabled="loadingReauth">Cancelar</el-button>
        <el-button
          type="primary"
          @click="confirmReauth"
          :loading="loadingReauth"
          :disabled="!reauthPassword.trim()"
          native-type="submit"
        >
          Confirmar
        </el-button>
      </div>
    </template>
  </el-dialog>
</template>

<script setup lang="ts">
import { reactive, ref, onMounted, computed, onUnmounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage, ElDialog } from 'element-plus'
import { Lock } from '@element-plus/icons-vue'
import { useAuth } from '@/composables/useAuth'

const route = useRoute()
const router = useRouter()
const { authHeaders } = useAuth()
const API = import.meta.env.VITE_API_URL

// Estados generales
const loading = ref(false)
const error = ref<string | null>(null)

// Estados específicos para cambio de contraseña
const errorPassword = ref<string | null>(null)
const nuevaPassword = ref('')
const showNuevaPassword = ref(false)

// Validación reactiva de contraseña (mínimo 8 caracteres)
const passwordValida = computed(() => {
  return nuevaPassword.value.length >= 8
})

// Estados para re-autenticación (entrar a editar)
const showReauthDialog = ref(true)
const loadingReauth = ref(false)
const reauthPassword = ref('')
const reauthError = ref<string | null>(null)
const reauthVerified = ref(false)

// 🔐 Contraseña verificada (solo en memoria, se limpia al desmontar)
const passwordVerificada = ref('')

/**
 * Interfaz completa que devuelve la API (incluye campos sensibles).
 * NOTA: El frontend usa solo nombre, apellido, email e id.
 * La API devuelve también `role` y `passwordHash` — son visibles en Network tab.
 * A futuro: el backend debería usar un DTO sin campos sensibles.
 */
interface AdministradorApi {
  id: number
  nombre: string
  apellido: string
  email: string
  role: string
  passwordHash?: string
}

/**
 * Estado reactivo del formulario principal (solo campos editables + id).
 * No guardamos `role` ni `passwordHash` en el estado del formulario.
 */
interface AdministradorForm {
  id: number
  nombre: string
  apellido: string
  email: string
}

const admin = reactive<AdministradorForm>({
  id: 0,
  nombre: '',
  apellido: '',
  email: ''
})

// 🔹 Re-autenticación antes de cargar datos
onMounted(async () => {
  // El dialog se abre automáticamente (showReauthDialog = true)
})

// Cargar datos del admin después de re-autenticación exitosa
const loadAdminData = async () => {
  const id = Number(route.params.id)

  if (!id || isNaN(id)) {
    error.value = 'ID inválido'
    router.push('/administracion')
    return
  }

  try {
    const res = await fetch(`${API}/api/administradores/${id}`, { headers: authHeaders() })
    if (!res.ok) throw new Error(`Error HTTP ${res.status}`)

    const data: AdministradorApi = await res.json()
    // Poblamos SOLO los campos que el formulario usa
    admin.id = data.id
    admin.nombre = data.nombre
    admin.apellido = data.apellido
    admin.email = data.email
    // data.role y data.passwordHash se ignoran intencionalmente
  } catch {
    error.value = 'No se pudieron cargar los datos del administrador'
  }
}

// Confirmar re-autenticación
const confirmReauth = async () => {
  if (!reauthPassword.value.trim()) {
    reauthError.value = 'La contraseña es obligatoria'
    return
  }

  reauthError.value = null
  loadingReauth.value = true

  try {
    // Verificar contraseña actual contra el backend
    const res = await fetch(`${API}/api/auth/verify-password`, {
      method: 'POST',
      headers: authHeaders(),
      body: JSON.stringify({ password: reauthPassword.value })
    })

    if (!res.ok) {
      const data = await res.json().catch(() => ({}))
      throw new Error(data.error || 'Contraseña incorrecta')
    }

    // 🔐 Guardar contraseña verificada en memoria (solo para esta sesión)
    passwordVerificada.value = reauthPassword.value

    reauthVerified.value = true
    showReauthDialog.value = false
    reauthPassword.value = ''
    reauthError.value = null

    // Cargar datos del admin
    await loadAdminData()
  } catch (err: unknown) {
    reauthError.value = err instanceof Error ? err.message : 'Error al verificar contraseña'
  } finally {
    loadingReauth.value = false
  }
}

const cancelReauth = () => {
  showReauthDialog.value = false
  reauthPassword.value = ''
  reauthError.value = null
  ElMessage.warning('Operación cancelada')
  router.push('/administracion')
}

const handleReauthClose = (done: () => void) => {
  // Solo permitir cerrar si ya se verificó
  if (reauthVerified.value) {
    done()
  } else {
    cancelReauth()
  }
}

// Limpiar contraseña verificada al desmontar
onUnmounted(() => {
  passwordVerificada.value = ''
})

// 🔹 Guardar cambios (datos generales: nombre, apellido, email + opcional contraseña)
const guardarAdministrador = async () => {
  loading.value = true
  error.value = null
  errorPassword.value = null

  try {
    // Validar contraseña si se quiere cambiar
    const quiereCambiarPassword = nuevaPassword.value.trim() !== ''
    if (quiereCambiarPassword && !passwordValida.value) {
      errorPassword.value = 'La contraseña debe tener al menos 8 caracteres'
      loading.value = false
      return
    }

    // 1. Primero guardamos los datos generales (nombre, apellido, email)
    const payload = {
      nombre: admin.nombre,
      apellido: admin.apellido,
      email: admin.email
    }

    const res = await fetch(
      `${API}/api/administradores/${admin.id}`,
      {
        method: 'PUT',
        headers: authHeaders(),
        body: JSON.stringify(payload)
      }
    )

    if (!res.ok) {
      const msg = await res.text()
      throw new Error(msg || 'Error al actualizar')
    }

    ElMessage.success('Datos actualizados correctamente')

    // 2. Si se quiere cambiar la contraseña, usar la contraseña ya verificada
    if (nuevaPassword.value.trim() !== '') {
      // Validar longitud mínima
      if (!passwordValida.value) {
        errorPassword.value = 'La contraseña debe tener al menos 8 caracteres'
        loading.value = false
        return
      }

      // Usar la contraseña ya verificada (sin pedirla de nuevo)
      if (!passwordVerificada.value) {
        errorPassword.value = 'Sesión de verificación expirada. Recargá la página e intentá de nuevo.'
        loading.value = false
        return
      }

      try {
        // Enviar cambio de contraseña al backend usando la contraseña ya verificada
        const payload = {
          passwordActual: passwordVerificada.value,
          nuevaPassword: nuevaPassword.value
        }

        const res = await fetch(
          `${API}/api/administradores/${admin.id}/password`,
          {
            method: 'PUT',
            headers: authHeaders(),
            body: JSON.stringify(payload)
          }
        )

        if (!res.ok) {
          const data = await res.json().catch(() => ({}))
          const msg = data.error || (res.status === 401 ? 'La contraseña verificada ya no es válida. Recargá la página e intentá de nuevo.' : 'Error al cambiar la contraseña')
          throw new Error(msg)
        }

        ElMessage.success('Contraseña actualizada correctamente')
        nuevaPassword.value = ''
      } catch (err: unknown) {
        // Si falla el cambio de contraseña, NO perdemos los datos guardados
        errorPassword.value = err instanceof Error ? err.message : 'Error al cambiar la contraseña'
        // NO hacemos router.push aquí para que el usuario vea el error
        // y pueda reintentar sin perder los datos
        loading.value = false
        return
      }
    }

    router.push('/administracion')
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Error al actualizar administrador'
  } finally {
    loading.value = false
  }
}
</script>