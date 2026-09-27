# 🏛️ Sistema de Gestión Institucional — Instituto Superior Docente Túpac Amaru

> Plataforma de gestión académica para la administración de carreras, alumnos, administradores, profesores, formularios e inscripciones. Desarrollada con arquitectura **Frontend (Vue 3) + Backend (ASP.NET Core 9 + SQL Server)**.

---

## 📋 Descripción del Proyecto

Sistema web para el **Instituto Superior Docente Túpac Amaru** que centraliza la gestión académica y administrativa:

| Módulo | Funcionalidad | Estado Backend | Estado Frontend |
|--------|---------------|----------------|-----------------|
| **Carreras** | CRUD completo + validación integridad (no eliminar si hay alumnos) | ✅ | ✅ |
| **Alumnos** | Inscripción pública + listado con join carrera | ✅ | ✅ (inscripción) / ⚠️ (panel admin sin Edit/Delete) |
| **Administradores** | CRUD autenticado con JWT | ✅ | ✅ |
| **Profesores** | CRUD completo autenticado | ✅ | ❌ (sin vistas) |
| **Formularios** | CRUD completo autenticado | ✅ | ✅ (UI completa) |
| **Listados** | Vista consolidada alumnos + carrera | ✅ | ✅ |
| **Autenticación** | Login JWT (8h) + Route Guards + Password toggle + Change password | ✅ | ✅ |

---

## 🏗️ Arquitectura

```
┌─────────────────────────────────────────────────────────────────┐
│                        CLIENTE (Navegador)                      │
│  Vue 3 + TypeScript + Vite + Element Plus + Pinia + Vue Router │
└────────────────────────────┬────────────────────────────────────┘
                             │ HTTPS / REST API + JWT
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│                      SERVIDOR (ASP.NET Core 9)                  │
│  Controllers → Services (SqlServerBaseService) → SQL Server     │
│  JWT Auth + BCrypt (workFactor:12) + Global Exception Handling  │
└─────────────────────────────────────────────────────────────────┘
```

**Patrones aplicados:**
- **Backend**: Repository genérico (`SqlServerBaseService<T>` + `ICrudJsonService<T>`), DTOs, Exception handling tipado, DI, Clean Architecture por capas
- **Frontend**: Composition API, Componentes por vista, CSS modular (BEM-like), Route Guards, Composable `useAuth`

---

## 🛠️ Stack Tecnológico

### Frontend
| Tecnología | Versión | Uso |
|------------|---------|-----|
| Vue 3 | 3.5+ | Framework reactivo (Composition API) |
| TypeScript | 5.8+ | Tipado estricto |
| Vite | 7+ | Bundler & Dev Server |
| Vue Router | 4.5+ | SPA Routing + Guards |
| Pinia | 3+ | Estado global (auth store) |
| Element Plus | 2.11+ | Componentes UI |
| @element-plus/icons-vue | 1.1+ | Iconografía |
| ESLint + Prettier | 9+ / 3.6+ | Linting & Formato |

### Backend
| Tecnología | Versión | Uso |
|------------|---------|-----|
| ASP.NET Core | 9.0 | Web API Framework |
| Microsoft.Data.SqlClient | 5.2+ | Driver SQL Server (ADO.NET) |
| BCrypt.Net-Next | 4.0+ | Hash de contraseñas |
| JWT Bearer | 9.0+ | Autenticación stateless |
| System.Text.Json | Built-in | Serialización |

### Base de Datos
- **SQL Server** (LocalDB para desarrollo)
- Esquema: `Administradores`, `Alumnos`, `Carreras`, `Profesores`, `Formularios`
- Script: `backend/Database/CreateDatabase.sql`

---

## 📁 Estructura del Proyecto

