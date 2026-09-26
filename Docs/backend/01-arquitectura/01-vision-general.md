# Visión General de la Arquitectura

## Resumen

El backend del **Instituto Tupac Amaru** es una **API REST** construida con **ASP.NET Core 9** que expone endpoints para la gestión académica de un instituto educativo. Sigue una arquitectura en capas (Clean Architecture simplificada) con separación clara de responsabilidades.

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
│  │ Controllers  │──│  Services    │──│  Data Access     │  │
│  │ (API Layer)  │  │ (Business)   │  │  (SQL Server)    │  │
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
│                    SQL SERVER (LocalDB)                      │
│         Tablas: Administradores, Alumnos, Carreras,         │
│                 Profesores                                   │
└─────────────────────────────────────────────────────────────┘
```

## Principios de Diseño

| Principio | Implementación |
|-----------|----------------|
| **Separation of Concerns** | Controllers → Services → Data Access |
| **Dependency Inversion** | Interfaces `ICrudJsonService<T>`, `IAdminAuthService` |
| **Single Responsibility** | Un servicio por entidad, un controlador por recurso |
| **DRY** | `SqlServerBaseService<T>` con helpers reutilizables |
| **Security by Default** | `[Authorize]` global, `[AllowAnonymous]` explícito |
| **Fail Fast** | Validaciones tempranas, excepciones tipadas |

## Tecnologías Principales

| Componente | Tecnología | Versión |
|------------|------------|---------|
| Framework | ASP.NET Core | 9.0 |
| Base de Datos | SQL Server (LocalDB) | 2022+ |
| Data Access | ADO.NET (`Microsoft.Data.SqlClient`) | 5.2.2 |
| Auth | JWT Bearer Tokens | 9.0.5 |
| Hashing | BCrypt.Net-Next | 4.0.3 |
| Serialización | System.Text.Json | Built-in |

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
4. Service Layer (lógica de negocio + validaciones)
        │
        ▼
5. Data Access (SQL parameterizado)
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
- **Records**: Para DTOs inmutables (`LoginDto`, `AdminResult`, `SetupAdminDto`)
- **Interfaces**: Prefijo `I` (`ICrudJsonService<T>`)