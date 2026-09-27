# Sistema de Gestión Institucional — Instituto Superior Docente Túpac Amaru

> Documento de referencia técnica para desarrollo y evolución del proyecto.
> Última actualización: 2026-09-27 | Versión: 1.0.0

---

## Índice

1. [Concepto del Sistema](#1-concepto-del-sistema)
2. [Arquitectura General](#2-arquitectura-general)
3. [Estructura de Directorios](#3-estructura-de-directorios)
4. [Back-end — ASP.NET Core 9 + SQL Server](#4-back-end--aspnet-core-9--sql-server)
5. [Front-end — Vue 3 + TypeScript + Vite](#5-front-end--vue-3--typescript--vite)
6. [Paradigma y Metodología de Desarrollo](#6-paradigma-y-metodología-de-desarrollo)
7. [Flujo de Datos](#7-flujo-de-datos)
8. [Requisitos Funcionales y Técnicos](#8-requisitos-funcionales-y-técnicos)
9. [Observaciones Técnicas y Deuda Técnica](#9-observaciones-técnicas-y-deuda-técnica)
10. [Hoja de Ruta — Próximos Pasos](#10-hoja-de-ruta--próximos-pasos)
11. [Historial de Versiones](#11-historial-de-versiones)

---

## 1. Concepto del Sistema

El sistema es una **plataforma de gestión académica** para el Instituto Superior Docente Túpac Amaru. Centraliza la administración de:

- **Carreras** — CRUD completo + validación de integridad (no eliminar si hay alumnos inscriptos).
- **Alumnos** — Inscripción pública (sin auth) + listado admin con join carrera.
- **Administradores** — CRUD autenticado con JWT + cambio de contraseña + re-autenticación.
- **Profesores** — CRUD completo autenticado (backend listo, frontend pendiente).
- **Formularios** — CRUD completo autenticado (estados: Borrador/Abierto/Cerrado).
- **Listados** — Vista consolidada alumnos + carrera (join en memoria).
- **Autenticación** — Login JWT (8h expiración), BCrypt workFactor 12, Route Guards, Password toggle, Verify/Change password.

El sistema expone un **panel interno** accesible por personal administrativo (JWT) y un **formulario público de inscripción** para nuevos alumnos.

---

## 2. Arquitectura General

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
- **Backend**: Repository genérico (`SqlServerBaseService<T>` + `ICrudJsonService<T>`), DTOs, Exception handling tipado, DI, Clean Architecture por capas.
- **Frontend**: Composition API, Componentes por vista, CSS modular (BEM-like), Route Guards, Composable `useAuth`.

---

## 3. Estructura de Directorios

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
│   │   ├── AdministradorController  # CRUD Admins (JWT) + change-password
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
        │   │   ├── EditarAdministradorView.vue  # Re-auth + change password
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

## 4. Back-end — ASP.NET Core 9 + SQL Server

### 4.1 Stack y Dependencias

| Paquete | Versión | Propósito |
|---------|---------|-----------|
| `Microsoft.AspNetCore` | 9.0 (SDK) | Framework web |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | 9.0.5 | Autenticación JWT Bearer |
| `Microsoft.AspNetCore.OpenApi` | 9.0.5 | Generación de OpenAPI/Swagger |
| `Microsoft.Data.SqlClient` | 5.2.2 | Driver SQL Server (ADO.NET) |
| `BCrypt.Net-Next` | 4.0.3 | Hash de contraseñas |
| `System.Text.Json` | Built-in | Serialización |

**Target Framework**: `net9.0`  
**Características C#**: Nullable reference types, implicit usings.

---

### 4.2 Punto de Entrada: `Program.cs`

```csharp
// ── CORS para el front-end Vue
builder.Services.AddCors(options =>
{
    options.AddPolicy("VueCors", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

// ── JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key no configurado.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(5)
        };
        // Respuestas 401/403 en JSON (no HTML)
        options.Events = new JwtBearerEvents
        {
            OnChallenge = ctx => { ... },  // 401 JSON
            OnForbidden = ctx => { ... }   // 403 JSON
        };
    });

builder.Services.AddAuthorization();

// ── Controllers + JSON options
builder.Services.AddControllers()
    .AddJsonOptions(o => { o.JsonSerializerOptions.PropertyNameCaseInsensitive = true; });

// ── Servicios de persistencia — SQL Server
builder.Services.AddScoped<ICrudJsonService<Carrera>, CarreraSqlServerService>();
builder.Services.AddScoped<ICrudJsonService<Alumno>, AlumnoSqlServerService>();
builder.Services.AddScoped<ICrudJsonService<Administrador>, AdministradorSqlServerService>();
builder.Services.AddScoped<ICrudJsonService<Profesor>, ProfesorSqlServerService>();
builder.Services.AddScoped<ICrudJsonService<Formulario>, FormularioSqlServerService>();

// ── Servicio de autenticación
builder.Services.AddScoped<IAdminAuthService, AdminAuthService>();

// ── Middleware global de errores
app.UseExceptionHandler(errorApp => { ... });

// ── Pipeline
app.UseRouting();
app.UseCors("VueCors");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/", () => "Backend corriendo correctamente!");
app.Run();
```

**Puertos (launchSettings.json):**
- HTTP: `http://localhost:5089`
- HTTPS: `https://localhost:7217`

---

### 4.3 Modelos y Jerarquía de Herencia

```
Persona (abstract)
├── Id       : int
├── Nombre   : string   [Required] [StringLength(100)]
├── Apellido : string   [Required] [StringLength(100)]
└── Email    : string   [Required] [EmailAddress] [StringLength(255)]
    │
    ├── Alumno
    │   ├── DNI              [Required] [Range(1000000, 99999999)]
    │   ├── FechaNacimiento  [Required]
    │   ├── Direccion        [StringLength(200)]
    │   ├── Nacionalidad     [StringLength(100)]
    │   ├── FechaInscripcion (default DateTime.Now)
    │   ├── Telefono         [Phone] [StringLength(50)]
    │   ├── TituloSecundario [StringLength(200)]
    │   ├── Turno            [StringLength(50)]
    │   ├── CarreraId        [Required] [Range(1, int.MaxValue)]  ← FK
    │   └── Edad             (propiedad calculada — no persiste)
    │
    ├── Administrador
    │   ├── Role             [Required] [StringLength(50)]  // Admin | SuperAdmin
    │   ├── PasswordHash     [Required] [StringLength(255)]
    │   └── Activo           [Required] (bit, default 1)     // Soft delete
    │
    └── Profesor
        ├── Telefono         [Phone] [StringLength(50)]
        └── Especialidad     [StringLength(100)]

Carrera  (independiente)
├── Id           : int
├── Nombre       : string  [Required] [StringLength(200)]
├── DuracionAnios: int     [Range(1, 10)]
├── Turno        : string? [StringLength(50)]
├── Modalidad    : string? [StringLength(50)]
├── Horario      : string? [StringLength(100)]
└── Estado       : string? [StringLength(50)]  // Activa | Inactiva (default Activa)

Formulario (independiente)
├── Id              : int
├── Nombre          : string  [Required] [StringLength(200)]
├── Estado          : string  [Required] [StringLength(20)]  // Borrador | Abierto | Cerrado
├── FechaApertura   : DateTime [Required]
├── FechaCierre     : DateTime [Required]
└── Descripcion     : string? [StringLength(1000)]
```

---

### 4.4 Excepciones Personalizadas

Ubicadas en `Backend/Exceptions/`. Desacoplan el servicio de la lógica HTTP.

#### `EntityNotFoundException`
Lanzada cuando una entidad buscada por `Id` no existe (o está inactiva para Admin).

```csharp
public class EntityNotFoundException : Exception
{
    public EntityNotFoundException(string message) : base(message) { }
    public EntityNotFoundException(string entityName, int id)
        : base($"{entityName} con Id {id} no fue encontrado/a.") { }
}
```
**Se lanza en**: `GetById`, `Update`, `Delete` (y `GetById` de Admin filtra `Activo=1`).  
**Capturada en**: controladores → `404 Not Found`.

#### `PersistenceException`
Lanzada ante cualquier error de BD (SqlException, timeout, constraint violation, etc.).

```csharp
public class PersistenceException : Exception
{
    public PersistenceException(string message) : base(message) { }
    public PersistenceException(string message, Exception inner) : base(message, inner) { }
}
```
**Se lanza en**: operaciones ADO.NET (ExecuteReader, ExecuteNonQuery, etc.).  
**Capturada en**: controladores → `500 Internal Server Error`.

---

### 4.5 Data Transfer Objects (DTOs)

#### `AdminResult` (record inmutable)
```csharp
public record AdminResult(
    int    Id,
    string Nombre,
    string Apellido,
    string Email,
    string Role
);
```
- **Sin PasswordHash** → Nunca expone credenciales.
- Serializado directamente en JWT claims y response JSON.

#### `AlumnoListadoDto`
Proyección plana de un alumno con información de su carrera. Usada por `ListadoController`.

```csharp
public class AlumnoListadoDto
{
    public int AlumnoId { get; set; }
    public string? NombreCompleto { get; set; }  // "Apellido, Nombre"
    public int DNI { get; set; }
    public string? Email { get; set; }
    public string? Carrera { get; set; }         // Nombre carrera o "Sin carrera"
    public string? Turno { get; set; }
    public int Edad { get; set; }
}
```

#### Otros DTOs
- `LoginDto` — email + password
- `SetupAdminDto` — nombre, apellido, email, password, role
- `ChangePasswordDto` — passwordActual, nuevaPassword
- `VerifyPasswordDto` — password

---

### 4.6 Servicios

#### Interfaz Común: `ICrudJsonService<T>`
```csharp
public interface ICrudJsonService<T> where T : class
{
    List<T> GetAll();
    T GetById(int id);
    T Create(T entity);
    void Update(int id, T entity);
    void Delete(int id);
}
```
Los 5 servicios implementan esta interfaz.

#### Base: `SqlServerBaseService<T>` (Template Method)
Clase base abstracta con ADO.NET puro. Centraliza:
- Conexión (`Microsoft.Data.SqlClient`)
- `EnsureConnectionAsync()`
- `ExecuteReaderAsync`, `ExecuteNonQueryAsync`, `ExecuteScalarAsync`
- **Template Methods** que subclases implementan:
  - `GetSelectAllSql()`, `GetSelectByIdSql()`, `GetInsertSql()`, `GetUpdateSql()`, `GetDeleteSql()`
  - `MapReaderToEntity(SqlDataReader)` — abstracto
  - `SetParameters(SqlCommand, T, bool isCreate)` — abstracto
- Manejo de excepciones: `SqlException` → `PersistenceException`, Id no encontrado → `EntityNotFoundException`

#### 1. `AdministradorSqlServerService`
- **Tabla**: `Administradores`
- **Soft delete**: `GetAll`/`GetById` filtran `WHERE Activo = 1`; `Delete` → `UPDATE SET Activo=0`.
- **Password**: En `Create`, hashea `PasswordTemp` (o fallback "Cambiar1234!") con BCrypt(12). `Update` no toca password.

#### 2. `AlumnoSqlServerService`
- **Tabla**: `Alumnos`
- **Hard delete**: `DELETE FROM Alumnos WHERE Id = @Id`.
- **NULL handling**: `DBNull.Value` para campos opcionales (`Direccion`, `Nacionalidad`, etc.).
- **FechaInscripcion**: Default `DateTime.Now` en servicio si null.

#### 3. `CarreraSqlServerService`
- **Tabla**: `Carreras`
- **Hard delete**: Validado en controller (no alumnos).
- **Estado**: Default `'Activa'` si null.

#### 4. `ProfesorSqlServerService`
- **Tabla**: `Profesores`
- **Hard delete**. Sin FKs, sin validaciones cruzadas. Email UNIQUE en BD.

#### 5. `FormularioSqlServerService`
- **Tabla**: `Formularios`
- **Hard delete**. Campos: Nombre, Estado (default Borrador), FechaApertura, FechaCierre, Descripcion.

#### Auth: `AdminAuthService` (`IAdminAuthService`)
```csharp
public interface IAdminAuthService
{
    Task<AdminResult?> LoginAsync(string email, string password);
    Task<bool> HayAdminsAsync();
    Task CrearAdminAsync(string nombre, string apellido, string email, string password, string role);
    Task ChangePasswordAsync(string email, string passwordActual, string nuevaPassword);
}
```

| Método | Propósito |
|--------|-----------|
| `LoginAsync` | Valida credenciales, retorna `AdminResult` (sin hash). Usa `BCrypt.Verify`. |
| `HayAdminsAsync` | `COUNT(*)` para setup inicial. |
| `CrearAdminAsync` | Inserta admin con `PasswordHash = BCrypt.Hash(password, 12)`. |
| `ChangePasswordAsync` | Verifica `passwordActual` con `BCrypt.Verify`, actualiza a nuevo hash (workFactor 12). Lanza `UnauthorizedAccessException` si falla. |

**BCrypt Configuration**: Work factor 12 (~250ms CPU 2024, recomendado 2024+).

---

### 4.7 Controladores y Endpoints API

Todos los controladores:
- Reciben servicios por **DI** en constructor.
- Validan `ModelState.IsValid` en POST/PUT.
- Retornan `201 Created` (con `Location` header) en POST.
- Encapsulan en `try-catch` tipado: `EntityNotFoundException`→404, `PersistenceException`→500, `UnauthorizedAccessException`→401, `Exception`→500.
- `[Authorize]` en clase (excepto Auth, Setup, Carreras GET públicos).

#### `AuthController` — `/api/auth`
| Método | Ruta | Auth | Descripción |
|--------|------|------|-------------|
| `POST` | `/api/auth/login` | Público | Login → JWT (8h) + admin data |
| `POST` | `/api/auth/verify-password` | JWT | Verifica password actual del usuario autenticado |

#### `SetupController` — `/api/setup`
| Método | Ruta | Auth | Descripción |
|--------|------|------|-------------|
| `POST` | `/api/setup/admin` | Público (solo Dev) | Crea primer admin si no hay ninguno. `_env.IsDevelopment()` gate. |

#### `CarreraController` — `/api/carreras`
| Método | Ruta | Auth | Descripción |
|--------|------|------|-------------|
| `GET` | `/api/carreras` | Público | Catálogo público |
| `GET` | `/api/carreras/{id}` | Público | Detalle público |
| `POST` | `/api/carreras` | JWT | Crear |
| `PUT` | `/api/carreras/{id}` | JWT | Actualizar |
| `DELETE` | `/api/carreras/{id}` | JWT | Eliminar (validación: no hay alumnos) |

#### `AlumnosController` — `/api/alumnos`
| Método | Ruta | Auth | Descripción |
|--------|------|------|-------------|
| `GET` | `/api/alumnos` | JWT | Listado admin |
| `GET` | `/api/alumnos/{id}` | JWT | Detalle |
| `POST` | `/api/alumnos` | **Público** | Inscripción web (`[AllowAnonymous]`) |
| `PUT` | `/api/alumnos/{id}` | JWT | Actualizar |
| `DELETE` | `/api/alumnos/{id}` | JWT | Eliminar |

**Validación**: `CarreraId` debe existir (`_carreraService.GetById` en Create/Update).

#### `AdministradorController` — `/api/administradores`
| Método | Ruta | Auth | Descripción |
|--------|------|------|-------------|
| `GET` | `/api/administradores` | JWT | Listado (solo Activo=1) |
| `GET` | `/api/administradores/{id}` | JWT | Detalle |
| `POST` | `/api/administradores` | JWT | Crear (hash password) |
| `PUT` | `/api/administradores/{id}` | JWT | Actualizar (no password) |
| `PUT` | `/api/administradores/{id}/password` | JWT | **Cambiar password** (verifica actual) |
| `DELETE` | `/api/administradores/{id}` | JWT | Soft delete (Activo=0) |

#### `ProfesorController` — `/api/profesores`
CRUD completo idéntico a Administrador (sin soft delete, sin password change). `[Authorize]` en clase.

#### `FormularioController` — `/api/formularios`
CRUD completo. `[Authorize]` en clase. Estados: `Borrador` | `Abierto` | `Cerrado`.

#### `ListadoController` — `/api/listado`
| Método | Ruta | Auth | Descripción |
|--------|------|------|-------------|
| `GET` | `/api/listado` | JWT | Join en memoria Alumno+Carrera → `AlumnoListadoDto[]` |

---

### 4.8 Base de Datos

**SQL Server (LocalDB para desarrollo)**  
Script: `backend/Database/CreateDatabase.sql`

| Tabla | Columnas clave | Índices / Constraints |
|-------|----------------|----------------------|
| `Administradores` | Id, Nombre, Apellido, Email, PasswordHash, Role, Activo, FechaCreacion | PK Id, UNIQUE Email, IX_Email |
| `Carreras` | Id, Nombre, DuracionAnios, Turno, Modalidad, Horario, Estado, FechaCreacion | PK Id, CHECK DuracionAnios 1-10 |
| `Alumnos` | Id, Nombre, Apellido, Email, DNI, FechaNacimiento, Direccion, Nacionalidad, FechaInscripcion, Telefono, TituloSecundario, Turno, CarreraId, FechaCreacion | PK Id, UNIQUE DNI, FK CarreraId→Carreras, IX_CarreraId, IX_DNI |
| `Profesores` | Id, Nombre, Apellido, Email, Telefono, Especialidad, FechaCreacion | PK Id, UNIQUE Email, IX_Email |
| `Formularios` | Id, Nombre, Estado, FechaApertura, FechaCierre, Descripcion, FechaCreacion | PK Id |

---

### 4.9 Configuración

**`appsettings.Development.json`:**
```json
{
  "ConnectionStrings": {
    "SqlServer": "Server=(localdb)\\MSSQLLocalDB;Database=InstitutoDB;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Key": "Tupac@Amaru#Instituto!JWT$2026*Clave&MuySecreta=32chars",
    "Issuer": "InstitutoTupacAmaru",
    "Audience": "InstitutoTupacAmaruAdmin"
  }
}
```

> **Nota**: En desarrollo usar *User Secrets* (`dotnet user-secrets`) para la clave JWT.

---

## 5. Front-end — Vue 3 + TypeScript + Vite

### 5.1 Stack y Dependencias

#### Producción
| Paquete | Versión | Propósito |
|---------|---------|-----------|
| `vue` | 3.5+ | Framework reactivo (Composition API) |
| `typescript` | 5.8+ | Tipado estricto |
| `vite` | 7+ | Bundler & Dev Server |
| `vue-router` | 4.5+ | SPA Routing + Guards |
| `pinia` | 3+ | Estado global (auth store) |
| `element-plus` | 2.11+ | Componentes UI |
| `@element-plus/icons-vue` | 1.1+ | Iconografía |

#### Desarrollo
| Paquete | Versión | Propósito |
|---------|---------|-----------|
| `eslint` | 9+ | Linting |
| `prettier` | 3.6+ | Formato |
| `vue-tsc` | 3+ | Type-check |

---

### 5.2 Configuración

**`.env`:**
```env
VITE_API_URL=http://localhost:5089
```

**`vite.config.ts`**: alias `@` → `./src`, plugin Vue, devTools.

**`tsconfig.app.json`**: strict mode, alias `@/*`, DOM lib.

---

### 5.3 Inicialización: `main.ts`

```typescript
import './assets/css/base/main.css'
import { createApp } from 'vue'
import { createPinia } from 'pinia'
import ElementPlus from 'element-plus'
import 'element-plus/dist/index.css'
import * as ElementPlusIconsVue from '@element-plus/icons-vue'
import App from './App.vue'
import router from './router'

const app = createApp(App)
app.use(createPinia())
app.use(router)
app.use(ElementPlus)

for (const [key, component] of Object.entries(ElementPlusIconsVue)) {
  app.component(key, component)
}

app.mount('#app')
```

---

### 5.4 Sistema de Rutas y Guards

**Archivo:** `src/router/index.ts`

| Ruta | Nombre | Componente | Meta | Props |
|------|--------|------------|------|-------|
| `/` | `home` | `HomeView` | `requiereAuth: true` | - |
| `/contacto` | `contacto` | `ContactoView` | - | - |
| `/inscripcion` | `inscripcion` | `InscripciónView` | - | - |
| `/login` | `login` | `LoginView` | `soloInvitado: true` | - |
| `/administracion` | `administracion` | `AdministradorView` | `requiereAuth: true` | - |
| `/agregaradministracion` | `agregaradministracion` | `AgregarAdministradorView` | `requiereAuth: true` | - |
| `/editaradministrador/:id` | `editaradministrador` | `EditarAdministradorView` | `requiereAuth: true` | `true` |
| `/eliminaradministrador/:id` | `eliminaradministrador` | `EliminarAdministradorView` | `requiereAuth: true` | `true` |
| `/carreras` | `carreras` | `CarreraView` | `requiereAuth: true` | - |
| `/agregarcarreras` | `agregarcarreras` | `AgregarCarreraView` | `requiereAuth: true` | - |
| `/editarcarrera/:id` | `editarcarrera` | `EditarCarreraView` | `requiereAuth: true` | `true` |
| `/eliminarcarreras/:id` | `eliminarcarreras` | `EliminarCarreraView` | `requiereAuth: true` | `true` |
| `/formularios` | `formularios` | `FormulariosView` | `requiereAuth: true` | - |
| `/agregarformulario` | `agregarformulario` | `AgregarFormularioView` | `requiereAuth: true` | - |
| `/editarformulario/:id` | `editarformulario` | `EditarFormularioView` | `requiereAuth: true` | `true` |
| `/eliminarformulario/:id` | `eliminarformulario` | `EliminarFormularioView` | `requiereAuth: true` | `true` |
| `/listados` | `listados` | `ListadoView` | `requiereAuth: true` | - |
| `*` | `not-found` | `NotFound` | - | - |

**Guards globales (`router.beforeEach`):**
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

---

### 5.5 Composable: `useAuth.ts`

```typescript
const TOKEN_KEY = 'auth_token'
const ADMIN_KEY = 'auth_admin'

export interface AdminSession {
  id: number
  nombre: string
  apellido: string
  email: string
  role: string
}

export function useAuth() {
  const getToken = () => sessionStorage.getItem(TOKEN_KEY)
  const getAdmin = () => {
    const raw = sessionStorage.getItem(ADMIN_KEY)
    return raw ? JSON.parse(raw) as AdminSession : null
  }
  const isAuthenticated = () => !!getToken()
  const guardarSesion = (token: string, admin: AdminSession) => {
    sessionStorage.setItem(TOKEN_KEY, token)
    sessionStorage.setItem(ADMIN_KEY, JSON.stringify(admin))
  }
  const cerrarSesion = () => {
    sessionStorage.removeItem(TOKEN_KEY)
    sessionStorage.removeItem(ADMIN_KEY)
  }
  const authHeaders = () => {
    const token = getToken()
    return token
      ? { 'Content-Type': 'application/json', Authorization: `Bearer ${token}` }
      : { 'Content-Type': 'application/json' }
  }
  return { getToken, getAdmin, isAuthenticated, guardarSesion, cerrarSesion, authHeaders }
}
```

- **Almacenamiento**: `sessionStorage` (expira al cerrar pestaña).
- **Logout**: Limpia `sessionStorage` + redirect `/login`.
- **Re-autenticación sensible**: `EditarAdministradorView` exige password actual antes de cargar datos (`POST /api/auth/verify-password`), guarda password verificada en `ref` memoria (se limpia en `onUnmounted`).

---

### 5.6 Vistas por Módulo

#### Módulo Público
| Vista | Ruta | Descripción |
|-------|------|-------------|
| `HomeView` | `/` | Dashboard admin (cards menú), health check `${API}/` |
| `ContactoView` | `/contacto` | Info contacto estática |
| `FormulariosView` | `/formularios` | Tabla formularios + CRUD |
| `AgregarFormularioView` | `/agregarformulario` | Formulario crear |
| `EditarFormularioView` | `/editarformulario/:id` | Formulario editar |
| `EliminarFormularioView` | `/eliminarformulario/:id` | Confirmar eliminar |

#### Módulo Auth
| Vista | Ruta | Descripción |
|-------|------|-------------|
| `LoginView` | `/login` | Login JWT + toggle password (View/Hide icons) |

#### Módulo Administradores
| Vista | Ruta | Descripción |
|-------|------|-------------|
| `AdministradorView` | `/administracion` | Listado tabla (Nombre, Apellido, Email, Acciones) |
| `AgregarAdministradorView` | `/agregaradministracion` | Formulario crear (role select, passwordTemp oculto) |
| `EditarAdministradorView` | `/editaradministrador/:id` | **Re-auth modal** → carga datos → PUT datos + opcional PUT password |
| `EliminarAdministradorView` | `/eliminaradministrador/:id` | Confirmar + DELETE |

#### Módulo Carreras
| Vista | Ruta | Descripción |
|-------|------|-------------|
| `CarreraView` | `/carreras` | Listado tabla (Nombre, Duración, Turno, Modalidad, Horario, Estado, Acciones) |
| `AgregarCarreraView` | `/agregarcarreras` | Formulario crear |
| `EditarCarreraView` | `/editarcarrera/:id` | Formulario editar |
| `EliminarCarreraView` | `/eliminarcarreras/:id` | Confirmar + DELETE (error si hay alumnos) |

#### Módulo Listados
| Vista | Ruta | Descripción |
|-------|------|-------------|
| `ListadoView` | `/listados` | Tabla Alumno+Carrera (join) |
| `InscripciónView` | `/inscripcion` | **Público** — Formulario extenso, carga carreras dinámicas, POST `/api/alumnos` |

---

### 5.7 Estilos CSS (Modular, sin `<style>` en .vue)

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

**Principio**: Las vistas solo usan clases globales. Variables en `global.css` (single source of truth).

---

### 5.8 Integración con la API

**Base URL:** `import.meta.env.VITE_API_URL` (configurado en `.env`)

**Headers:** `useAuth().authHeaders()` → Bearer token automático.

**Patrón estándar:**
```typescript
const API = import.meta.env.VITE_API_URL
const { authHeaders } = useAuth()

const res = await fetch(`${API}/api/administradores`, { headers: authHeaders() })
if (!res.ok) throw new Error(`HTTP ${res.status}`)
const data = await res.json()
```

**Endpoints consumidos por módulo:**
- Ver `Docs/frontend/10-frontend/06-integracion-api.md` para tabla completa.

---

## 6. Paradigma y Metodología de Desarrollo

### 6.1 Paradigma: Programación Orientada a Objetos (POO)

#### Encapsulamiento
- `SqlServerBaseService<T>` encapsula ADO.NET, conexión, semáforos, mapeo.
- Helpers `MapReaderToEntity`, `SetParameters` protegidos/abstractos.

#### Herencia
- `Persona` abstracta concentra propiedades comunes + validaciones.
- `Alumno`, `Administrador`, `Profesor` heredan y añaden campos específicos.
- Servicios heredan de `SqlServerBaseService<T>` implementando template methods.

#### Polimorfismo
- `ICrudJsonService<T>` permite DI genérica.
- Controladores dependen de abstracción, no implementación concreta.

---

### 6.2 Patrones Utilizados

| Patrón | Aplicación |
|--------|------------|
| **Repository genérico** | `SqlServerBaseService<T>` + `ICrudJsonService<T>` para cualquier entidad |
| **Template Method** | `SqlServerBaseService` define esqueleto CRUD, subclases implementan SQL/mapeo |
| **DTO** | `AdminResult`, `AlumnoListadoDto` desacoplan representación de dominio |
| **Scoped DI** | Servicios por request (thread-safe, conexión por request) |
| **Separación de capas** | Controllers / Services / Models / DTOs / Exceptions |
| **Inyección de dependencias** | Constructores, no `new` directo |

---

### 6.3 Manejo de Errores — Estrategia por Capas

```
SqlServerBaseService              Controlador                   Cliente HTTP
─────────────────────────────     ─────────────────────────      ───────────────
SqlException       → PersistenceEx → catch(PersistenceEx)      →   500 + { error }
Id no encontrado   → EntityNotFound → catch(EntityNotFound)     →   404 + { error }
UnauthorizedAccess                   catch(UnauthorizedAccess) →   401 + { error }
Exception          → PersistenceEx → catch(Exception)           →   500 + { error }
                                       ↓ si escapa todo
                                    UseExceptionHandler global   →   500 + { error }
```

**Principios:**
- Ningún stack trace llega al cliente.
- Respuestas error siempre JSON: `{ "error": "..." }`.
- Middleware global `UseExceptionHandler` = red de seguridad final.
- Endpoints 401/403 devuelven JSON (configurado en `JwtBearerEvents`).

---

### 6.4 Convenciones de Código

| Convención | Detalle |
|------------|---------|
| **Validación declarativa** | Data Annotations en modelos (`[Required]`, `[Range]`, `[EmailAddress]`, `[Phone]`, `[StringLength]`) |
| **Validación imperativa** | `ModelState.IsValid` en todos los endpoints POST/PUT |
| **Respuestas HTTP semánticas** | `200`, `201` (Location), `204`, `400`, `401`, `403`, `404`, `500` |
| **No exposición de internos** | `catch (Exception)` → mensaje controlado, nunca stack trace |
| **Null safety** | Propiedades requeridas `= string.Empty`; `?` solo donde opcional |
| **Consistencia Id en PUT** | `id` de ruta usado, no del body |
| **Work factor BCrypt** | 12 (configurado en `AdminAuthService` y `AdministradorSqlServerService`) |

---

## 7. Flujo de Datos

### Flujo: Inscripción Pública de Alumno
```
1. Usuario abre /inscripcion (público, sin NavBar)
   ↓
2. onMounted → GET ${API}/api/carreras (público) → Popula dropdown carreras
   ↓
3. Usuario completa formulario + clic "Inscribirse"
   ↓
4. Validación client-side (Nombre, Apellido, DNI, CarreraId requeridos)
   ↓
5. fetch POST ${API}/api/alumnos (público, [AllowAnonymous])
   Body: { Nombre, Apellido, Email, DNI, FechaNacimiento, ..., CarreraId }
   ↓
6. AlumnosController.Create() valida ModelState.IsValid
   ↓
7. _carreraService.GetById(alumno.CarreraId)
   Si no existe → 400 "La carrera con Id X no existe."
   ↓
8. alumno.FechaInscripcion = DateTime.Now
   ↓
9. _alumnoService.Create(alumno) [ADO.NET + SCOPE_IDENTITY()]
   ↓
10. Controller retorna 201 Created + Location: /api/alumnos/{id}
    ↓
11. Frontend muestra confirmación + resetea formulario
```

### Flujo: Login + Acceso Panel
```
1. Usuario en /login ingresa credenciales
   ↓
2. POST ${API}/api/auth/login
   ↓
3. AuthController → AdminAuthService.LoginAsync(email, password)
   - Busca admin (Email + Activo=1)
   - BCrypt.Verify(password, hash)
   - Retorna AdminResult o null
   ↓
4. Si OK → GenerarToken(AdminResult) → JWT (8h, claims: sub, email, name, role, jti)
   ↓
5. Frontend: useAuth().guardarSesion(token, admin) → sessionStorage
   ↓
6. Router guard permite acceso a rutas requiereAuth
   ↓
7. Redirect a / (HomeView)
```

### Flujo: Edición de Administrador con Re-autenticación
```
1. Usuario navega a /editaradministrador/:id
   ↓
2. Se abre modal re-autenticación (showReauthDialog = true)
   ↓
3. Usuario ingresa password actual
   ↓
4. POST ${API}/api/auth/verify-password { password } con JWT
   ↓
5. Si OK → passwordVerificada (ref en memoria)
   ↓
6. GET ${API}/api/administradores/${id} → Popula formulario
   ↓
7. Usuario edita datos → PUT ${API}/api/administradores/${id}
   ↓
8. Si cambia password → PUT ${API}/api/administradores/${id}/password
   Usa passwordVerificada (memoria) como passwordActual
   ↓
9. onUnmounted limpia passwordVerificada
```

---

## 8. Requisitos Funcionales y Técnicos

### 8.1 Requisitos Funcionales

| ID | Módulo | Requisito | Estado |
|----|--------|-----------|--------|
| RF-01 | Carreras | CRUD completo + validación integridad | ✅ |
| RF-02 | Alumnos | Inscripción pública + listado admin con join | ✅ |
| RF-03 | Administradores | CRUD autenticado + change password + re-auth | ✅ |
| RF-04 | Profesores | CRUD completo autenticado (backend) | ✅ Backend / ❌ Frontend |
| RF-05 | Formularios | CRUD completo autenticado (estados) | ✅ |
| RF-06 | Listados | Vista consolidada alumnos + carrera | ✅ |
| RF-07 | Auth | Login JWT (8h) + BCrypt 12 | ✅ |
| RF-08 | Auth | Route Guards + sessionStorage | ✅ |
| RF-09 | Auth | Verify/Change password | ✅ |
| RF-10 | Setup | Primer admin solo Dev (1 vez) | ✅ |

### 8.2 Requisitos Técnicos — Back-end
- .NET 9 SDK
- SQL Server LocalDB (desarrollo)
- `backend/Data/` no requerido (usa BD real)
- User Secrets para `Jwt:Key` en dev
- CORS `AllowAnyOrigin` (solo dev)

### 8.3 Requisitos Técnicos — Front-end
- Node.js ≥ 20, npm
- `npm install` en `Frontend/`
- `npm run dev` → `http://localhost:5173`
- Backend en `http://localhost:5089`

### 8.4 Comandos de Inicio

```bash
# 1. Base de Datos (ejecutar en SSMS / Azure Data Studio / VS Code)
# Archivo: backend/Database/CreateDatabase.sql

# 2. Back-end
cd backend
dotnet run --environment Development
# → http://localhost:5089 | https://localhost:7217

# 3. Front-end (otra terminal)
cd Frontend
npm install    # solo primera vez
npm run dev
# → http://localhost:5173

# 4. Crear primer admin (una sola vez)
POST http://localhost:5089/api/setup/admin
Content-Type: application/json
{
  "nombre": "Admin",
  "apellido": "Sistema",
  "email": "admin@tupac.edu.ar",
  "password": "Password123",
  "role": "SuperAdmin"
}

# 5. Login
# Abrir http://localhost:5173/login con credenciales del paso 4
```

---

## 9. Observaciones Técnicas y Deuda Técnica

### Bugs Críticos Activos
| Ubicación | Problema | Severidad |
|-----------|----------|-----------|
| `InscripciónView.vue` | `CarreraId: string` vs `number` (backend) → 400 potencial | **Crítica** |
| 6 archivos Vue | `http://localhost:5089` hardcoded (no `VITE_API_URL`) | **Crítica** |

### Deuda Técnica — Back-end
| Item | Prioridad | Estado |
|------|-----------|--------|
| Sin capa DTO para GET administradores (expone PasswordHash, Role) | Alta | ❌ |
| `ex.Message.Contains("Carrera")` frágil en `AlumnosController.cs:112` | Media | ❌ |
| Sin rate limiting | Alta | ❌ |
| Sin paginación en GET All | Media | ❌ |
| Sin Swagger/OpenAPI | Media | ❌ |
| Sin tests (xUnit) | Alta | ❌ |
| Profesores: Backend completo pero Frontend sin vistas | Media | ❌ |

### Deuda Técnica — Front-end
| Item | Prioridad | Estado |
|------|-----------|--------|
| Sin capa de servicios API centralizada (`src/services/`) | Alta | ❌ |
| `HomeView.vue` health check hardcoded `http://localhost:5089/weatherforecast` | Alta | ❌ |
| Nombre archivo `InscripciónView.vue` con `ó` (riesgo Linux/CI) | Media | ❌ |
| Typo `innputs.css` → `inputs.css` | Baja | ❌ |
| Carpeta `Frondend` en docs antiguas → `Frontend` | Baja | ❌ |
| 6 archivos con `localhost:5089` hardcoded | Crítica | ❌ |

### Fortalezas del Diseño Actual
- Herencia `Persona` elimina duplicación en 3 entidades.
- `SqlServerBaseService<T>` genérico, template methods, ADO.NET robusto.
- Separación clara: Controllers / Services / Models / DTOs / Exceptions.
- Data Annotations centralizan validaciones en modelo (single source of truth).
- Excepciones tipadas → mapeo HTTP limpio sin acoplamiento.
- JWT stateless + BCrypt 12 + claims estándar.
- Frontend: Composition API, CSS modular, Route Guards, `useAuth` composable.
- `EditarAdministradorView`: re-autenticación segura + password en memoria (limpieza `onUnmounted`).

---

## 10. Hoja de Ruta — Próximos Pasos

### Completado (v1.0.0)
- [x] Arquitectura SQL Server + ADO.NET (`SqlServerBaseService`)
- [x] JWT Authentication + BCrypt workFactor 12
- [x] 8 Controladores con CRUD completo
- [x] 5 Servicios CRUD + AuthService
- [x] Excepciones tipadas + middleware global errores
- [x] Route Guards + sessionStorage + re-auth modal
- [x] CSS modular sin `<style>` en componentes
- [x] Documentación modular en `Docs/`

### Prioridad Crítica
- [ ] Corregir `CarreraId: string` → `number` en `InscripciónView.vue`
- [ ] Migrar 6 archivos a `VITE_API_URL` (eliminar localhost hardcoded)
- [ ] Crear vistas de Profesores (Frontend)

### Prioridad Alta
- [ ] Capa de servicios API centralizada (`src/services/`)
- [ ] DTO para administradores (no exponer PasswordHash/Role en GET)
- [ ] Rate limiting (ASP.NET Core built-in)
- [ ] Paginación en listados
- [ ] Tests xUnit + integración
- [ ] Swagger/OpenAPI

### Prioridad Media
- [ ] Renombrar `InscripciónView.vue` → `InscripcionView.vue`
- [ ] Fix `ex.Message.Contains("Carrera")` → tipar excepción FK
- [ ] Renombrar `innputs.css` → `inputs.css`
- [ ] Validación `FechaCierre > FechaApertura` en backend (Formularios)

### Prioridad Baja
- [ ] Auditoria login (IP, user-agent, timestamp)
- [ ] Refresh tokens + revocación
- [ ] 2FA (TOTP)
- [ ] Policy-based authorization (SuperAdmin vs Admin)

---

## 11. Historial de Versiones

| Versión | Fecha | Cambios Principales |
|---------|-------|---------------------|
| **1.0.0** | 2026-09-27 | **Migración completa a SQL Server + JWT**: `SqlServerBaseService`, 8 controllers, 5 servicios CRUD, AuthService, excepciones tipadas, middleware global, route guards, re-autenticación, CSS modular, documentación completa en `Docs/`. |
| 0.1.1 | 2026-04-15 | Corrección typo `Roll`→`Role`, `201 Created` en POSTs, validación `CarreraId`, `EnsureFileExists`, `SemaphoreSlim`, middleware errores, Data Annotations. |
| 0.1.0 | 2026-03-01 | Backend JSON files (`CrudJsonService<T>`), herencia `Persona`, 4 controladores, frontend Vue 3 + Element Plus básico, inscripción pública, listados. |

---

*Documento mantenido por el equipo de desarrollo — Instituto Superior Docente Túpac Amaru (2026)*