```
instituto/
├── README.md                    # Este archivo
├── .gitignore                   # Exclusiones globales
├── PROYECTO.md                  # Documentación técnica detallada
│
├── backend/                     # ASP.NET Core 9 API
│   ├── backend.sln
│   ├── Backend.csproj
│   ├── Program.cs               # Entry point, DI, JWT, CORS, Middleware
│   ├── appsettings.json         # Configuración (conexión, JWT)
│   ├── appsettings.Development.json
│   ├── Backend.http             # Tests HTTP (REST Client)
│   ├── .gitignore
│   │
│   ├── Controllers/
│   │   ├── AuthController.cs        # POST /api/auth/login, /verify-password
│   │   ├── SetupController.cs       # POST /api/setup/admin (solo Dev, 1 vez)
│   │   ├── CarreraController.cs     # CRUD Carreras
│   │   ├── AlumnosController.cs     # CRUD Alumnos + validación CarreraId
│   │   ├── AdministradorController  # CRUD Admins (JWT)
│   │   ├── ProfesorController.cs    # CRUD Profesores (JWT)
│   │   ├── FormularioController.cs  # CRUD Formularios (JWT)
│   │   └── ListadoController.cs     # Join Alumno + Carrera
│   │
│   ├── Models/
│   │   ├── Persona.cs          # Base abstracta (Id, Nombre, Apellido, Email)
│   │   ├── Alumno.cs           # Hereda Persona + DNI, FechaNac, CarreraId, Edad
│   │   ├── Administrador.cs    # Hereda Persona + Role + PasswordHash + Activo
│   │   ├── Profesor.cs         # Hereda Persona + Telefono, Especialidad
│   │   ├── Carrera.cs          # Entidad independiente
│   │   ├── Formulario.cs       # Entidad independiente
│   │   ├── LoginDto.cs
│   │   ├── SetupAdminDto.cs
│   │   ├── AdminResult.cs
│   │   ├── ChangePasswordDto.cs
│   │   └── VerifyPasswordDto.cs
│   │
│   ├── DTOs/
│   │   └── AlumnoListadoDTO.cs # Proyección Alumno + Carrera
│   │
│   ├── Services/
│   │   ├── ICrudJsonService.cs         # Interfaz genérica CRUD
│   │   ├── SqlServerBaseService.cs     # Base ADO.NET + Template Methods
│   │   ├── IAdminAuthService.cs        # Interfaz auth
│   │   ├── AdminAuthService.cs         # Login/Registro JWT + BCrypt
│   │   ├── CarreraSqlServerService
│   │   ├── AlumnoSqlServerService
│   │   ├── AdministradorSqlServerService
│   │   ├── ProfesorSqlServerService
│   │   └── FormularioSqlServerService
│   │
│   ├── Exceptions/
│   │   ├── EntityNotFoundException.cs  # → 404
│   │   └── PersistenceException.cs     # → 500
│   │
│   ├── Database/
│   │   └── CreateDatabase.sql    # Script creación BD + tablas
│   │
│   └── Properties/launchSettings.json
│
└── Frontend/                    # Vue 3 + Vite
    ├── package.json
    ├── vite.config.ts
    ├── tsconfig.json / .app / .node
    ├── eslint.config.ts
    ├── .env                     # VITE_API_URL=http://localhost:5089
    ├── .gitignore
    │
    ├── index.html
    │
    └── src/
        ├── main.ts              # Bootstrap: Vue, Pinia, Router, Element Plus, Icons
        ├── App.vue              # Layout raíz + NavBar + RouterView
        │
        ├── router/index.ts      # Rutas + Guards (requiereAuth, soloInvitado)
        │
        ├── composables/
        │   └── useAuth.ts       # Token/Admin en sessionStorage + helpers
        │
        ├── components/
        │   ├── NavBar.vue       # Logo + links + logout (oculto en /inscripcion)
        │   └── ... (TheWelcome, WelcomeItem - legacy)
        │
        ├── views/
        │   ├── public/
        │   │   ├── HomeView.vue          # Dashboard admin (menu cards)
        │   │   ├── ContactoView.vue
        │   │   ├── FormulariosView.vue   # Listado formularios
        │   │   ├── AgregarFormularioView.vue
        │   │   ├── EditarFormularioView.vue
        │   │   └── EliminarFormularioView.vue
        │   ├── auth/
        │   │   └── LoginView.vue         # Login JWT + toggle password
        │   ├── carrera/
        │   │   ├── CarreraView.vue       # Tabla + CRUD
        │   │   ├── AgregarCarreraView.vue
        │   │   ├── EditarCarreraView.vue
        │   │   └── EliminarCarreraView.vue
        │   ├── administradores/
        │   │   ├── AdministradorView.vue # Tabla + empty/error states
        │   │   ├── AgregarAdministradorView.vue
        │   │   ├── EditarAdministradorView.vue
        │   │   └── EliminarAdministradorView.vue
        │   └── Listados/
        │       ├── ListadoView.vue       # Join Alumno+Carrera (JWT)
        │       └── InscripciónView.vue   # Formulario público dinámico
        │
        └── assets/css/
            ├── base/
            │   ├── main.css        # Entry point
            │   └── global.css      # Variables CSS, reset, utilidades
            ├── components/
            │   ├── buttons.css     # .btn, variants, sizes
            │   ├── card.css        # .card, .card-center, .card-lg, .card-header, .card-title
            │   ├── forms.css       # .form, .form-row, .field, .form-actions
            │   ├── innputs.css     # Inputs, selects, .password-field, .password-toggle
            │   ├── table.css       # .table, .table-header, .table-row, .table-empty-state, badges
            │   ├── navbar.css      # .navbar, .menu
            │   └── admin-menu.css  # Grid botones dashboard
            └── layout/
                └── section.css     # .section (centrado + padding)
```

