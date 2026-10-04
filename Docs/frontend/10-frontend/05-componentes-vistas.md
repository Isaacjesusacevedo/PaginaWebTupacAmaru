# Componentes y Vistas

## Componentes Reutilizables (`src/components/`)

### `NavBar.vue`
**Propósito:** Barra de navegación superior fija.

**Características:**
- Logo clickeable → redirige a `/` (home)
- Enlaces: Inscripción (`/inscripcion`), Contacto (`/contacto`)
- **Se oculta automáticamente** en ruta `/inscripcion` (vía computed `ocultarNavbar`)

```typescript
const ocultarNavbar = computed(() => route.path === '/inscripcion')
```

**Estilos:** Clases CSS propias (`.navbar`, `.logo`, `.menu`) + variables CSS globales.

---

### `Banner/` (Carpeta de Assets)
Contiene imágenes estáticas usadas como fondo decorativo en vistas:
- `bannerEstudiante.jpg` - Banner para vistas de estudiantes
- `bannerProfesor.jpg` - Banner para vistas de administración/profesores

Se importan via alias `@/components/Banner/archivo.jpg`.

---

## Vistas por Módulo

### Módulo Público (`views/public/`)

#### `HomeView.vue` - `/` (Dashboard Principal)
- **Meta:** `requiereAuth: true`
- **UI:** Card centrada con banner, título, menú de navegación admin (botones router-link)
- **Funcionalidad:** 
  - Menú con enlaces a: Administradores, Carreras, Inscripción, Listados
  - Botones deshabilitados: Información académica, Descargas
  - Botón "Cerrar sesión" → navega a `/login`
  - `onMounted`: Health check a `${API}/` (test backend conectividad)

#### `ContactoView.vue` - `/contacto`
- Vista simple estática de información de contacto.

#### `FormulariosView.vue` - `/formularios`
- **Meta:** `requiereAuth: true`
- **UI:** Tabla con columnas: Nombre, Estado, Fecha apertura, Fecha cierre, Acciones
- **Servicio:** `formularioApi.getAll()` → `GET /api/formularios`
- **Acciones:** Editar (router-link) + Eliminar (router-link)
- **Estados:** loading, error, empty state
- **CTA footer:** "Agregar formulario" → `/agregarformulario`

#### `AgregarFormularioView.vue` - `/agregarformulario`
- **Meta:** `requiereAuth: true`
- **Formulario:** nombre, estado (select), fechaApertura, fechaCierre, descripcion
- **Servicio:** `formularioApi.create()` → `POST /api/formularios`

#### `EditarFormularioView.vue` - `/editarformulario/:id`
- **Meta:** `requiereAuth: true`, `props: true`
- **Servicio:** `formularioApi.getById(id)` → `GET /api/formularios/${id}`
- **Submit:** `formularioApi.update(id, formulario)` → `PUT /api/formularios/${id}`

#### `EliminarFormularioView.vue` - `/eliminarformulario/:id`
- **Meta:** `requiereAuth: true`, `props: true`
- **Servicio:** `formularioApi.getById(id)` → `GET /api/formularios/${id}`
- **Acción:** `formularioApi.delete(id)` → `DELETE /api/formularios/${id}`

---

### Módulo Autenticación (`views/auth/`)

#### `LoginView.vue` - `/login`
- **Meta:** `soloInvitado: true`
- **UI:** Formulario con email + password, validación HTML5
- **Lógica:** 
  - POST a `${API}/api/auth/login`
  - En éxito: `guardarSesion(token, admin)` + `router.push({ name: 'home' })`
  - En error: muestra mensaje de error
- **Feature:** Toggle password visibility (botón ojo con icons View/Hide)

---

### Módulo Administradores (`views/administradores/`)

Patrón CRUD consistente en 4 vistas:

| Vista | Ruta | Props | Operación |
|-------|------|-------|-----------|
| `AdministradorView.vue` | `/administracion` | - | **Read** (listado) |
| `AgregarAdministradorView.vue` | `/agregaradministracion` | - | **Create** |
| `EditarAdministradorView.vue` | `/editaradministrador/:id` | `id` | **Update** |
| `EliminarAdministradorView.vue` | `/eliminaradministrador/:id` | `id` | **Delete** |

