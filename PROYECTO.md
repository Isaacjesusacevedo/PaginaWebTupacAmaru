# Sistema de Gestión Institucional — Instituto Superior Docente Tupac Amaru

> Documento de referencia técnica para desarrollo y evolución del proyecto.
> Última actualización: 2026-04-15 | Versión: 0.1.1

---

## Índice

1. [Concepto del Sistema](#1-concepto-del-sistema)
2. [Arquitectura General](#2-arquitectura-general)
3. [Estructura de Directorios](#3-estructura-de-directorios)
4. [Back-end — C# .NET 9](#4-back-end--c-net-9)
   - [Stack y Dependencias](#41-stack-y-dependencias)
   - [Punto de Entrada](#42-punto-de-entrada-programcs)
   - [Modelos y Jerarquía de Herencia](#43-modelos-y-jerarquía-de-herencia)
   - [Excepciones Personalizadas](#44-excepciones-personalizadas)
   - [Data Transfer Objects (DTOs)](#45-data-transfer-objects-dtos)
   - [Servicios](#46-servicios)
   - [Controladores y Endpoints](#47-controladores-y-endpoints-api)
   - [Persistencia de Datos](#48-persistencia-de-datos)
5. [Front-end — Vue.js 3 + TypeScript](#5-front-end--vuejs-3--typescript)
   - [Stack y Dependencias](#51-stack-y-dependencias)
   - [Configuración](#52-configuración)
   - [Inicialización de la App](#53-inicialización-de-la-app-maints)
   - [Sistema de Rutas](#54-sistema-de-rutas)
   - [Componentes](#55-componentes)
   - [Vistas por Módulo](#56-vistas-por-módulo)
   - [Estilos CSS](#57-estilos-css)
   - [Integración con la API](#58-integración-con-la-api)
6. [Paradigma y Metodología de Desarrollo](#6-paradigma-y-metodología-de-desarrollo)
7. [Flujo de Datos](#7-flujo-de-datos)
8. [Requisitos Funcionales y Técnicos](#8-requisitos-funcionales-y-técnicos)
9. [Observaciones Técnicas y Deuda Técnica](#9-observaciones-técnicas-y-deuda-técnica)
10. [Hoja de Ruta — Próximos Pasos](#10-hoja-de-ruta--próximos-pasos)
11. [Historial de Versiones](#11-historial-de-versiones)

---

## 1. Concepto del Sistema

El sistema es una **plataforma de gestión académica** para el Instituto Superior Docente Tupac Amaru. Centraliza la administración de:

- **Carreras** — creación, edición y baja de propuestas académicas.
- **Alumnos** — inscripción, seguimiento y consulta de estudiantes.
- **Administradores** — gestión de usuarios con acceso al panel.
- **Listados** — vista consolidada de alumnos con sus carreras.

El sistema expone un **panel interno** accesible por personal administrativo y un **formulario público de inscripción** para nuevos alumnos.

---

## 2. Arquitectura General

```
┌─────────────────────────────────────────────────────┐
│                 CLIENTE (Navegador)                 │
│          Vue.js 3 + TypeScript + Element Plus       │
│                                                     │
│  ┌──────────┐  ┌───────────┐  ┌───────────────┐   │
│  │  Router  │  │   Pinia   │  │  Element Plus  │   │
│  │(vue-router)│ │  (store)  │  │   (UI lib)    │   │
│  └──────────┘  └───────────┘  └───────────────┘   │
└──────────────────────┬──────────────────────────────┘
                       │ HTTP (fetch API)
                       │ http://localhost:5089
┌──────────────────────▼──────────────────────────────┐
│               SERVIDOR (ASP.NET Core 9)             │
│                                                     │
│  ┌─────────────┐  ┌──────────────┐  ┌───────────┐  │
│  │ Controllers │→ │   Services   │→ │ JSON Files│  │
│  │  (REST API) │  │(CrudJsonSvc) │  │  (Data/)  │  │
│  └──────┬──────┘  └──────────────┘  └───────────┘  │
│         │                                           │
│  ┌──────▼──────┐  ┌──────────────┐                 │
│  │  Exceptions │  │    Models    │                 │
│  │(tipadas/OOP)│  │  (Persona ←  │                 │
│  └─────────────┘  │  herencia)   │                 │
│                   └──────────────┘                 │
└─────────────────────────────────────────────────────┘
```

**Patrón arquitectónico**: MVC en el back-end con separación estricta de capas:

- **Modelos**: jerarquía de herencia con clase base abstracta `Persona` (`Alumno`, `Administrador`, `Profesor` heredan de ella). `Carrera` es una entidad independiente con Data Annotations.
- **Servicio genérico**: `CrudJsonService<T>` implementa `ICrudJsonService<T>` y centraliza toda la lógica de acceso a datos, incluyendo control de concurrencia con `SemaphoreSlim(1,1)`.
- **Excepciones personalizadas**: `EntityNotFoundException` y `PersistenceException` representan errores de dominio y de infraestructura respectivamente. Se propagan desde el servicio y son capturadas en los controladores para producir respuestas HTTP semánticas.
- **Controladores**: reciben servicios por inyección de dependencias, validan el modelo con `ModelState.IsValid` y manejan cada tipo de excepción de forma explícita.

**Persistencia**: Archivos JSON en disco (sin base de datos relacional). Cada entidad tiene su propio archivo en `backend/Data/`. El servicio garantiza que el archivo se crea automáticamente con `[]` si no existe.

**Front-end**: patrón de componentes por vista. Router y Pinia instalados; el store no está activo aún.

---

## 3. Estructura de Directorios

```
instituto/
├── PROYECTO.md                        ← Este documento
├── brana.sln                          ← Solución raíz (Visual Studio)
│
├── backend/                           ← Back-end C# .NET 9
│   ├── Backend.sln
│   ├── Backend.csproj                 ← Definición del proyecto .NET
│   ├── Program.cs                     ← Entrada, DI, CORS, middleware global
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   ├── Backend.http                   ← Tests HTTP manuales (REST Client)
│   │
│   ├── Controllers/
│   │   ├── CarreraController.cs       ← CRUD Carreras (DI, try-catch, 201)
│   │   ├── AlumnosController.cs       ← CRUD Alumnos (valida CarreraId)
│   │   ├── AdministradorController.cs ← CRUD Administradores
│   │   └── ListadoController.cs       ← Consulta combinada alumno+carrera
│   │
│   ├── Exceptions/                    ← [NUEVO] Excepciones de dominio
│   │   ├── EntityNotFoundException.cs ← Recurso no encontrado por ID
│   │   └── PersistenceException.cs    ← Errores de I/O o JSON inválido
│   │
│   ├── Models/
│   │   ├── Persona.cs                 ← [NUEVO] Clase base abstracta
│   │   ├── Carrera.cs                 ← Data Annotations agregadas
│   │   ├── Alumno.cs                  ← Hereda Persona, Data Annotations
│   │   ├── Administrador.cs           ← Hereda Persona, Roll→Role corregido
│   │   └── Profesor.cs                ← Hereda Persona, sin controller aún
│   │
│   ├── DTOs/
│   │   └── AlumnoListadoDTO.cs        ← Vista de alumno con datos de carrera
│   │
│   ├── Services/
│   │   ├── ICrudJsonService.cs        ← Interfaz genérica (GetById/Create actualizados)
│   │   └── CrudJsonService.cs         ← Implementación con SemaphoreSlim y manejo de errores
│   │
│   ├── Data/                          ← Almacenamiento JSON
│   │   ├── Carrera.json
│   │   ├── Alumno.json
│   │   └── Administrador.json         ← "Roll" → "Role" corregido
│   │
│   └── Properties/
│       └── launchSettings.json        ← Configuración de puertos
│
└── Frondend/                          ← Front-end Vue.js 3
    ├── index.html                     ← HTML raíz
    ├── package.json                   ← Dependencias npm
    ├── tsconfig.json                  ← Configuración TypeScript (raíz)
    ├── tsconfig.app.json              ← Config TS para la aplicación
    ├── tsconfig.node.json             ← Config TS para Node/tooling
    ├── vite.config.ts                 ← Configuración de Vite
    ├── eslint.config.ts               ← Reglas de linting
    │
    └── src/
        ├── main.ts                    ← Inicialización de Vue
        ├── App.vue                    ← Componente raíz
        │
        ├── router/
        │   └── index.ts               ← Definición de rutas
        │
        ├── components/
        │   ├── NavBar.vue             ← Barra de navegación principal
        │   ├── TheWelcome.vue
        │   └── WelcomeItem.vue
        │
        ├── views/
        │   ├── public/
        │   │   ├── HomeView.vue       ← Panel principal / menú admin
        │   │   └── ContactoView.vue   ← Información de contacto
        │   ├── auth/
        │   │   └── LoginView.vue      ← Formulario de acceso (sin auth real)
        │   ├── carrera/
        │   │   ├── CarreraView.vue    ← Listado de carreras
        │   │   ├── AgregarCarreraView.vue
        │   │   ├── EditarCarreraView.vue
        │   │   └── EliminarCarreraView.vue
        │   ├── administradores/
        │   │   ├── AdministradorView.vue
        │   │   ├── AgregarAdministradorView.vue
        │   │   ├── EditarAdministradorView.vue
        │   │   └── EliminarAdministradorView.vue
        │   ├── Listados/
        │   │   ├── ListadoView.vue    ← Tabla de alumnos con carrera
        │   │   └── InscripciónView.vue ← Formulario público de inscripción
        │   └── NotFound.vue           ← Página 404
        │
        ├── assets/
        │   ├── LogTupac.jpg           ← Logo del instituto
        │   └── css/
        │       ├── base/
        │       │   ├── main.css       ← Importador principal de estilos
        │       │   └── global.css     ← Variables y resets globales
        │       ├── components/
        │       │   ├── admin-menu.css
        │       │   ├── buttons.css
        │       │   ├── card.css
        │       │   ├── forms.css
        │       │   ├── innputs.css    ← (typo pendiente: inputs)
        │       │   ├── navbar.css
        │       │   └── table.css
        │       └── layout/
        │           └── section.css
        │
        └── plugins/                   ← (reservado para plugins futuros)
```

---

## 4. Back-end — C# .NET 9

### 4.1 Stack y Dependencias

| Paquete | Versión | Propósito |
|---------|---------|-----------|
| `Microsoft.AspNetCore` | 9.0 (SDK) | Framework web |
| `Microsoft.AspNetCore.OpenApi` | 9.0.5 | Generación de OpenAPI/Swagger |
| `System.Text.Json` | incluido en SDK | Serialización/deserialización JSON |

**Objetivo del framework**: `net9.0`
**Características C# habilitadas**: Nullable reference types, implicit usings.

---

### 4.2 Punto de Entrada: `Program.cs`

```csharp
// Registro de servicios singleton por tipo de entidad
builder.Services.AddSingleton<ICrudJsonService<Carrera>>(
    new CrudJsonService<Carrera>("Data/Carrera.json")
);
builder.Services.AddSingleton<ICrudJsonService<Alumno>>(
    new CrudJsonService<Alumno>("Data/Alumno.json")
);
builder.Services.AddSingleton<ICrudJsonService<Administrador>>(
    new CrudJsonService<Administrador>("Data/Administrador.json")
);

// CORS (todos los orígenes — sólo para desarrollo)
builder.Services.AddCors(options =>
    options.AddPolicy("VueCors", policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

// JSON case-insensitive
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.PropertyNameCaseInsensitive = true);
```

**Middleware global de errores** (red de seguridad para excepciones no capturadas):

```csharp
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        var body = JsonSerializer.Serialize(
            new { error = "Ocurrió un error interno del servidor." });
        await context.Response.WriteAsync(body);
    });
});
```

**Puertos configurados**:
- HTTP: `http://localhost:5089`
- HTTPS: `https://localhost:7217`

---

### 4.3 Modelos y Jerarquía de Herencia

#### Diagrama de herencia

```
Persona (abstract)
├── Id       : int
├── Nombre   : string   [Required] [StringLength(100)]
├── Apellido : string   [Required] [StringLength(100)]
└── Email    : string   [Required] [EmailAddress]
    │
    ├── Alumno
    │   ├── DNI              [Required] [Range(1000000, 99999999)]
    │   ├── FechaNacimiento  [Required]
    │   ├── Direccion        [StringLength(200)]
    │   ├── Nacionalidad     [StringLength(100)]
    │   ├── FechaInscripcion (nullable, asignada automáticamente en POST)
    │   ├── Telefono         [Phone]
    │   ├── TituloSecundario
    │   ├── Turno
    │   ├── CarreraId        [Range(1, int.MaxValue)]
    │   └── Edad             (propiedad calculada — no persiste)
    │
    ├── Administrador
    │   └── Role             [Required] [StringLength(50)]
    │                        ← corregido desde "Roll"
    │
    └── Profesor
        ├── Telefono         [Phone]
        └── Especialidad     [StringLength(100)]

Carrera  (no hereda de Persona)
├── Id           : int
├── Nombre       : string  [Required] [StringLength(200)]
├── DuracionAnios: int     [Range(1, 10)]
├── Turno        : string?
├── Modalidad    : string?
├── Horario      : string?
└── Estado       : string?
```

#### `Persona` — clase base abstracta

```csharp
public abstract class Persona
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio.")]
    [StringLength(100)]
    public string Apellido { get; set; } = string.Empty;

    [Required(ErrorMessage = "El email es obligatorio.")]
    [EmailAddress(ErrorMessage = "El email no tiene un formato válido.")]
    public string Email { get; set; } = string.Empty;
}
```

#### `Alumno` — hereda de `Persona`

```csharp
public class Alumno : Persona
{
    [Required(ErrorMessage = "El DNI es obligatorio.")]
    [Range(1000000, 99999999, ErrorMessage = "El DNI debe tener entre 7 y 8 dígitos.")]
    public int DNI { get; set; }

    [Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
    public DateTime FechaNacimiento { get; set; }

    [StringLength(200)] public string? Direccion { get; set; }
    [StringLength(100)] public string? Nacionalidad { get; set; }
    public DateTime? FechaInscripcion { get; set; }
    [Phone] public string? Telefono { get; set; }
    public string? TituloSecundario { get; set; }
    public string? Turno { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una carrera válida.")]
    public int CarreraId { get; set; }

    // Propiedad calculada: no almacenada, recomputada en cada lectura
    public int Edad => (int)((DateTime.Now - FechaNacimiento).TotalDays / 365.25);
}
```

#### `Administrador` — hereda de `Persona`

```csharp
public class Administrador : Persona
{
    [Required(ErrorMessage = "El rol es obligatorio.")]
    [StringLength(50)]
    public string Role { get; set; } = string.Empty;  // ← corregido desde "Roll"
}
```

#### `Profesor` — hereda de `Persona` *(sin controller activo)*

```csharp
public class Profesor : Persona
{
    [Phone] public string? Telefono { get; set; }
    [StringLength(100)] public string? Especialidad { get; set; }
}
```

#### `Carrera` — entidad independiente

```csharp
public class Carrera
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre de la carrera es obligatorio.")]
    [StringLength(200)]
    public string Nombre { get; set; } = string.Empty;

    [Range(1, 10, ErrorMessage = "La duración debe estar entre 1 y 10 años.")]
    public int DuracionAnios { get; set; }

    public string? Turno { get; set; }       // Mañana | Tarde | Noche
    public string? Modalidad { get; set; }   // Presencial | Virtual | Mixta
    public string? Horario { get; set; }     // Ej: "18:00 a 22:00"
    public string? Estado { get; set; }      // Activa | Inactiva
}
```

---

### 4.4 Excepciones Personalizadas

Ubicadas en `backend/Exceptions/`. Representan errores de dominio e infraestructura respectivamente, desacoplando el servicio de la lógica HTTP de los controladores.

#### `EntityNotFoundException`

Lanzada cuando una entidad buscada por `Id` no existe en el archivo JSON.

```csharp
public class EntityNotFoundException : Exception
{
    public EntityNotFoundException(string message) : base(message) { }

    // Constructor semántico: "Alumno con Id 5 no fue encontrado/a."
    public EntityNotFoundException(string entityName, int id)
        : base($"{entityName} con Id {id} no fue encontrado/a.") { }
}
```

**Se lanza en**: `GetById`, `Update` (si el índice no existe), `Delete` (si no se eliminó ningún registro).
**Capturada en**: controladores → retorna `404 Not Found`.

#### `PersistenceException`

Lanzada ante cualquier error de lectura/escritura del archivo JSON (I/O, JSON malformado, o falta de propiedad `Id`).

```csharp
public class PersistenceException : Exception
{
    public PersistenceException(string message) : base(message) { }

    public PersistenceException(string message, Exception innerException)
        : base(message, innerException) { }
}
```

**Se lanza en**: `EnsureFileExists`, `ReadFile` (IOException, JsonException), `WriteFile` (IOException), helpers internos.
**Capturada en**: controladores → retorna `500 Internal Server Error`.

---

### 4.5 Data Transfer Objects (DTOs)

#### `AlumnoListadoDto`
Proyección plana de un alumno con información de su carrera. Usada por `ListadoController`.

```csharp
public class AlumnoListadoDto
{
    public int AlumnoId { get; set; }
    public string? NombreCompleto { get; set; }  // Formato: "Apellido, Nombre"
    public int DNI { get; set; }
    public string? Email { get; set; }
    public string? Carrera { get; set; }         // Nombre de la carrera (join en memoria)
    public string? Turno { get; set; }
    public int Edad { get; set; }
}
```

---

### 4.6 Servicios

#### Interfaz: `ICrudJsonService<T>`

```csharp
public interface ICrudJsonService<T> where T : class
{
    List<T> GetAll();

    // Retorna T o lanza EntityNotFoundException — ya no retorna null
    T GetById(int id);

    // Asigna Id autoincremental y retorna la entidad persistida
    T Create(T entity);

    void Update(int id, T entity);
    void Delete(int id);
}
```

#### Implementación: `CrudJsonService<T>`

Servicio genérico con control de concurrencia y manejo explícito de errores.

| Característica | Detalle |
|---------------|---------|
| **Ciclo de vida** | Singleton — una instancia por tipo de entidad |
| **Ruta de archivo** | Configurada en el constructor: `"Data/{Entidad}.json"` |
| **Inicialización** | `EnsureFileExists()` crea el archivo con `[]` si no existe |
| **Concurrencia** | `SemaphoreSlim(1, 1)` — una operación a la vez por instancia |
| **Serialización** | `System.Text.Json` con `WriteIndented = true` y `PropertyNameCaseInsensitive = true` |
| **Acceso a Id** | Reflexión (`PropertyInfo`) centralizada en helpers `GetId` / `SetId` |
| **Autoincremento** | `Create` asigna `Id = max(existentes) + 1` dentro del semáforo (sin race condition) |

**Manejo de errores por capa**:

| Operación | Error capturado | Excepción lanzada |
|-----------|-----------------|-------------------|
| `ReadFile` | `IOException` | `PersistenceException` |
| `ReadFile` | `JsonException` | `PersistenceException` |
| `ReadFile` / `WriteFile` | `Exception` genérica | `PersistenceException` |
| `GetById` | Id no encontrado | `EntityNotFoundException` |
| `Update` | Id no encontrado | `EntityNotFoundException` |
| `Delete` | Ningún registro eliminado | `EntityNotFoundException` |

**Estructura interna**:

```csharp
public class CrudJsonService<T> : ICrudJsonService<T> where T : class
{
    private readonly string _path;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public CrudJsonService(string path)
    {
        _path = path;
        EnsureFileExists(); // crea el archivo si no existe
    }

    // Create asigna Id dentro del semáforo y retorna la entidad persistida
    public T Create(T entity)
    {
        _semaphore.Wait();
        try
        {
            var list = ReadFile();
            int nextId = list.Count == 0 ? 1 : list.Max(GetId) + 1;
            SetId(entity, nextId);
            list.Add(entity);
            WriteFile(list);
            return entity;
        }
        finally { _semaphore.Release(); }
    }
    // ... resto de operaciones con el mismo patrón Wait/try/finally Release
}
```

---

### 4.7 Controladores y Endpoints API

Todos los controladores fueron refactorizados para:
- Recibir servicios por **inyección de dependencias** (DI) en el constructor.
- Validar `ModelState.IsValid` en todos los endpoints que reciben body.
- Retornar `201 Created` (con `Location` header) en los endpoints POST.
- Encapsular cada acción en `try-catch` tipado sin exponer stack traces.

#### `CarrerasController` — `/api/carreras`

| Método | Ruta | Respuesta exitosa | Descripción |
|--------|------|-------------------|-------------|
| `GET` | `/api/carreras` | `200 OK` | Retorna todas las carreras |
| `GET` | `/api/carreras/{id}` | `200 OK` | Retorna una carrera por ID |
| `POST` | `/api/carreras` | `201 Created` | Crea una carrera (Id autogenerado) |
| `PUT` | `/api/carreras/{id}` | `200 OK` | Actualiza una carrera existente |
| `DELETE` | `/api/carreras/{id}` | `204 No Content` | Elimina una carrera |

**Regla de negocio en DELETE**: Verifica que no haya alumnos con `CarreraId == id`. Si los hay, retorna `400 Bad Request` antes de intentar la eliminación.

**Manejo de errores**:

| `catch` | Código HTTP |
|---------|-------------|
| `EntityNotFoundException` | `404 Not Found` |
| `PersistenceException` | `500 Internal Server Error` |
| `Exception` (genérica) | `500 Internal Server Error` (mensaje genérico) |

---

#### `AlumnosController` — `/api/alumnos`

Inyecta `ICrudJsonService<Alumno>` **y** `ICrudJsonService<Carrera>` para validar que el `CarreraId` exista.

| Método | Ruta | Respuesta exitosa | Descripción |
|--------|------|-------------------|-------------|
| `GET` | `/api/alumnos` | `200 OK` | Retorna todos los alumnos |
| `GET` | `/api/alumnos/{id}` | `200 OK` | Retorna un alumno por ID |
| `POST` | `/api/alumnos` | `201 Created` | Crea un alumno (asigna `FechaInscripcion = DateTime.Now`) |
| `PUT` | `/api/alumnos/{id}` | `200 OK` | Actualiza un alumno existente |
| `DELETE` | `/api/alumnos/{id}` | `204 No Content` | Elimina un alumno |

**Validaciones en POST y PUT**:
1. `ModelState.IsValid` — valida Data Annotations del modelo.
2. `_carreraService.GetById(alumno.CarreraId)` — si lanza `EntityNotFoundException`, retorna `400 Bad Request` con mensaje de carrera inexistente.

**Manejo de errores**:

| `catch` | Condición | Código HTTP |
|---------|-----------|-------------|
| `EntityNotFoundException` (carrera) | `CarreraId` inexistente | `400 Bad Request` |
| `EntityNotFoundException` (alumno) | Alumno no encontrado | `404 Not Found` |
| `PersistenceException` | Error de archivo | `500 Internal Server Error` |
| `Exception` | Cualquier otro error | `500 Internal Server Error` |

---

#### `AdministradorController` — `/api/administradores`

| Método | Ruta | Respuesta exitosa | Descripción |
|--------|------|-------------------|-------------|
| `GET` | `/api/administradores` | `200 OK` | Retorna todos los administradores |
| `GET` | `/api/administradores/{id}` | `200 OK` | Retorna un administrador por ID |
| `POST` | `/api/administradores` | `201 Created` | Crea un administrador |
| `PUT` | `/api/administradores/{id}` | `200 OK` | Actualiza un administrador |
| `DELETE` | `/api/administradores/{id}` | `204 No Content` | Elimina un administrador |

**Manejo de errores**: igual a `CarrerasController` (EntityNotFoundException → 404, resto → 500).

---

#### `ListadoController` — `/api/listado`

| Método | Ruta | Respuesta exitosa | Descripción |
|--------|------|-------------------|-------------|
| `GET` | `/api/listado` | `200 OK` | Retorna alumnos con datos de carrera (`AlumnoListadoDto`) |

**Lógica**: Join en memoria entre `Alumno` y `Carrera` por `CarreraId`. Nombre formateado como `"Apellido, Nombre"`. Si un alumno no tiene carrera válida, muestra `"Sin carrera"`.

---

### 4.8 Persistencia de Datos

Los datos se almacenan en archivos JSON en `backend/Data/`. El servicio crea el archivo automáticamente con `[]` si no existe al arrancar.

| Archivo | Entidad | Estado actual |
|---------|---------|---------------|
| `Carrera.json` | Carreras | 2 registros (Ingeniería, Matemáticas) |
| `Alumno.json` | Alumnos | 4 registros |
| `Administrador.json` | Administradores | 1 registro de prueba (campo `"Role"` corregido) |

> **Nota**: `Profesor.json` se crea automáticamente cuando se registre el servicio en DI y se realice la primera operación.

---

## 5. Front-end — Vue.js 3 + TypeScript

### 5.1 Stack y Dependencias

#### Producción

| Paquete | Versión | Propósito |
|---------|---------|-----------|
| `vue` | ^3.5.18 | Framework reactivo |
| `vue-router` | ^4.5.1 | Enrutamiento SPA |
| `pinia` | ^3.0.3 | Estado global (instalado, uso mínimo) |
| `element-plus` | ^2.11.1 | Biblioteca de componentes UI |
| `@element-plus/icons-vue` | ^1.1.4 | Iconos para Element Plus |

#### Desarrollo

| Paquete | Versión | Propósito |
|---------|---------|-----------|
| `vite` | ^7.0.6 | Bundler y servidor de desarrollo |
| `typescript` | ~5.8.0 | Tipado estático |
| `@vitejs/plugin-vue` | ^6.0.1 | Soporte Vue en Vite |
| `vue-tsc` | ^3.0.4 | Type-check de componentes Vue |
| `eslint` | ^9.31.0 | Linting |
| `prettier` | 3.6.2 | Formateo de código |
| `vitest` | — | Testing unitario |

---

### 5.2 Configuración

**`vite.config.ts`**
```typescript
export default defineConfig({
  plugins: [vue(), vueDevTools()],
  resolve: {
    alias: { '@': './src' }  // Alias para importaciones absolutas
  }
})
```

**`tsconfig.app.json`** — configuración relevante:
- Alias `@/*` → `./src/*`
- Librería DOM habilitada
- Tipos Vue incluidos

**`launchSettings.json`** (back-end):
- HTTP: `http://localhost:5089`
- HTTPS: `https://localhost:7217`

---

### 5.3 Inicialización de la App: `main.ts`

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

// Registro global de todos los íconos de Element Plus
for (const [key, component] of Object.entries(ElementPlusIconsVue)) {
  app.component(key, component)
}

app.mount('#app')
```

---

### 5.4 Sistema de Rutas

Definido en [Frondend/src/router/index.ts](Frondend/src/router/index.ts).

| Ruta | Vista | Módulo | Notas |
|------|-------|--------|-------|
| `/` | `HomeView` | Público | Menú principal del panel |
| `/contacto` | `ContactoView` | Público | Info de contacto |
| `/login` | `LoginView` | Auth | Sin auth real aún |
| `/administracion` | `AdministradorView` | Admins | Listado |
| `/agregaradministracion` | `AgregarAdministradorView` | Admins | Crear |
| `/editaradministrador/:id` | `EditarAdministradorView` | Admins | Editar por ID |
| `/eliminaradministrador/:id` | `EliminarAdministradorView` | Admins | Confirmar baja |
| `/carreras` | `CarreraView` | Carreras | Listado |
| `/agregarcarreras` | `AgregarCarreraView` | Carreras | Crear |
| `/editarcarrera/:id` | `EditarCarreraView` | Carreras | Editar por ID |
| `/eliminarcarreras/:id` | `EliminarCarreraView` | Carreras | Confirmar baja |
| `/listados` | `ListadoView` | Listados | Alumnos con carrera |
| `/inscripcion` | `InscripciónView` | Público | Formulario de inscripción |
| `/:pathMatch(.*)*` | `NotFound` | — | 404 |

---

### 5.5 Componentes

#### `NavBar.vue`
- Logo del instituto (enlace a `/`).
- Links de navegación: Inscripción, Contacto.
- **Lógica especial**: Se oculta automáticamente cuando la ruta activa es `/inscripcion` (via propiedad computada).
- Estilos: `navbar.css`.

#### `TheWelcome.vue` / `WelcomeItem.vue`
- Componentes de bienvenida generados por el scaffold de Vite.
- Actualmente no usados en producción.

---

### 5.6 Vistas por Módulo

#### Módulo Público

**`HomeView.vue`** — Panel de administración
- Menú de acceso rápido a todas las secciones.
- Items: Gestión de Administradores, Carreras, Formulario de Inscripción, Listados.
- Items deshabilitados: Información Académica, Descargas, Cerrar Sesión.
- Realiza un `fetch` de prueba al backend en `onMounted`.

**`ContactoView.vue`** — Contacto
- Página estática con información de contacto institucional.

**`NotFound.vue`** — Error 404
- Página genérica para rutas no encontradas.

---

#### Módulo Auth

**`LoginView.vue`** — Acceso al sistema
- Formulario con campos: usuario y contraseña.
- **Estado actual**: Validación simulada con delay de 800ms. Sin integración real con API.
- **Pendiente**: Implementar autenticación real (JWT u otro mecanismo).

---

#### Módulo Carreras

**`CarreraView.vue`** — Listado de carreras
- Obtiene datos de `GET /api/carreras`.
- Tabla con columnas: Nombre, Duración, Turno, Modalidad, Horario, Estado.
- Botones por fila: Editar (ícono), Eliminar (ícono).
- Botón CTA: "Agregar carrera" → `/agregarcarreras`.

**`AgregarCarreraView.vue`** — Alta de carrera
- Campos: Nombre (text, requerido), Duración en años (number, mín. 1), Turno (select), Modalidad (select), Horario (text), Estado (select).
- Envía `POST /api/carreras`. Redirige a `/carreras` en éxito.

**`EditarCarreraView.vue`** — Edición de carrera
- Carga datos con `GET /api/carreras/:id` al montar.
- Mismos campos que Alta, pre-poblados.
- Envía `PUT /api/carreras/:id`. Redirige a `/carreras` en éxito.

**`EliminarCarreraView.vue`** — Baja de carrera
- Muestra los datos de la carrera en modo lectura.
- Envía `DELETE /api/carreras/:id`.
- Muestra el mensaje de error del servidor si hay alumnos inscriptos.

---

#### Módulo Administradores

**`AdministradorView.vue`** — Listado de administradores
- Obtiene datos de `GET /api/administradores`.
- Tabla con columnas: Nombre, Apellido, Email, Rol, Acciones.
- Estado vacío: mensaje "No hay administradores registrados".

**`AgregarAdministradorView.vue`** — Alta de administrador
- Campos: Nombre, Apellido, Email, Rol (todos requeridos).
- Envía `POST /api/administradores`.

**`EditarAdministradorView.vue`** — Edición de administrador
- Carga datos al montar. Rol como dropdown (Admin / SuperAdmin).
- Envía `PUT /api/administradores/:id`.

**`EliminarAdministradorView.vue`** — Baja de administrador
- Confirmación visual. Envía `DELETE /api/administradores/:id`.

---

#### Módulo Listados / Inscripción

**`InscripciónView.vue`** — Formulario público de inscripción
- Vista más extensa del proyecto. NavBar oculta en esta ruta.
- Secciones del formulario:

  | Sección | Campos |
  |---------|--------|
  | Datos personales | Nombre, Apellido, DNI, Nacionalidad, Fecha de nacimiento |
  | Contacto | Dirección, Teléfono, Email |
  | Información académica | Fecha de egreso, Título secundario, checkboxes de documentación |
  | Inscripción | Turno (select), Carrera (select dinámico desde API) |

- **Lógica de negocio**:
  - Calcula edad automáticamente a partir de `FechaNacimiento` (referencia: 30/06/2026).
  - Mutua exclusividad entre "Título" y "Título en trámite".
  - Carga carreras disponibles en `onMounted` desde `GET /api/carreras`.
- Envía `POST /api/alumnos`. Resetea formulario tras éxito.
- Campos requeridos: Nombre, Apellido, DNI, CarreraId.

**`ListadoView.vue`** — Listado de alumnos
- Obtiene datos de `GET /api/listado` (join alumno + carrera).
- Tabla con columnas: Alumno (Apellido, Nombre), DNI, Edad, Carrera, Turno.

---

### 5.7 Estilos CSS

Arquitectura modular de CSS organizada en tres capas:

```
assets/css/
├── base/
│   ├── main.css       ← Importa todos los módulos (entry point)
│   └── global.css     ← Variables CSS y resets globales
├── components/
│   ├── admin-menu.css ← Menú del panel de administración
│   ├── buttons.css    ← Clases: btn-primary, btn-success, btn-danger
│   ├── card.css       ← Contenedores tipo tarjeta / panel
│   ├── forms.css      ← Layout de formularios y form-row
│   ├── innputs.css    ← Estilos de inputs (⚠ typo en nombre de archivo)
│   ├── navbar.css     ← Barra de navegación
│   └── table.css      ← Tablas con grid columns
└── layout/
    └── section.css    ← Secciones de layout
```

---

### 5.8 Integración con la API

**URL base**: `http://localhost:5089` — **hardcodeada en cada componente**.

Todos los llamados a la API utilizan la `fetch` API nativa del navegador. No existe una capa de abstracción (no hay un archivo `api.ts` o `useApi.ts`).

**Ejemplo de patrón actual** (repetido en cada vista):
```typescript
const response = await fetch('http://localhost:5089/api/carreras')
const data = await response.json()
```

> **Deuda técnica pendiente**: Este patrón debe centralizarse en un servicio o composable para facilitar mantenimiento y cambios de entorno (`.env`).

---

## 6. Paradigma y Metodología de Desarrollo

### 6.1 Paradigma: Programación Orientada a Objetos (POO)

El back-end aplica los tres pilares de la POO de forma explícita:

#### Encapsulamiento
- Cada clase agrupa datos y comportamiento relacionado.
- `CrudJsonService<T>` encapsula toda la lógica de acceso al archivo JSON detrás de la interfaz `ICrudJsonService<T>`, ocultando detalles como el `SemaphoreSlim`, la ruta del archivo y la reflexión para acceder a `Id`.
- Los helpers `GetId` y `SetId` son privados al servicio; ninguna capa externa accede directamente a las propiedades por reflexión.

#### Herencia
- `Persona` es una clase abstracta que concentra las propiedades comunes (`Id`, `Nombre`, `Apellido`, `Email`) y sus validaciones.
- `Alumno`, `Administrador` y `Profesor` heredan de `Persona` y sólo definen sus propiedades específicas, eliminando duplicación de código.
- `CrudJsonService<T>` accede a `Id` mediante reflexión sobre el tipo en tiempo de ejecución, funcionando de forma uniforme para cualquier entidad (sea `Persona`-derivada o `Carrera`).

#### Polimorfismo
- La interfaz genérica `ICrudJsonService<T>` permite que los controladores dependan de una abstracción, no de una implementación concreta.
- `CrudJsonService<T>` con distintos tipos (`Carrera`, `Alumno`, `Administrador`) produce comportamientos específicos sin cambiar el contrato de la interfaz.

---

### 6.2 Patrones Utilizados

| Patrón | Aplicación en el proyecto |
|--------|--------------------------|
| **Repositorio genérico** | `CrudJsonService<T>` actúa como repositorio para cualquier entidad |
| **DTO (Data Transfer Object)** | `AlumnoListadoDto` desacopla la representación de `Alumno` de su proyección en el listado |
| **Singleton (vía DI)** | Cada instancia de `CrudJsonService<T>` es singleton; el `SemaphoreSlim` vive por instancia |
| **Separación de capas** | Controllers / Services / Models / DTOs / Exceptions — cada capa con responsabilidad única |
| **Inyección de dependencias** | Los controladores reciben sus servicios por constructor; no instancian `CrudJsonService` directamente |

---

### 6.3 Manejo de Errores — Estrategia por Capas

La propagación de errores sigue una cadena unidireccional:

```
CrudJsonService<T>                   Controlador                   Cliente HTTP
────────────────────────────────     ──────────────────────────     ───────────────
IOException      → PersistenceEx  →  catch(PersistenceEx)      →   500 + { error }
JsonException    → PersistenceEx  →  catch(PersistenceEx)      →   500 + { error }
Id no encontrado → EntityNotFound →  catch(EntityNotFound)     →   404 + { error }
CarreraId inválido → EntityNotFound→ catch(EntityNotFound)     →   400 + { error }
Exception        → PersistenceEx  →  catch(Exception)          →   500 + { error }
                                      ↓ si escapa todo
                                   UseExceptionHandler global   →   500 + { error }
```

**Principios aplicados**:
- Ningún stack trace llega al cliente.
- Las respuestas de error siempre son JSON con la forma `{ "error": "..." }`.
- El middleware global `UseExceptionHandler` actúa como red de seguridad final.

---

### 6.4 Convenciones de Código

| Convención | Detalle |
|------------|---------|
| **Validación declarativa** | Data Annotations (`[Required]`, `[Range]`, `[EmailAddress]`, `[Phone]`, `[StringLength]`) en los modelos |
| **Validación imperativa** | `ModelState.IsValid` en todos los endpoints POST y PUT |
| **Respuestas HTTP semánticas** | `200 OK`, `201 Created`, `204 No Content`, `400 Bad Request`, `404 Not Found`, `500 Internal Server Error` |
| **No exposición de internos** | `catch (Exception)` captura genérica con mensaje controlado; nunca `ex.StackTrace` |
| **Null safety** | Modelos con `= string.Empty` en propiedades requeridas; `?` sólo donde el valor es genuinamente opcional |
| **Consistencia de Id en PUT** | `SetId(entity, id)` se llama dentro del servicio antes de escribir, garantizando que el Id del body no pueda sobrescribir el de la ruta |

---

## 7. Flujo de Datos

### Flujo completo de una operación (ejemplo: Inscripción de alumno)

```
1. Usuario completa InscripciónView.vue
       ↓
2. onMounted → GET /api/carreras → Popula dropdown de carreras
       ↓
3. Usuario completa formulario y hace clic en "Inscribirse"
       ↓
4. Validación client-side (Nombre, Apellido, DNI, CarreraId requeridos)
       ↓
5. fetch POST http://localhost:5089/api/alumnos
   Body: { Nombre, Apellido, Email, DNI, ... CarreraId }
       ↓
6. AlumnosController.Create() valida ModelState.IsValid
       ↓
7. _carreraService.GetById(alumno.CarreraId)
   Si no existe → 400 Bad Request "La carrera con Id X no existe."
       ↓
8. alumno.FechaInscripcion = DateTime.Now
       ↓
9. _alumnoService.Create(alumno) [dentro del SemaphoreSlim]
   - Lee Data/Alumno.json
   - Asigna Id = max(existentes) + 1
   - Agrega alumno y escribe Data/Alumno.json
   - Retorna alumno con Id asignado
       ↓
10. Controller retorna 201 Created + Location: /api/alumnos/{id}
       ↓
11. Frontend muestra confirmación y resetea el formulario
```

### Flujo de baja con validación (ejemplo: Eliminar carrera)

```
1. Usuario navega a /eliminarcarreras/:id
       ↓
2. onMounted → GET /api/carreras/{id} → Muestra datos en modo lectura
       ↓
3. Usuario confirma eliminación
       ↓
4. fetch DELETE http://localhost:5089/api/carreras/{id}
       ↓
5. CarrerasController.Delete()
   - _alumnoService.GetAll() → verifica si algún alumno tiene CarreraId == id
   - Si hay alumnos → return 400 Bad Request { error: "No se puede eliminar..." }
   - Si no hay → _carreraService.Delete(id)
       ↓
6. CrudJsonService<Carrera>.Delete(id) [dentro del SemaphoreSlim]
   - Si no encuentra el Id → lanza EntityNotFoundException
   - Si lo encuentra → remueve y escribe el archivo
       ↓
7. Controller retorna 204 No Content
       ↓
8. Frontend redirige a /carreras
```

---

## 8. Requisitos Funcionales y Técnicos

### 8.1 Requisitos Funcionales

| ID | Módulo | Requisito | Estado |
|----|--------|-----------|--------|
| RF-01 | Carreras | CRUD completo de carreras | ✅ Implementado |
| RF-02 | Carreras | Validar que no haya alumnos antes de eliminar | ✅ Implementado |
| RF-03 | Alumnos | Formulario de inscripción pública | ✅ Implementado |
| RF-04 | Alumnos | CRUD completo de alumnos desde panel | ⚠ Parcial (faltan vistas Edit/Delete) |
| RF-05 | Administradores | CRUD completo de administradores | ✅ Implementado |
| RF-06 | Listados | Vista combinada alumnos + carrera | ✅ Implementado |
| RF-07 | Auth | Login con credenciales reales | ❌ No implementado |
| RF-08 | Auth | Control de acceso por rol | ❌ No implementado |
| RF-09 | Profesores | CRUD de profesores | ❌ No implementado (modelo y herencia definidos) |
| RF-10 | Info Académica | Sección académica | ❌ No implementado (deshabilitado en UI) |

### 8.2 Requisitos Técnicos — Back-end

- .NET 9 SDK instalado.
- El proyecto corre en `http://localhost:5089`.
- El directorio `backend/Data/` debe tener permisos de escritura.
- Los archivos JSON en `Data/` son creados automáticamente por el servicio si no existen.
- Las propiedades `[Required]` sin valor envían `400 Bad Request` automáticamente vía `ModelState`.
- El acceso concurrente al mismo archivo es serializado por `SemaphoreSlim(1,1)` por entidad.
- Las excepciones personalizadas (`EntityNotFoundException`, `PersistenceException`) deben importarse desde `Backend.Exceptions`.

### 8.3 Requisitos Técnicos — Front-end

- Node.js ≥ 20 y npm instalados.
- Ejecutar `npm install` en `Frondend/` antes de iniciar.
- Servidor de desarrollo: `npm run dev` (Vite, puerto por defecto: 5173).
- El back-end debe estar corriendo en `http://localhost:5089` para que las llamadas API funcionen.

### 8.4 Comandos de inicio

```bash
# Back-end
cd backend
dotnet run

# Front-end (en otra terminal)
cd Frondend
npm install    # solo la primera vez
npm run dev
```

---

## 9. Observaciones Técnicas y Deuda Técnica

### Bugs resueltos en v0.1.0

| Ubicación | Problema | Resolución |
|-----------|----------|-----------|
| `Administrador.cs` | Propiedad `Roll` debería ser `Role` | ✅ Corregido — renombrado a `Role` |
| `Data/Administrador.json` | Campo `"Roll"` en los datos persistidos | ✅ Corregido — renombrado a `"Role"` |
| Todos los controladores POST | Retornaban `200 OK` en lugar de `201 Created` | ✅ Corregido — `CreatedAtAction` en todos |

### Bugs pendientes

| Ubicación | Problema | Severidad |
|-----------|----------|-----------|
| `AdministradorView.vue:41` | Interface local usa `roll` pero el backend envía `role` — columna "Rol" siempre vacía en producción | **Crítica** |
| `InscripciónView.vue` — interface `Alumno` | `CarreraId` tipado como `string` pero el backend espera `int` — posible `400 Bad Request` | Alta |
| `LoginView.vue` | Sin autenticación real — `login()` usa `setTimeout` sin llamar al backend | Alta |
| Todos los componentes Vue | URL `http://localhost:5089` hardcodeada — rompe en cualquier deploy | Alta |
| `AlumnosController.cs:112` | `ex.Message.Contains("Carrera")` para distinguir tipo de error — frágil ante cambios de mensaje | Media |
| `InscripciónView.vue` — nombre de archivo | Carácter no-ASCII `ó` en el nombre — puede fallar en sistemas Linux/CI | Media |
| `assets/css/components/innputs.css` | Typo en nombre de archivo | Baja |
| `Frondend/` (carpeta raíz) | Typo en nombre de directorio | Baja |

### Deuda técnica — Back-end

| Item | Estado |
|------|--------|
| Sin autenticación — endpoints públicos | ❌ Pendiente |
| CORS abierto (`AllowAnyOrigin`) | ❌ Pendiente (solo apto para desarrollo) |
| I/O sincrónico bloquea ThreadPool (`File.ReadAllText`, `_semaphore.Wait`) | ❌ Pendiente — migrar a `async/await` + `WaitAsync` |
| Sin rate limiting en ningún endpoint | ❌ Pendiente |
| Sin headers de seguridad (HSTS, X-Frame-Options, X-Content-Type-Options) | ❌ Pendiente |
| Sin HTTPS enforcement en producción | ❌ Pendiente |
| `PersistenceException.Message` puede revelar rutas del sistema al cliente | ❌ Pendiente — sanitizar antes de enviar |
| Sin paginación en endpoints que devuelven listas | ❌ Pendiente |
| Constraint genérico en `CrudJsonService<T>` usa reflexión en lugar de interfaz `IEntity` | ❌ Pendiente |
| Sin Swagger/OpenAPI | ❌ Pendiente |
| Sin tests (xUnit, integración, CI) | ❌ Pendiente |
| `Profesor` sin controller ni endpoints | ❌ Pendiente |
| Concurrencia en JSON sin locks | ✅ Resuelto — `SemaphoreSlim(1,1)` en `CrudJsonService` |
| Sin validación de modelos (`[Required]`, `ModelState`) | ✅ Resuelto — Data Annotations y `ModelState.IsValid` en todos los endpoints |
| Typo `Roll` → `Role` en `Administrador.cs` | ✅ Resuelto |
| Sin manejo de excepciones tipadas | ✅ Resuelto — `EntityNotFoundException` y `PersistenceException` |
| `Create` con race condition de ID entre `GetAll` y `Create` | ✅ Resuelto — ID asignado dentro del semáforo |
| Archivos JSON inexistentes causan error silencioso | ✅ Resuelto — `EnsureFileExists()` en el constructor |
| Sin middleware global de errores | ✅ Resuelto — `UseExceptionHandler` en `Program.cs` |

### Deuda técnica — Front-end

| Item | Estado |
|------|--------|
| Bug: `AdministradorView.vue` muestra `admin.roll` — columna Rol siempre vacía | ❌ Pendiente — corregir a `admin.role` |
| Bug: `InscripciónView.vue` interface tipea `CarreraId: string` cuando debe ser `number` | ❌ Pendiente |
| URL `http://localhost:5089` hardcodeada en 8+ componentes | ❌ Pendiente — mover a `VITE_API_URL` en `.env` |
| Sin capa de servicio API — cada componente llama a `fetch` directamente | ❌ Pendiente — crear `src/services/` por entidad |
| Sin Route Guards — rutas de administración accesibles sin login | ❌ Pendiente — `beforeEach` en el router |
| Login completamente falso — `login()` solo valida campos vacíos con `setTimeout` | ❌ Pendiente |
| Interfaces `Carrera` y `Alumno` duplicadas en múltiples archivos | ❌ Pendiente — centralizar en `src/types/` |
| Sin `AbortController` — requests no se cancelan al desmontar el componente | ❌ Pendiente |
| `alert()` usado para feedback de éxito — bloquea la UI | ❌ Pendiente — reemplazar con notificación Element Plus |
| Sin estado de carga inicial en vistas de listado — tabla vacía sin spinner | ❌ Pendiente |
| Inputs de `InscripciónView.vue` sin atributos `id`/`for` — accesibilidad rota | ❌ Pendiente |
| `main.ts` registra globalmente todos los íconos Element Plus — impacta bundle | ❌ Pendiente — importar solo los usados |
| Binarios `backend/bin/` y `backend/obj/` commiteados en el repositorio | ❌ Pendiente — agregar a `.gitignore` |
| Pinia instalada pero sin uso real | ❌ Pendiente — implementar stores por módulo |
| Sin tipos TypeScript para la API (interfaces no comparten con back-end) | ❌ Pendiente |
| Sin loading states consistentes en todos los componentes | ⚠ Parcial |
| `resetAlumno()` asigna cada campo manualmente — frágil ante nuevos campos | ❌ Pendiente — usar patrón `Object.assign(alumno, estadoInicial())` |

### Fortalezas del diseño actual

- Herencia con `Persona` elimina duplicación en tres entidades del dominio.
- `CrudJsonService<T>` genérico, concurrente y con manejo de errores robusto.
- Separación clara de Controllers / Services / Models / DTOs / Exceptions.
- Data Annotations centralizan las reglas de validación en el modelo (única fuente de verdad).
- Excepciones tipadas permiten mapear errores de dominio a respuestas HTTP sin acoplamiento.
- Stack moderno: Vue 3, TypeScript, .NET 9, Element Plus.
- CSS modular con separación por capas (base, components, layout).

---

## 10. Hoja de Ruta — Próximos Pasos

### Completado en v0.1.0

- [x] Refactorización OOP del back-end (herencia, `Persona`, Data Annotations).
- [x] Excepciones personalizadas (`EntityNotFoundException`, `PersistenceException`).
- [x] Control de concurrencia con `SemaphoreSlim` en `CrudJsonService<T>`.
- [x] Manejo de errores por capas en todos los controladores.
- [x] `201 Created` en todos los endpoints POST.
- [x] Validación de `ModelState.IsValid` en todos los endpoints con body.
- [x] Validación de `CarreraId` al crear/editar alumnos.
- [x] Inicialización automática de archivos JSON faltantes.
- [x] Middleware global de errores en `Program.cs`.
- [x] Corrección del typo `Roll` → `Role`.

### Prioridad Crítica (bugs activos en producción)

- [ ] **Corregir `admin.roll` → `admin.role`** en `AdministradorView.vue` — columna Rol siempre vacía.
- [ ] **Corregir `CarreraId: string` → `CarreraId: number`** en la interface `Alumno` de `InscripciónView.vue`.
- [ ] **Mover URL base de la API a variable de entorno** — crear `.env` con `VITE_API_URL=http://localhost:5089` y reemplazar en todos los componentes.

### Prioridad Alta

- [ ] Implementar autenticación real: endpoint `POST /api/auth/login`, JWT en back-end, middleware `[Authorize]` en todos los endpoints de administración.
- [ ] Agregar Route Guards en el router — `beforeEach` que redirige a `/login` si no hay token.
- [ ] Migrar `CrudJsonService<T>` a operaciones asíncronas (`async/await` + `File.ReadAllTextAsync` + `_semaphore.WaitAsync`).
- [ ] Centralizar llamadas API en `src/services/` (un archivo por entidad: `carreraService.ts`, `alumnoService.ts`, `administradorService.ts`).
- [ ] Centralizar interfaces TypeScript en `src/types/` — eliminar definiciones duplicadas de `Carrera` en tres componentes.
- [ ] Agregar `.gitignore` con `backend/bin/` y `backend/obj/` — remover binarios del repositorio.

### Prioridad Media

- [ ] Restringir CORS a origen conocido (`WithOrigins(...)`) para entornos no locales.
- [ ] Agregar headers de seguridad: `X-Frame-Options`, `X-Content-Type-Options`, `Referrer-Policy`.
- [ ] Reemplazar `alert()` por notificaciones de Element Plus (`ElMessage`).
- [ ] Agregar estado de carga inicial (`loading`) en vistas de listado (`CarreraView`, `AdministradorView`, `ListadoView`).
- [ ] Agregar `AbortController` en llamadas de `onMounted` para cancelar requests al desmontar.
- [ ] Corregir accesibilidad en `InscripciónView.vue` y `LoginView.vue` — agregar `id`/`for` en todos los campos.
- [ ] Añadir interfaz `IEntity` al back-end para eliminar reflexión en `CrudJsonService<T>`.
- [ ] Implementar stores de Pinia para Carreras, Alumnos y Administradores.
- [ ] Implementar CRUD completo de Alumnos desde el panel (vistas Editar/Eliminar).
- [ ] Implementar módulo de Profesores (controller, DI en `Program.cs`, vistas).
- [ ] Configurar Swagger/OpenAPI en `Program.cs`.

### Prioridad Baja / Futuro

- [ ] Agregar tests unitarios al back-end (`xUnit`) — comenzar con `CrudJsonService<T>`.
- [ ] Agregar tests de componentes al front-end (`Vitest` + `@vue/test-utils`).
- [ ] Migrar persistencia de JSON a base de datos relacional (SQLite → PostgreSQL).
- [ ] Agregar paginación en todos los endpoints que devuelven listas.
- [ ] Implementar sección "Información Académica".
- [ ] Implementar sección "Descargas".
- [ ] Implementar "Cerrar sesión".
- [ ] Corregir typo del directorio `Frondend` → `Frontend`.
- [ ] Corregir typo del archivo `innputs.css` → `inputs.css`.
- [ ] Renombrar `InscripciónView.vue` → `InscripcionView.vue` (remover carácter no-ASCII).

---

## 11. Historial de Versiones

| Versión | Fecha | Descripción |
|---------|-------|-------------|
| `0.0.1` | 2026-03-18 | Versión inicial — análisis y documentación del estado del proyecto |
| `0.1.0` | 2026-03-18 | Refactorización OOP del back-end: herencia (`Persona`), excepciones personalizadas, `SemaphoreSlim`, Data Annotations, manejo de errores por capas, `201 Created` en POST, corrección `Roll`→`Role` |
| `0.1.1` | 2026-04-15 | Code review exhaustivo (19 secciones) — nuevos bugs registrados (`admin.roll`, `CarreraId: string`), deuda técnica actualizada, hoja de ruta reorganizada por prioridad crítica/alta/media/baja |

---

*Documento mantenido manualmente. Actualizar con cada iteración significativa del proyecto.*