---

## 🚀 Inicio Rápido

### Prerrequisitos
- **.NET 9 SDK**
- **Node.js 20+** y **npm**
- **SQL Server LocalDB** (incluido en Visual Studio / VS Code)

### 1. Base de Datos
```bash
# Ejecutar en SSMS / Azure Data Studio / VS Code (ext SQL Server)
# Archivo: backend/Database/CreateDatabase.sql
```

### 2. Configuración Backend
```json
// backend/appsettings.Development.json
{
  "ConnectionStrings": {
    "SqlServer": "Server=(localdb)\\MSSQLLocalDB;Database=InstitutoDB;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Key": "TU_CLAVE_SECRETA_MUY_LARGA_DE_AL_MENOS_32_CARACTERES",
    "Issuer": "InstitutoTupacAmaru",
    "Audience": "InstitutoTupacAmaruAdmin"
  }
}
```

> **Nota**: En desarrollo usar *User Secrets* (`dotnet user-secrets`) para la clave JWT.

### 3. Ejecutar Backend
```bash
cd backend
dotnet run
# → http://localhost:5089 | https://localhost:7217
```

### 4. Configuración Frontend
```bash
# Frontend/.env (ya existe)
VITE_API_URL=http://localhost:5089
```

### 5. Ejecutar Frontend
```bash
cd Frontend
npm install      # solo primera vez
npm run dev      # → http://localhost:5173
```

### 6. Crear Primer Admin (solo primera vez)
```bash
# Terminal o REST Client (backend/Backend.http)
POST http://localhost:5089/api/setup/admin
Content-Type: application/json

{
  "nombre": "Tu Nombre",
  "apellido": "Tu Apellido",
  "email": "admin@tupac.edu.ar",
  "password": "Password123",
  "role": "SuperAdmin"
}
```

### 7. Login
- Abrir `http://localhost:5173/login`
- Credenciales del paso 6
- Redirige a `/` (Dashboard)

---

## 🔐 Autenticación & Autorización

| Aspecto | Implementación |
|---------|----------------|
| **Login** | `POST /api/auth/login` → JWT (8h expiry) |
| **Hash** | BCrypt `workFactor: 12` |
| **Token Storage** | `sessionStorage` (expira al cerrar pestaña) |
| **Route Guards** | `meta.requiereAuth` + `meta.soloInvitado` |
| **Logout** | Limpia `sessionStorage` + redirect `/login` |
| **Roles** | `Admin` / `SuperAdmin` (claim `ClaimTypes.Role`) |
| **Password Toggle** | Botón ojo en LoginView (View/Hide icons) |
| **Verify Password** | `POST /api/auth/verify-password` (requiere JWT) |
| **Change Password** | `PUT /api/administradores/{id}/password` (requiere JWT) |

---

## 📡 API Endpoints Principales

