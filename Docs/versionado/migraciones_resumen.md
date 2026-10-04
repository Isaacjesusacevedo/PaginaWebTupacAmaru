# Resumen de Migraciones y Decisiones Técnicas

---

## Cronología de Migraciones

```
v0.0.1 (2024) 
    │
    ▼ Migración Completa: N-Tier + EF Core + JWT + Vue 3
v0.1.0 (2026-09) 
    │
    ▼ Migración Modular: 2 APIs + Módulo Académico + SpInvoker
v1.0.0 (2026-10) ← ACTUAL
```

---

## Migración 1: v0.0.1 → v0.1.0 (Septiembre 2026)

### Objetivo
Transformar proyecto académico monolítico (Dapper, 1 entidad) en sistema institucional completo con arquitectura profesional.

### Cambios Estructurales

| Antes (v0.0.1) | Después (v0.1.0) |
|----------------|------------------|
| 1 proyecto: `MinimalApiDapper` | 4 proyectos: `Instituto.AD`, `Instituto.BR`, `Instituto.MinimalAPI`, `Instituto.MinimalAPI.Test` + Tests |
| Dapper | **EF Core 9** + `FromSqlRaw` para SPs |
| `AddSingleton<AdminRepository>` | `AddScoped` + `AddDbContext<InstitutoDbContext>` |
| Connection string hardcodeada | `appsettings.json` + `IConfiguration` |
| Password plano en BD | **BCrypt workFactor:12** + `PasswordHash` |
| 1 SP (`Logueo_Admin`) | **~15 SPs** CRUD completos |
| Sin autenticación real | **JWT Bearer** (8h) + claims + `sessionStorage` |
| 2 endpoints | **~35 endpoints** CRUD completos |
| HTML/JS vanilla | **Vue 3 + Vite + Element Plus** (19 vistas) |
| Sin tests | **44 tests** (MSTest + Moq + WebApplicationFactory) |

### Decisiones Clave v0.1.0

1. **N-Tier (AD/BR/API)** — Separación estricta: Controllers → Services → Repositories → BD
2. **EF Core + SPs** — No ORM puro; SPs son fuente de verdad, EF solo para mapeo y FromSqlRaw
3. **Repository Pattern tipado** — Una interface por entidad (`IAlumnoRepository`, no `IRepository<T>`)
4. **Result Pattern** — `ServiceResult<T>` en BR, `ApiResponse<T>` en API, `Result<T>` en v0.0.1
5. **Soft Delete solo Administradores** — `Activo` bit + UNIQUE parcial `WHERE Activo=1`
6. **Patrón ADO.NET directo en Repos** — `ExecuteScalarAsync` UNA VEZ, `FromSqlRaw` + `ToListAsync()` (nunca `FirstOrDefaultAsync` sobre EXEC)

### Breaking Changes v0.0.1 → v0.1.0
- **BD**: Esquema completamente nuevo (5 tablas vs ~3-4)
- **Auth**: Passwords hasheados (no migrables, requieren recreate)
- **Frontend**: Reescritura total (HTML/JS → Vue 3)
- **Endpoints**: Nombres y contratos cambiados

---

## Migración 2: v0.1.0 → v1.0.0 (Octubre 2026)

### Objetivo
Modularizar API monolítica, implementar módulo académico completo (catálogo + registros + inscripción pública), centralizar helpers y validaciones.

### Cambios Estructurales

| Antes (v0.1.0) | Después (v1.0.0) |
|----------------|------------------|
| 1 Minimal API (`Instituto.MinimalAPI`, 455 líneas) | **2 Minimal APIs**: `Instituto.MinimalAPI` (5127) + `Instituto.MinimalAPI.Academica` (5128) |
| ~15 SPs base | **~52 SPs** (base + 14 académicas) |
| 6 entidades | **7 entidades + 2 académicas** |
| 1 FK | **3 FKs** (incluye CASCADE) |
| `ListadoService` simple (join memoria) | **Listado inteligente** (SP N filas/alumno → BR agrupa por prioridad) |
| Validaciones dispersas | **Centralizadas**: `InfAcademicaValidator` + `InfAcademicaConstants` |
| Sin helpers SP | **SpInvoker.cs** (extension methods centralizados) |
| 2 patrones repo | **Patrón A (ADO.NET)** + **Patrón B (SpInvoker)** |

