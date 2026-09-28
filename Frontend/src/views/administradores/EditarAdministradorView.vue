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
        <p class="subtitle">Modificá los datos del administrador</p>
      </header>

      <!-- FORM -->
      <form class="form" @submit.prevent="guardarAdministrador">
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

        <div class="form-row">
          <div class="field">
            <label for="email">Email</label>
            <input id="email" v-model="admin.email" type="email" required />
          </div>

          <div class="field">
            <label for="nuevaPassword">Nueva Contraseña</label>
            <div class="password-input-wrapper">
              <input
                id="nuevaPassword"
                v-model="nuevaPassword"
                :type="showNuevaPassword ? 'text' : 'password'"
                autocomplete="new-password"
                placeholder="Mínimo 8 caracteres"
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
        </div>

        <p v-if="errorPassword" class="form-error text-center">{{ errorPassword }}</p>

        <div class="form-actions">
          <button class="btn btn-success" type="submit" :disabled="loading">
            {{ loading ? 'Guardando...' : 'Guardar cambios' }}
          </button>
          <router-link to="/administracion" class="btn btn-secondary">Cancelar</router-link>
        </div>

        <p v-if="error" class="form-error text-center">{{ error }}</p>
      </form>
    </div>
  </main>

  <!-- MODAL DE RE-AUTENTICACIÓN -->
  <el-dialog
    v-model="showReauthDialog"
    class="reauth-dialog"
    title="Verificación de Seguridad"
    :modal="true"
    :close-on-click-modal="false"
    :close-on-press-escape="false"
    :show-close="false"
    width="420"
  >
    <template #header>
      <div class="dialog-header">
        <div class="dialog-header-icon">
          <el-icon class="dialog-header-icon-inner"><Lock /></el-icon>
        </div>
        <div class="dialog-title-wrapper">
          <span class="dialog-title">Verificación de Seguridad</span>
          <span class="dialog-subtitle">Confirma tu identidad para continuar</span>
        </div>
      </div>
    </template>

    <div class="dialog-content">
      <div class="dialog-illustration">
        <div class="dialog-illustration-bg">
          <el-icon class="dialog-icon-large" :size="40"><User /></el-icon>
        </div>
      </div>

      <p class="dialog-message">
        Para editar los datos de un administrador, por favor ingrese su contraseña actual.
      </p>

      <div class="reauth-input-wrapper">
        <label for="reauthPassword" class="form-label">Contraseña actual</label>
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
        />
      </div>

      <p v-if="reauthError" class="form-error text-center">{{ reauthError }}</p>

      <p class="dialog-hint text-muted">
        Por seguridad, debes confirmar tu identidad para realizar cambios.
      </p>
    </div>

    <template #footer>
      <div class="dialog-footer">
        <el-button @click="cancelReauth" :disabled="loadingReauth" variant="light">Cancelar</el-button>
        <el-button
          type="primary"
          @click="confirmReauth"
          :loading="loadingReauth"
          :disabled="!reauthPassword.trim()"
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
import { ElMessage } from 'element-plus'
import { Lock, User, Hide, View } from '@element-plus/icons-vue'
import { apiGet, apiPut } from '@/composables/useApiFetch'

const route = useRoute()
const router = useRouter()

// ── ID desde la ruta ─────────────────────────────────────
const id = Number(route.params.id)
if (isNaN(id) || id <= 0) {
  ElMessage.error('ID de administrador inválido')
  router.push('/administracion')
}

// ── Estado del formulario ────────────────────────────────
interface AdministradorApi {
  id: number
  nombre: string
  apellido: string
  email: string
  role: string
  passwordHash?: string
}

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

const loading = ref(false)
const error = ref<string | null>(null)
const errorPassword = ref<string | null>(null)
const nuevaPassword = ref('')
const showNuevaPassword = ref(false)
const passwordValida = computed(() => nuevaPassword.value.length >= 8)

// ── Estado de re-autenticación ───────────────────────────
const showReauthDialog = ref(true)
const loadingReauth = ref(false)
const reauthPassword = ref('')
const reauthError = ref<string | null>(null)
const reauthVerified = ref(false)
const passwordVerificada = ref('')

// ── Cargar datos del admin ───────────────────────────────
const loadAdminData = async () => {
  if (isNaN(id) || id <= 0) return
  try {
    const data = await apiGet<AdministradorApi>(`/api/administradores/${id}`)
    admin.id = data.id
    admin.nombre = data.nombre
    admin.apellido = data.apellido
    admin.email = data.email
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'No se pudieron cargar los datos'
  }
}

onMounted(() => {
  // El diálogo se abre automáticamente (showReauthDialog = true)
  // Los datos se cargan recién después de verificar la contraseña
})

// ── Re-autenticación ─────────────────────────────────────
const confirmReauth = async () => {
  if (!reauthPassword.value.trim()) {
    reauthError.value = 'La contraseña es obligatoria'
    return
  }

  reauthError.value = null
  loadingReauth.value = true

  try {
    const res = await fetch(`${import.meta.env.VITE_API_URL}/api/auth/verify-password`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        Authorization: `Bearer ${sessionStorage.getItem('auth_token')}`
      },
      body: JSON.stringify({ password: reauthPassword.value })
    })

    if (!res.ok) {
      const data = await res.json().catch(() => ({}))
      throw new Error(data.error || 'Contraseña incorrecta')
    }

    passwordVerificada.value = reauthPassword.value
    reauthVerified.value = true
    showReauthDialog.value = false
    reauthPassword.value = ''
    reauthError.value = null

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

onUnmounted(() => {
  passwordVerificada.value = ''
})

// ── Guardar cambios ──────────────────────────────────────
const guardarAdministrador = async () => {
  if (isNaN(id) || id <= 0) return

  loading.value = true
  error.value = null
  errorPassword.value = null

  try {
    const quiereCambiarPassword = nuevaPassword.value.trim() !== ''

    if (quiereCambiarPassword && !passwordValida.value) {
      errorPassword.value = 'La contraseña debe tener al menos 8 caracteres'
      return
    }

    // 1. Guardar datos generales
    const payload = {
      nombre: admin.nombre,
      apellido: admin.apellido,
      email: admin.email
    }

    await apiPut(`/api/administradores/${id}`, payload)
    ElMessage.success('Datos actualizados correctamente')

    // 2. Cambiar contraseña si se pidió
    if (quiereCambiarPassword) {
      if (!passwordVerificada.value) {
        errorPassword.value = 'Sesión de verificación expirada. Recargá la página.'
        return
      }

      const res = await fetch(`${import.meta.env.VITE_API_URL}/api/administradores/${id}/password`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${sessionStorage.getItem('auth_token')}`
        },
        body: JSON.stringify({
          passwordActual: passwordVerificada.value,
          nuevaPassword: nuevaPassword.value
        })
      })

      if (!res.ok) {
        const data = await res.json().catch(() => ({}))
        throw new Error(data.error || 'Error al cambiar la contraseña')
      }

      ElMessage.success('Contraseña actualizada correctamente')
      nuevaPassword.value = ''
    }

    router.push('/administracion')
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Error al actualizar administrador'
  } finally {
    loading.value = false
  }
}
</script>