| Módulo | Endpoints | Auth |
|--------|-----------|------|
| **Auth** | `POST /api/auth/login` | Público |
| **Auth** | `POST /api/auth/verify-password` | JWT |
| **Administradores** | `PUT /api/administradores/{id}/password` | JWT |
| **Setup** | `POST /api/setup/admin` (solo Dev, 1 vez) | Público |
| **Carreras** | `GET/POST/PUT/DELETE /api/carreras` | JWT |
| **Alumnos** | `GET/POST/PUT/DELETE /api/alumnos`<br>`POST` público (inscripción) | JWT / Público (POST) |
| **Administradores** | `GET/POST/PUT/DELETE /api/administradores` | JWT |
| **Profesores** | `GET/POST/PUT/DELETE /api/profesores` | JWT |
| **Formularios** | `GET/POST/PUT/DELETE /api/formularios` | JWT |
| **Listado** | `GET /api/listado` (JWT) — Join Alumno+Carrera | JWT |

**Códigos HTTP estándar:**
- `200 OK` — GET, PUT exitosos
- `201 Created` — POST (con `Location` header)
- `204 No Content` — DELETE
- `400 Bad Request` — Validación / FK inexistente
- `401 Unauthorized` — Token inválido/expirado
- `403 Forbidden` — Sin permisos
- `404 Not Found` — Recurso inexistente
- `500 Internal Server Error` — Error interno (sin stack trace)

---

## 🎨 CSS Modular (Sin `<style>` en .vue)

```
src/assets/css/
├── base/
│   ├── main.css      # @import de todo
│   └── global.css    # Variables CSS, reset, utilidades
├── components/
│   ├── buttons.css   # .btn, variants, sizes
│   ├── card.css      # .card, .card-center, .card-lg, .card-header, .card-title
│   ├── forms.css     # .form, .form-row, .field, .form-actions
│   ├── innputs.css   # Inputs, selects, .password-field, .password-toggle
│   ├── table.css     # .table, .table-header, .table-row, .table-empty-state, badges
│   ├── navbar.css    # .navbar, .menu
│   └── admin-menu.css # Grid botones dashboard
└── layout/
    └── section.css   # .section (centrado + padding)
```

**Principio:** Las vistas solo usan clases globales. Variables en `global.css` (single source of truth).

---

## 🧪 Testing Manual (REST Client)

Archivo: `backend/Backend.http` — Incluye requests para:
- Setup admin
- Login + copy token
- CRUD Carreras / Alumnos / Administradores / Profesores / Formularios
- Listado
- Headers `Authorization: Bearer {{token}}`

---

## 📝 Scripts Disponibles

### Frontend
```bash
npm run dev        # Servidor desarrollo (Vite)
npm run build      # Build producción (type-check + vite build)
npm run preview    # Preview build
npm run lint       # ESLint + fix
npm run format     # Prettier
npm run type-check # vue-tsc --build
```

### Backend
```bash
dotnet run         # Dev (watch habilitado en launchSettings)
dotnet build       # Compilar
dotnet test        # (pendiente: agregar xUnit)
```

---

## 🐛 Deuda Técnica Conocida

| Prioridad | Item |
|-----------|------|
| **Crítica** | `InscripciónView.vue` tipa `CarreraId: string` vs `number` (backend) |
| **Crítica** | **6 archivos** aún usan `localhost:5089` hardcoded (no `VITE_API_URL`) |
| **Alta** | Sin capa de servicios API centralizada (`src/services/`) |
| **Alta** | `HomeView.vue` health check hardcoded `http://localhost:5089/weatherforecast` |
| **Media** | `ex.Message.Contains("Carrera")` frágil en `AlumnosController.cs:112` |
| **Media** | Nombre archivo `InscripciónView.vue` con `ó` (riesgo Linux/CI) |
| **Media** | **Profesores**: Backend CRUD completo ✅ pero **Frontend sin vistas** |
| **Baja** | Typo `innputs.css` → `inputs.css` |
| **Baja** | Carpeta `Frondend` → `Frontend` (en docs antiguas) |
| **Baja** | Backend devuelve `PasswordHash` y `Role` en GET administradores (debería usar DTO) |

---

## 📄 Documentación Técnica Completa

| Archivo | Estado |
|---------|--------|
| **README.md** | ✅ Actualizado (este archivo) |
| **PROYECTO.md** | ✅ Actualizado (arquitectura SQL Server, JWT, flujo de datos) |
| **Docs/** | ✅ Documentación modular actualizada (backend + frontend) |

---

## 👥 Autores

Desarrollado por estudiantes de **3.º año TSAS** — Instituto Superior Docente Túpac Amaru (2026)

---

## 📄 Licencia

MIT — Uso educativo e institucional.