### Nuevas Capacidades v1.0.0

#### Módulo Académico Completo
- **Catálogo configurable** (`Inf_Academica`): 4 tipos seeding, habilitados/deshabilitados
- **Registros por alumno** (`Inf_Academica_Est`): FK Alumno (CASCADE) + FK Catálogo, UNIQUE compuesto
- **Inscripción pública transaccional**: Crea Alumno + N registros académicos con rollback manual si falla
- **Listado con prioridad**: SP devuelve N filas/alumno → BR elige 1 por prioridad (Título > Trámite > Constancias) + fecha reciente

#### Helpers Centralizados
```csharp
// SpInvoker.cs - Un solo lugar para patrones EF+SP
ctx.QueryEntityAsync<T>("EXEC Sp @P1", p)           // List<T>
ctx.ScalarInsertAsync("EXEC Sp_Insert @P1", p)      // int ID
ctx.NonQueryAsync("EXEC Sp_Update @Id, @P1", p)     // void
ctx.ExistsAsync("EXEC Sp_Exists @Id", p)            // bool
```

#### Validaciones Académicas Unificadas
- `InfAcademicaConstants`: Tipos, estados, catálogo (single source of truth)
- `InfAcademicaValidator`: Campos por tipo, fechas, estados, defaults
- Usado por: `InfAcademicaEstService` + `InscripcionService` (DRY)

### Breaking Changes v0.1.0 → v1.0.0

| Componente | Cambio | Impacto |
|------------|--------|---------|
| `sp_Listado_GetAll` | Columnas distintas, N filas/alumno, SIN Edad | Frontend `ListadoView` OK (usa `ListadoService`), pero DTO pierde `Edad` del SP |
| API | 1 → 2 APIs (puertos 5127 + 5128) | Frontend sigue en 5127; endpoints académicos en 5128 |
| DI | 1 `AddDbContext` → 2 (uno por API) | Configuración separada |
| Auth Académica | JWT opcional → **JWT obligatorio + AdminPolicy** | Endpoints académicos requieren token + rol |
| Config | `appsettings.json` → **User Secrets** (API Académica) | No commitear claves |
| **Listado Académico** | **Protegido (JWT+AdminPolicy) → Público (AllowAnonymous)** | Frontend puede consumir `GET /api/listado` sin token desde API Académica 5128 |

### Scripts de Migración BD

```bash
# 1. SPs base (incluye sp_Listado_GetAll v1 - antigua)
sqlcmd -S localhost -U instituto_user -P Instituto2026 -d InstitutoDB -I -i "Docs/sql/sp_stored_procedures.sql"

# 2. Tablas académicas + seeding + 14 SPs + SOBRESCRIBE sp_Listado_GetAll v2 (nueva)
sqlcmd -S localhost -U instituto_user -P Instituto2026 -d InstitutoDB -I -i "Docs/sql/04_InfAcademica.sql"
```

> **Crítico:** `04_InfAcademica.sql` ejecuta `CREATE OR ALTER PROCEDURE sp_Listado_GetAll` **después** del script base, por lo que la versión nueva (joins académicos, SIN Edad) es la que queda en BD.

---

## Decisiones Técnicas Acumuladas

