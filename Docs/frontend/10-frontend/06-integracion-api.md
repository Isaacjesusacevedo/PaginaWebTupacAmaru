# Integración con API (Backend)

## Cliente HTTP

El frontend usa **`fetch` nativo** (sin Axios ni librería adicional) para todas las peticiones al backend.

## Configuración de Base URL

```typescript
const API = import.meta.env.VITE_API_URL
// Ejemplo: http://localhost:5127
```

Se define en `.env`:
```env
VITE_API_URL=http://localhost:5127
```

## Arquitectura de Servicios API

El frontend cuenta con una **capa de servicios API** en `src/services/api/` que encapsula todas las llamadas al backend:

| Servicio | Archivo | Endpoints cubiertos |
|----------|---------|---------------------|
| `authApi` | `authApi.js` | Login, verify-password, setup admin |
| `adminApi` | `adminApi.js` | CRUD administradores + change-password |
| `alumnoApi` | `alumnoApi.js` | CRUD alumnos + inscripción pública + getCarreras |
| `carreraApi` | `carreraApi.js` | CRUD carreras |
| `profesorApi` | `profesorApi.js` | CRUD profesores |
| `formularioApi` | `formularioApi.js` | CRUD formularios |
| `listadoApi` | `listadoApi.js` | Listado consolidado alumnos |
| `statsApi` | `statsApi.js` | Stats del dashboard |

**Uso en vistas:**
```javascript
import { adminApi } from '@/services/api'

// En lugar de fetch manual:
const data = await adminApi.getAll()
await adminApi.create({ nombre, apellido, email, role, password })
await adminApi.update(id, { nombre, apellido, email })
await adminApi.changePassword(id, { passwordActual, nuevaPassword })
await adminApi.delete(id)
```

## Headers Estándar

Generados internamente por `useApiFetch` → `useAuth().authHeaders()`:

```typescript
// Con sesión activa
{
  'Content-Type': 'application/json',
  'Authorization': 'Bearer <jwt-token>'
}

// Sin sesión
{
  'Content-Type': 'application/json'
}
```

## Endpoints Consumidos

### Autenticación
| Método | Endpoint | Servicio | Descripción |
|--------|----------|----------|-------------|
| POST | `/api/auth/login` | `authApi.login()` | Login, retorna `{ token, admin }` |
| POST | `/api/auth/verify-password` | `authApi.verifyPassword()` | Verifica password actual |
| POST | `/api/setup/admin` | `authApi.setupAdmin()` | Crea primer admin (solo Dev) |
| GET | `/api/setup/status` | `authApi.setupStatus()` | Verifica si hay admin |

### Administradores
| Método | Endpoint | Servicio | Descripción |
|--------|----------|----------|-------------|
| GET | `/api/administradores` | `adminApi.getAll()` | Listado completo |
| GET | `/api/administradores/:id` | `adminApi.getById()` | Detalle por ID |
| POST | `/api/administradores/with-password` | `adminApi.create()` | Crear nuevo con password |
| PUT | `/api/administradores/:id` | `adminApi.update()` | Actualizar datos |
| PUT | `/api/administradores/:id/password` | `adminApi.changePassword()` | Cambiar contraseña |
| DELETE | `/api/administradores/:id` | `adminApi.delete()` | Eliminar |

### Carreras
| Método | Endpoint | Servicio | Descripción |
|--------|----------|----------|-------------|
| GET | `/api/carreras` | `carreraApi.getAll()` | Listado completo (público) |
| GET | `/api/carreras/:id` | `carreraApi.getById()` | Detalle por ID |
| POST | `/api/carreras` | `carreraApi.create()` | Crear nueva |
| PUT | `/api/carreras/:id` | `carreraApi.update()` | Actualizar |
| DELETE | `/api/carreras/:id` | `carreraApi.delete()` | Eliminar |

### Alumnos
| Método | Endpoint | Servicio | Descripción |
|--------|----------|----------|-------------|
| GET | `/api/alumnos` | `alumnoApi.getAll()` | Listado completo |
| GET | `/api/alumnos/:id` | `alumnoApi.getById()` | Detalle por ID |
| POST | `/api/alumnos` | `alumnoApi.create()` | Crear (admin) |
| POST | `/api/inscripcion` | `alumnoApi.inscribir()` | **Pública** - Nueva inscripción |
| PUT | `/api/alumnos/:id` | `alumnoApi.update()` | Actualizar |
| DELETE | `/api/alumnos/:id` | `alumnoApi.delete()` | Eliminar |
| GET | `/api/carreras` | `alumnoApi.getCarreras()` | Catálogo para inscripción |

### Formularios
| Método | Endpoint | Servicio | Descripción |
|--------|----------|----------|-------------|
| GET | `/api/formularios` | `formularioApi.getAll()` | Listado completo |
| GET | `/api/formularios/:id` | `formularioApi.getById()` | Detalle por ID |
| POST | `/api/formularios` | `formularioApi.create()` | Crear nuevo |
| PUT | `/api/formularios/:id` | `formularioApi.update()` | Actualizar |
| DELETE | `/api/formularios/:id` | `formularioApi.delete()` | Eliminar |

### Listados / Reportes
| Método | Endpoint | Servicio | Descripción |
|--------|----------|----------|-------------|
| GET | `/api/listado` | `listadoApi.getAll()` | Alumnos inscriptos por carrera (join) |

### Stats
| Método | Endpoint | Servicio | Descripción |
|--------|----------|----------|-------------|
| GET | `/api/stats` | `statsApi.getAll()` | Contadores totales por módulo |

## Patrón de Respuestas (Auto-Unwrap)

Todos los servicios usan `useApiFetch` que **desenvuelve automáticamente** el wrapper `ApiResponse<T>` del backend:

```javascript
// Backend retorna: { isSuccess: true, message: "OK", data: { id: 1, nombre: "Juan" } }
// Servicio retorna directamente: { id: 1, nombre: "Juan" }

const admin = await adminApi.getById(1)  // ← ya es el objeto Admin, no el wrapper
```

### Manejo de Errores

Los servicios lanzan `Error` con mensaje descriptivo si:
- `response.ok === false` (HTTP 4xx/5xx)
- `json.isSuccess === false` (error de negocio del backend)

```javascript
try {
  const admin = await adminApi.getById(id)
} catch (err) {
  // err.message = "Administrador con Id 999 no fue encontrado."
  error.value = err.message
}
```

## Variables de Entorno Requeridas

| Variable | Descripción | Requerida |
|----------|-------------|-----------|
| `VITE_API_URL` | URL base del backend API | Sí |

Archivo `.env.example`:
```env
VITE_API_URL=http://localhost:5127
```

## CORS

El backend debe permitir:
- Origin: `http://localhost:5176` (Vite dev server)
- Headers: `Content-Type`, `Authorization`
- Methods: `GET, POST, PUT, DELETE, OPTIONS`

## Timeouts y Reintentos

No implementados actualmente. El `fetch` nativo no tiene timeout por defecto.

## Estado de Sesión y Token

- **Almacenamiento:** `sessionStorage` (se limpia al cerrar pestaña)
- **Expiración:** Manejada por backend (JWT). Frontend no valida `exp` claim.
- **Refresh token:** No implementado
- **Logout:** Limpia `sessionStorage` localmente; no notifica a backend

## 401/403 Auto-Handling

`useApiFetch` maneja automáticamente:
- Si respuesta es 401/403 **y** había token válido **y** no está en `/login`:
  1. Limpia `sessionStorage`
  2. Muestra toast "Sesión expirada. Iniciá sesión nuevamente."
  3. Redirige a `/login` con `redirect` query param