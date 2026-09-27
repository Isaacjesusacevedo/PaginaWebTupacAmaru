# Visión General de la Arquitectura

## Resumen

El backend del **Instituto Tupac Amaru** es una **API REST** construida con **ASP.NET Core 9** que expone endpoints para la gestión académica de un instituto educativo. Sigue una arquitectura **N-Tier (3 capas)** con separación clara de responsabilidades: **AD (Data Access)** → **BR (Business Rules)** → **API (Presentation)**.

## Diagrama de Alto Nivel

```
┌─────────────────────────────────────────────────────────────┐
│                        CLIENTE (Vue 3)                       │
└─────────────────────────┬───────────────────────────────────┘
                          │ HTTPS / REST + JWT
                          ▼
┌─────────────────────────────────────────────────────────────┐
│                      ASP.NET CORE 9 API                      │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────┐  │
│  │ Controllers  │──│   Services   │──│  Data Access     │  │
│  │ (API Layer)  │  │ (BR Layer)   │  │  (AD Layer)      │  │
│  └──────────────┘  └──────────────┘  └──────────────────┘  │
│         │                │                   │               │
│         ▼                ▼                   ▼               │
│  ┌──────────────────────────────────────────────────────┐   │
│  │           Cross-Cutting Concerns                      │   │
│  │  Auth (JWT) │ CORS │ Logging │ Exception Handling   │   │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────────────┐
│                    SQL SERVER (Express)                      │
│         Tablas: Administradores, Alumnos, Carreras,         │
│                 Profesores, Formularios                      │
└─────────────────────────────────────────────────────────────┘
```

## Estructura de Capas (N-Tier)

| Capa | Proyecto | Responsabilidad | Tecnologías |
|------|----------|-----------------|-------------|
| **API** | `Instituto.API` | Controllers, Middleware, DI, Config | ASP.NET Core 9, JWT, CORS |
| **BR** | `Instituto.BR` | Business Logic, Validaciones, Orquestación | BCrypt, DTOs, Interfaces |
| **AD** | `Instituto.AD` | Data Access, Repositorios, SQL, ADO.NET | `Microsoft.Data.SqlClient`, `AccesoDB` |

## Principios de Diseño

| Principio | Implementación |
|-----------|----------------|
| **Separation of Concerns** | Controllers (API) → Services (BR) → Repositories (AD) |
| **Dependency Inversion** | Interfaces `ICrudJsonService<T>`, `IAdminAuthService`, `IRepository` |
| **Single Responsibility** | Un servicio por entidad, un controlador por recurso |
| **DRY** | `AccesoDB` base + Repositorios tipados con helpers reutilizables |
| **Security by Default** | `[Authorize]` global, `[AllowAnonymous]` explícito |
| **Fail Fast** | Validaciones tempranas, excepciones tipadas (`EntityNotFoundException`, `PersistenceException`) |
| **Layer Isolation** | API no conoce AD, BR no conoce API ni AD directamente |

## Tecnologías Principales

| Componente | Tecnología | Versión |
|------------|------------|---------|
| Framework | ASP.NET Core | 9.0 |
| Base de Datos | SQL Server (Express) | 2022+ |
| Data Access | ADO.NET (`Microsoft.Data.SqlClient`) | 5.2.2 |
| Auth | JWT Bearer Tokens | 9.0.5 |
| Hashing | BCrypt.Net-Next | 4.0.3 |
| Serialización | System.Text.Json | Built-in |
| Testing | MSTest + Moq | 3.6.4 / 4.20.72 |

## Flujo de Request Típico

```
1. HTTP Request llega a Kestrel
        │
        ▼
2. Middleware Pipeline:
   - Exception Handler (global)
   - Routing
   - CORS
   - Authentication (JWT validation)
   - Authorization (policy/role check)
        │
        ▼
3. Controller Action ejecutada
        │
        ▼
4. Service Layer (BR) - Lógica de negocio + validaciones
        │
        ▼
5. Repository (AD) - SQL parameterizado via AccesoDB
        │
        ▼
6. Response serializada a JSON
        │
        ▼
7. HTTP Response
```

## Convenciones de Código

- **Naming**: PascalCase para tipos/miembros públicos, camelCase para parámetros/privados
- **Async**: Todas las operaciones I/O son `async`/`await`
- **Nullability**: `<Nullable>enable</Nullable>` en csproj
- **Records**: Para DTOs inmutables (`LoginDto`, `AdminResult`, `SetupAdminDto`, `ServiceResult<T>`)
- **Interfaces**: Prefijo `I` (`IRepository`, `ICrudJsonService<T>`, `IAdminAuthService`)
- **Result Pattern**: `ServiceResult<T>` para respuestas de servicios con éxito/fallo tipados