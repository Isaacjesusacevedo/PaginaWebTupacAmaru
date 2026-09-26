# Estructura del Proyecto Backend

## Árbol de Directorios

```
backend/
├── Controllers/           # Capa de presentación (API endpoints)
│   ├── AuthController.cs         # POST /api/auth/login
│   ├── SetupController.cs        # POST /api/setup/admin (solo Dev)
│   ├── AdministradorController.cs# CRUD /api/administradores
│   ├── AlumnosController.cs      # CRUD /api/alumnos
│   ├── CarreraController.cs      # CRUD /api/carreras
│   ├── ProfesorController.cs     # CRUD /api/profesores
│   └── ListadoController.cs      # GET /api/listado (join Alumno+Carrera)
│
├── Services/              # Capa de lógica de negocio
│   ├── Interfaces/
│   │   ├── ICrudJsonService.cs       # Contrato genérico CRUD
│   │   └── IAdminAuthService.cs      # Contrato autenticación
│   ├── SqlServerBaseService.cs       # Base abstracta acceso SQL
│   ├── AdminAuthService.cs           # Implementación auth + BCrypt
│   ├── AdministradorSqlServerService.cs
│   ├── AlumnoSqlServerService.cs
│   ├── CarreraSqlServerService.cs
│   └── ProfesorSqlServerService.cs
│
├── Models/                # Entidades de dominio + DTOs de entrada
│   ├── Persona.cs                  # Base abstracta (Id, Nombre, Apellido, Email)
│   ├── Administrador.cs            # + Role, PasswordTemp
│   ├── Alumno.cs                   # + DNI, FechaNacimiento, CarreraId, etc.
│   ├── Carrera.cs                  # + DuracionAnios, Turno, Modalidad, Estado
│   ├── Profesor.cs                 # + Telefono, Especialidad
│   ├── LoginDto.cs                 # Input login (record)
│   ├── SetupAdminDto.cs            # Input setup admin (record)
│   └── AdminResult.cs              # Output login (record)
│
├── DTOs/                  # DTOs de salida / vistas aplanadas
│   └── AlumnoListadoDto.cs         # Vista join Alumno+Carrera para listados
│
├── Exceptions/            # Excepciones de dominio personalizadas
│   ├── PersistenceException.cs     # Errores de BD / persistencia
│   └── EntityNotFoundException.cs  # Entidad no encontrada (404)
│
├── Database/              # Scripts SQL
│   └── CreateDatabase.sql          # DDL completo (tablas, índices, FKs)
│
├── Properties/
│   └── launchSettings.json         # Perfiles de ejecución (IIS Express, Kestrel)
│
├── Program.cs             # Composition root + pipeline middleware
├── Backend.csproj         # Dependencias y target framework
├── appsettings.json       # Config base (connection strings, JWT)
└── appsettings.Development.json    # Overrides para desarrollo
```

## Responsabilidades por Capa

### Controllers (API Layer)
- **Reciben** HTTP requests, validan `ModelState`
- **Delegan** a servicios (no contienen lógica de negocio)
- **Manejan** excepciones de dominio → HTTP status codes
- **Retornan** `IActionResult` con JSON serializado

### Services (Business Layer)
- **Contienen** lógica de negocio y reglas
- **Orquestan** validaciones cruzadas (ej. Alumno valida Carrera existe)
- **Ejecutan** operaciones de BD vía `SqlServerBaseService`
- **Manejan** transacciones implícitas (una operación = un command)

### Models (Domain Layer)
- **Entidades** ricas con validaciones `DataAnnotations`
- **Herencia**: `Persona` → `Administrador`, `Alumno`, `Profesor`
- **DTOs inmutables** (`record`) para inputs de API

### Exceptions (Cross-Cutting)
- **PersistenceException**: Wrapea errores de BD/IO → 500
- **EntityNotFoundException**: 404 semántico con mensaje claro

## Convenciones de Nombres

| Elemento | Convención | Ejemplo |
|----------|------------|---------|
| Controllers | `{Entidad}Controller` | `AlumnosController` |
| Services | `{Entidad}SqlServerService` | `AlumnoSqlServerService` |
| Interfaces | `I{Funcionalidad}` | `ICrudJsonService<T>` |
| DTOs Input | `{Accion}{Entidad}Dto` | `SetupAdminDto`, `LoginDto` |
| DTOs Output | `{Entidad}Result` / `{Entidad}Dto` | `AdminResult`, `AlumnoListadoDto` |
| Exceptions | `{Contexto}Exception` | `PersistenceException` |
| SQL Tables | Plural PascalCase | `Administradores`, `Alumnos` |
| SQL Columns | PascalCase | `Nombre`, `FechaNacimiento` |
| Parámetros SQL | `@NombreParametro` | `@Email`, `@Id` |