#### `AdministradorView.vue` (Listado)
- **Tabla** con columnas: Nombre, Apellido, Email, Acciones
- **Acciones:** Botones Editar (router-link) + Eliminar (router-link) con icons Element Plus
- **Servicio:** `adminApi.getAll()` → `GET /api/administradores`
- **Estados:** loading, error, empty state
- **CTA footer:** "Agregar administrador" → `/agregaradministracion`

#### `AgregarAdministradorView.vue` (Crear)
- **Formulario reactivo:** nombre, apellido, email, role (select), passwordTemp (oculto, default)
- **Validación:** required en campos
- **Servicio:** `adminApi.create({ nombre, apellido, email, role, password })` → `POST /api/administradores/with-password`
- **Éxito:** `router.push({ name: 'administracion' })`

#### `EditarAdministradorView.vue` (Editar) ⭐ **Re-autenticación**
- **Props:** `defineProps<{ id: string }>()`
- **Flujo único:** Antes de cargar datos → modal de verificación de contraseña actual
  1. Abre modal `showReauthDialog`
  2. Usuario ingresa password actual
  3. `authApi.verifyPassword()` → `POST /api/auth/verify-password` → si OK, guarda `passwordVerificada` en ref (memoria)
  4. Carga datos admin: `adminApi.getById(id)` → `GET /api/administradores/${id}`
- **Submit datos:** `adminApi.update(id, { nombre, apellido, email })` → `PUT /api/administradores/${id}`
- **Cambio password opcional:** Si usuario ingresa nueva → `adminApi.changePassword(id, { passwordActual, nuevaPassword })` usando `passwordVerificada` guardada
- **Limpieza:** `onUnmounted` limpia `passwordVerificada`

#### `EliminarAdministradorView.vue` (Eliminar)
- **Props:** `defineProps<{ id: string }>()`
- **UI:** Confirmación con datos del admin (`adminApi.getById()` previo)
- **Acción:** `adminApi.delete(id)` → `DELETE /api/administradores/${id}`
- **Éxito:** redirect a listado
- ⚠️ **Deuda técnica:** Usa `http://localhost:5089` hardcoded (2 líneas)

---

### Módulo Carreras (`views/carrera/`)

Estructura idéntica a Administradores:

| Vista | Ruta | Props | Operación |
|-------|------|-------|-----------|
| `CarreraView.vue` | `/carreras` | - | **Read** |
| `AgregarCarreraView.vue` | `/agregarcarreras` | - | **Create** |
| `EditarCarreraView.vue` | `/editarcarrera/:id` | `id` | **Update** |
| `EliminarCarreraView.vue` | `/eliminarcarreras/:id` | `id` | **Delete** |

#### Diferencias clave:
- **Interfaz `Carrera`:** id, nombre, duracionAnios, turno, modalidad, horario, estado
- **Tabla columnas:** Nombre, Duración, Turno, Modalidad, Horario, Estado, Acciones
- **Servicio:** `carreraApi.getAll()` → `GET /api/carreras`
- **Crear:** `carreraApi.create(carrera)` → `POST /api/carreras`
- **Actualizar:** `carreraApi.update(id, carrera)` → `PUT /api/carreras/${id}`
- **Eliminar:** `carreraApi.delete(id)` → `DELETE /api/carreras/${id}`
- ⚠️ **Deuda técnica:** `CarreraView.vue` usa `http://localhost:5089` hardcoded (1 línea)

---

### Módulo Listados (`views/Listados/`)

#### `ListadoView.vue` - `/listados`
- **Meta:** `requiereAuth: true`
- **Propósito:** Reporte de alumnos inscriptos por carrera
- **Interfaz `AlumnoListado`:** alumnoId, nombreCompleto, dni, email, carrera, turno, edad
- **Servicio:** `listadoApi.getAll()` → `GET /api/listado`
- **Tabla:** Alumno, DNI, Edad, Carrera, Turno, (2 columnas vacías para acciones futuras)