| Decisión | Versión | Arquivo | Por qué |
|----------|---------|---------|---------|
| N-Tier AD/BR/API | v0.1.0 | `PROYECTO.md` | Testabilidad, separación concerns, mantenibilidad |
| EF Core + SPs (no ORM puro) | v0.1.0 | `InstitutoDbContext` | Performance SPs + tipado fuerte + migraciones futuras |
| Repository por entidad | v0.1.0 | `IRepositories.cs` | Interfaces específicas, DI tipada, sin genéricos leaky |
| Result Pattern `ServiceResult<T>` | v0.1.0 | `CommonDtos.cs` | Error handling tipado, sin exceptions para control de flujo |
| Soft Delete + UNIQUE parcial | v0.1.0 | `InstitutoDbContext` + SP | Reactivar emails, no borrar datos |
| `FromSqlRaw` + `ToListAsync()` | v0.1.0 | Repositorios | Fix bug `FirstOrDefaultAsync` sobre EXEC (non-composable) |
| `ExecuteScalarAsync` UNA VEZ | v0.1.0 | Repos `CreateAsync` | Fix bug doble inserción (Profesor, Formulario) |
| 2 Minimal APIs | v1.0.0 | `Instituto.MinimalAPI.Academica` | Separar Auth/CRUD base (público/JWT opcional) de Académico (JWT obligatorio + Policy) |
| `SpInvoker` centralizado | v1.0.0 | `SpInvoker.cs` | DRY: 6 repos usan mismos patrones EF+SP |
| Validaciones académicas centralizadas | v1.0.0 | `InfAcademicaValidator` | DRY: `InscripcionService` + `InfAcademicaEstService` comparten lógica |
| AlumnoId inmutable en Update | v1.0.0 | `InfAcademicaEstService` + SP | Regla de negocio: registro no cambia de alumno |
| CASCADE DELETE Alumno→Académico | v1.0.0 | BD `FK_InfAcademicaEst_Alumnos` | Limpieza automática, integridad referencial |
| UNIQUE compuesto (InfAca, Alumno) | v1.0.0 | BD + BR `ExistsByCombination` | Un alumno no repite tipo académico |

---

## Comandos de Verificación Post-Migración

```bash
# Verificar BD
sqlcmd -S localhost -U instituto_user -P Instituto2026 -d InstitutoDB -Q "
  SELECT COUNT(*) FROM sys.tables WHERE name LIKE '%Inf_Academ%';
  SELECT COUNT(*) FROM sys.procedures;
  SELECT * FROM Inf_Academica;
  EXEC sp_Listado_GetAll;
"

# Verificar APIs
curl http://localhost:5127/health
curl http://localhost:5128/health

# Verificar Frontend
# Abrir http://localhost:5176/login
```

---

## Checklist de Release v1.0.0

- [x] BD: 7 tablas, 3 FKs, 12 índices UNIQUE, ~52 SPs
- [x] API Principal (5127): Auth, Setup, CRUD base, Stats, Health
- [x] API Académica (5128): JWT + AdminPolicy, **Listado público**, Inscripción, CRUD Académico
- [x] Frontend (5176): 19 vistas, Route Guards, useAuth, CSS modular
- [x] Tests: 44+ passing (AD/BR/API)
- [x] Docs: `DATABASE.md` + 12 archivos modulares + `versionado/`
- [x] Scripts SQL oficiales en `Docs/sql/`
- [ ] **Pendiente**: Frontend consume API Académica (actualmente usa legacy)
- [ ] **Pendiente**: Fix `InscripciónView.vue` `CarreraId: string` → `number`
- [ ] **Pendiente**: `PasswordTemp` en SPs `GetById`/`GetByEmail` Administradores
- [ ] **Pendiente**: Alias `AS Exists` → `AS Result` en SPs

---

## Referencias Rápidas

| Documento | Ubicación |
|-----------|-----------|
| BD Completa | `Docs/DATABASE.md` |
| BD Modular | `Docs/DATABASE/` (12 archivos) |
| Scripts SQL | `Docs/sql/` |
| Proyecto v0.1.0 | `instituto/` |
| Proyecto v0.0.1 | `Rodrigo_David_Raffo_Fabio_Final_Algoritmos_3/` |
| Este versionado | `Docs/versionado/` |