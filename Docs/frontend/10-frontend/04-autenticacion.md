# Autenticación y Autorización (Frontend)

## Composables: `useAuth.ts`

**Archivo:** `src/composables/useAuth.ts`

Gestiona todo el estado de autenticación del lado del cliente usando `sessionStorage`.

### Estado Almacenado

| Clave | Tipo | Descripción |
|-------|------|-------------|
| `auth_token` | `string` | JWT token devuelto por backend |
| `auth_admin` | `string (JSON)` | Objeto `AdminSession` serializado |

### Interfaz `AdminSession`

```typescript
interface AdminSession {
  id: number
  nombre: string
  apellido: string
  email: string
  role: string
}
```

### API del Composable

| Función | Retorno | Descripción |
|---------|---------|-------------|
| `getToken()` | `string \| null` | Obtiene JWT de sessionStorage |
| `getAdmin()` | `AdminSession \| null` | Obtiene datos del admin parseados |
| `isAuthenticated()` | `boolean` | `true` si existe token válido |
| `guardarSesion(token, admin)` | `void` | Guarda token + admin en sessionStorage |
| `cerrarSesion()` | `void` | Limpia sessionStorage |
| `authHeaders()` | `Record<string, string>` | Headers para peticiones autenticadas |

### Headers de Autenticación

```typescript
const authHeaders = (): Record<string, string> => {
  const token = getToken()
  return token
    ? { 'Content-Type': 'application/json', Authorization: `Bearer ${token}` }
    : { 'Content-Type': 'application/json' }
}
```

- **Con token:** `Authorization: Bearer <jwt>` + `Content-Type: application/json`
- **Sin token:** Solo `Content-Type: application/json`

## Flujo de Login

```
1. Usuario ingresa credenciales en LoginView.vue
       │
       ▼
2. POST /api/auth/login → Backend valida
       │
       ▼
3. Backend retorna { token, admin: { id, nombre, apellido, email, role } }
       │
       ▼
4. Frontend: useAuth().guardarSesion(token, admin)
       │
       ▼
5. Router guard permite acceso a rutas protegidas
       │
       ▼
6. Redirect a HomeView (dashboard)
```

## Flujo de Logout

```
1. Usuario click "Cerrar sesión" en HomeView.vue
       │
       ▼
2. Router navega a /login (ruta con meta.soloInvitado: true)
       │
       ▼
3. Navigation guard detecta soloInvitado + autenticado
       │
       ▼
4. Redirect a home → but wait, need explicit logout call
       │
       ▼
5. En LoginView o componente: useAuth().cerrarSesion()
       │
       ▼
6. sessionStorage limpio → isAuthenticated() = false
```

> **Nota:** Actualmente el logout se hace navegando a `/login`. El guard `soloInvitado` redirige a `home` si hay sesión, pero no cierra la sesión automáticamente. Se recomienda llamar `cerrarSesion()` explícitamente antes de navegar.

## Uso en Componentes

```typescript
// En cualquier vista/componente
const { authHeaders, isAuthenticated, getAdmin, cerrarSesion } = useAuth()

// Para fetch autenticado
const res = await fetch(`${API}/api/administradores`, { headers: authHeaders() })

// Verificar rol/permisos
const admin = getAdmin()
if (admin?.role === 'SuperAdmin') { ... }

// Cerrar sesión
const handleLogout = () => {
  cerrarSesion()
  router.push({ name: 'login' })
}
```

## Integración con Navigation Guards

El guard global en `router/index.ts` usa `useAuth()`:

```typescript
router.beforeEach((to) => {
  const { isAuthenticated } = useAuth()
  const autenticado = isAuthenticated()

  if (to.meta.requiereAuth && !autenticado)
    return { name: 'login' }

  if (to.meta.soloInvitado && autenticado)
    return { name: 'home' }
})
```

## Variables de Entorno

| Variable | Uso |
|----------|-----|
| `VITE_API_URL` | Base URL para llamadas al backend (ej: `http://localhost:5089`) |

Se accede via `import.meta.env.VITE_API_URL` en componentes.

## Seguridad

- **Token en sessionStorage:** Se limpia al cerrar pestaña/navegador (más seguro que localStorage)
- **No persistencia:** No hay "recordar sesión" implementado
- **Headers automáticos:** `authHeaders()` inyecta Bearer token en cada petición
- **Validación en backend:** El frontend confía en que el backend valida expiración/firma del JWT