#### `InscripciónView.vue` - `/inscripcion`
- **Acceso público** (sin auth, sin navbar)
- **Formulario extenso:** datos personales, contacto, carrera, turno, modalidad, horario
- **Carreras:** `alumnoApi.getCarreras()` → `GET /api/carreras` (público)
- **Submit:** `alumnoApi.inscribir({...})` → `POST /api/inscripcion` (público, `[AllowAnonymous]`)
- ⚠️ **Deuda técnica:** Usa `http://localhost:5089` hardcoded (2 líneas)

---

### Vista Transversal

#### `NotFound.vue` - `*` (404)
- Página genérica "Página no encontrada" con link a home.

---

## Patrones Comunes en Vistas

### 1. Uso de Servicios API (Nuevo - Recomendado)
```typescript
import { adminApi, carreraApi, formularioApi } from '@/services/api'

const data = ref<T[]>([])
const loading = ref(true)
const error = ref<string | null>(null)

const cargar = async () => {
  try {
    data.value = await adminApi.getAll()  // Auto-unwrap, maneja 401/403
  } catch (err) {
    error.value = err.message  // Mensaje amigable ya parseado
  } finally {
    loading.value = false
  }
}

onMounted(cargar)

// Crear
await adminApi.create({ nombre, apellido, email, role, password })

// Actualizar
await adminApi.update(id, { nombre, apellido, email })

// Cambiar password
await adminApi.changePassword(id, { passwordActual, nuevaPassword })

// Eliminar
await adminApi.delete(id)
```

### 2. Fetch Manual (Legacy - En Deuda Técnica)
```typescript
const res = await fetch(`${API}/api/administradores`, { headers: authHeaders() })
if (!res.ok) throw new Error(`HTTP ${res.status}`)
const data = await res.json()
```

### 2. Props de Ruta Tipadas
```typescript
// En router: props: true
// En componente:
defineProps<{ id: string }>()
const idNum = Number(id)
```

### 3. Uso de Element Plus Icons
```vue
<el-icon><Edit /></el-icon>
<el-icon><Delete /></el-icon>
```

### 4. Estructura Visual Estándar
```vue
<main class="section">
  <div class="card card-center">
    <img class="card-media" src="@/components/Banner/bannerProfesor.jpg" alt="..." />
    <header class="text-center">
      <h1>Título</h1>
      <p class="subtitle">Descripción</p>
    </header>
    <!-- Tabla / Formulario -->
    <footer class="table-actions center">
      <router-link class="btn btn-primary" to="...">Acción principal</router-link>
    </footer>
  </div>
</main>
```

---

## Estilos Globales (`src/assets/css/base/main.css`)

Define:
- Variables CSS (colores, spacing, breakpoints)
- Clases utilitarias: `.section`, `.card`, `.card-center`, `.card-lg`, `.table`, `.table-header`, `.table-row`, `.table-cols-admin`, `.table-cols-default`, `.table-cols-formularios`, `.table-actions`, `.btn`, `.btn-primary`, `.btn-success`, `.btn-danger`, `.btn-secondary`, `.btn-menu`, `.text-center`, `.text-muted`, `.text-danger`, `.error-msg`, `.center`
- Reset básico y tipografía

---

## Responsive Mobile (CSS) — Actualización 2026

### Archivos modificados:
| Archivo | Cambios |
|---------|---------|
| `src/assets/css/layout/section.css` | Padding reducido a `16px 12px` en ≤640px |
| `src/assets/css/components/card.css` | `width: 100%`, `box-sizing: border-box`, padding `20px` en mobile |
| `src/assets/css/components/forms.css` | Breakpoint `.form-row` cambiado a `768px` (era 640px) |
| `src/assets/css/components/innputs.css` | `width: 100%`, `font-size: 1rem` (16px) para evitar zoom iOS, `box-sizing: border-box` |

### Principios aplicados:
1. **Mobile-first** en breakpoints: `768px` para formularios, `640px` para layout general
2. **Touch-friendly**: inputs con `16px` font-size (evita zoom automático en iOS)
3. **Single-column layout** en formularios en tablet/mobile
4. **Cards con box-sizing** para evitar overflow
5. **Padding responsivo** en `.section` para no consumir viewport en móvil