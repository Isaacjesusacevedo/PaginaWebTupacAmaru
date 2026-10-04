# Comparativa Detallada de Cambios entre Versiones

---

## Matriz de Evolución

| Categoría | v0.0.1 (Rodrigo) | v0.1.0 (Instituto) | v1.0.0 (Actual) |
|-----------|------------------|-------------------|-----------------|
| **Arquitectura** | Monolito 1 proyecto | N-Tier 3 capas (AD/BR/API) | N-Tier 4 proyectos (AD/BR/API/API.Academica) |
| **API Style** | 1 Minimal API | 1 Minimal API (455 líneas) | **2 Minimal APIs** separadas |
| **Data Access** | Dapper + SPs | EF Core + SPs (FromSqlRaw) | EF Core + SPs + **SpInvoker helpers** |
| **DI / Lifetime** | `AddSingleton` (❌ thread-safety) | `AddScoped` + `AddDbContext` | `AddScoped` + `AddDbContext` (x2) |
| **Configuración** | Hardcodeada en Program.cs | `appsettings.json` + `IConfiguration` | `appsettings.json` + **User Secrets** (API Academica) |
| **Autenticación** | SP con OUTPUT param (texto plano) | **JWT + BCrypt** (workFactor:12) | JWT + BCrypt + **AdminPolicy** (roles) |
| **Frontend** | HTML/JS vanilla (2 páginas) | **Vue 3 + Vite + Element Plus** (19 vistas) | Vue 3 (igual, conecta a API principal) |

---

## Entidades / Modelos

| Entidad | v0.0.1 | v0.1.0 | v1.0.0 |
|---------|--------|--------|--------|
| `Administrador` | 6 campos, password plano | Hereda `Persona`: +Role, PasswordHash, Activo, PasswordTemp, FechaCreacion | **Igual** (soft delete, UNIQUE parcial Email) |
| `Alumno` | ❌ | **Nueva**: 14 cols, DNI/Email UNIQUE, FK Carrera, Edad computed | **Igual** + legacy `TituloSecundario` |
| `Carrera` | ❌ | **Nueva**: 8 cols, DuracionAnios 1-10 | **Igual** |
| `Profesor` | ❌ | **Nueva**: 7 cols, Email UNIQUE | **Igual** |
| `Formulario` | ❌ | **Nueva**: 7 cols, Estados Borrador/Abierto/Cerrado | **Igual** |
| `ListadoItem` | ❌ | **Nueva DTO**: Join Alumno+Carrera + Edad | **Igual** (pero SP cambió) |
| `InfAcademica` | ❌ | ❌ | **NUEVA**: Catálogo 4 tipos (HABILITADO/DESHABILITADO) |
| `InfAcademicaEst` | ❌ | ❌ | **NUEVA**: Registros por alumno (FK Alumno CASCADE, FK Catálogo) |
| `Persona` (abstract) | ❌ | **NUEVA**: Id, Nombre, Apellido, Email | **Igual** |

---

## Base de Datos

| Métrica | v0.0.1 | v0.1.0 | v1.0.0 |
|---------|--------|--------|--------|
| Tablas | ~3-4 | 5 | **7** |
| Columnas totales | ~20 | 45 | **56** |
| FKs | 0 | 1 | **3** |
| Índices UNIQUE | 0 | 7 | **12** (1 parcial) |
| Registros seed | 0 | 17 | 17 + 4 catálogo |
| Soft delete | ❌ | ✅ Administradores | ✅ Administradores |
| CASCADE DELETE | ❌ | ❌ | ✅ Inf_Academica_Est → Alumnos |

### Esquema Tablas v1.0.0
```sql
-- 7 Tablas
Administradores (9 cols, PK, UNIQUE parcial Email WHERE Activo=1)
Alumnos (14 cols, PK, UNIQUE Email, UNIQUE DNI, FK CarreraId)
Carreras (8 cols, PK)
Profesores (7 cols, PK, UNIQUE Email)
Formularios (7 cols, PK)
Inf_Academica (4 cols, PK)           -- NUEVA
Inf_Academica_Est (7 cols, PK, 2 FKs, UNIQUE compuesto) -- NUEVA
```

---

## Stored Procedures

| Aspecto | v0.0.1 | v0.1.0 | v1.0.0 |
|---------|--------|--------|--------|
| Total SPs | 2 | ~15 | **~52** |
| ORM usado | Dapper | EF Core FromSqlRaw | EF Core + **SpInvoker** |
| Patrones C# | 1 (Dapper directo) | 1 (ADO.NET directo) | **2 Patrones** (A: ADO.NET, B: SpInvoker) |
| `sp_Listado_GetAll` | 1 (estudiantes+carreras+info aca) | 1 (join simple + Edad en SQL) | **2 versiones en scripts** → Nueva en BD (joins académicos, SIN Edad) |

### Discrepancia Crítica `sp_Listado_GetAll`
| Script | Versión | Estado en BD |
|--------|---------|--------------|
| `sp_stored_procedures.sql` | Antigua (Edad en SQL, sin joins académicos) | ❌ Sobrescrita |
| `04_InfAcademica.sql` | **Nueva** (joins Inf_Academica_Est + Inf_Academica, SIN Edad) | ✅ **Actual** |

---

## Servicios (BR Layer)

| Servicio | v0.0.1 | v0.1.0 | v1.0.0 |
|----------|--------|--------|--------|
| `AdministradorService` | Login simple + Exportar Excel | CRUD + Auth (BCrypt, JWT, ChangePassword, Setup) | **Igual** |
| `AlumnoService` | ❌ | CRUD + Validaciones (DNI, Email, Carrera) | **Igual** |
| `CarreraService` | ❌ | CRUD + Validación no eliminar con alumnos | **Igual** |
| `ProfesorService` | ❌ | CRUD + Email único | **Igual** |
| `FormularioService` | ❌ | CRUD + Validación fechas | **Igual** |
| `ListadoService` | ❌ | Join memoria Alumno+Carrera + Edad | **NUEVA LÓGICA**: Agrupa N filas/alumno, prioridad académica, Edad en BR |
| `InfAcademicaService` | ❌ | ❌ | **NUEVO**: CRUD Catálogo + validación no borrar con registros |
| `InfAcademicaEstService` | ❌ | ❌ | **NUEVO**: CRUD Registros + AlumnoId inmutable + validaciones por tipo |
| `InscripcionService` | ❌ | ❌ | **NUEVO**: Inscripción pública (Alumno + N académicos transaccional) |

---

## Validaciones Académicas (NUEVAS v1.0.0)

Centralizadas en `InfAcademicaValidator.cs` + `InfAcademicaConstants.cs`:

| Validación | Dónde | Lógica |
|------------|-------|--------|
| Tipo habilitado | Create/Update/Inscripcion | `Estado == HABILITADO` |
| Alumno existe | Create/Inscripcion | `AlumnoRepository.ExistsAsync` |
| No duplicado tipo/alumno | Create/Update | `ExistsByCombinationAsync` (UNIQUE BD + check BR) |
| Campos por tipo | Create/Update/Inscripcion | **Título**: TituloSecundario obligatorio (≤100) <br> **Título en trámite**: Institucion obligatoria (≤150) <br> **Constancias**: ambos null |
| Estado por defecto | Create | **Trámite** → TRAMITE, otros → MANO |
| Fecha emisión | Create/Update | No futura salvo TRAMITE |
| No borrar tipo con registros | `InfAcademicaService.DeleteAsync` | `ExistsByInfAcademicaAsync` |

---

## Endpoints API

| Módulo | v0.0.1 | v0.1.0 | v1.0.0 |
|--------|--------|--------|--------|
| Auth | 1 (`/api/admin/login`) | 2 (`/api/auth/login`, `/verify-password`) | 2 (API Principal) |
| Setup | ❌ | 2 (`/setup/admin`, `/setup/status`) | 2 (API Principal) |
| Carreras | ❌ | 5 (CRUD) | 5 (API Principal) |
| Alumnos | ❌ | 5 (CRUD + POST público) | 5 (API Principal) |
| Administradores | ❌ | 6 (CRUD + ChangePassword) | 6 (API Principal) |
| Profesores | ❌ | 5 (CRUD) | 5 (API Principal) |
| Formularios | ❌ | 5 (CRUD) | 5 (API Principal) |
| Listado | 1 (`/exportar-estudiantes`) | 1 (`/api/listado` legacy) | **3**: Legacy (API Ppal) + **Académico (público)** + Inscripción Pública |
| Info Académica | ❌ | ❌ | **9** (Catálogo 5 + Registros 4) |
| Inscripción Pública | ❌ | POST `/api/alumnos` (simple) | **POST `/api/inscripcion`** (Alumno + N académicos) |
| Stats/Health | ❌ | 2 | 2 (API Principal) |
| **Total** | **2** | **~35** | **~50** |

---

## Frontend (Vue 3)

| Aspecto | v0.0.1 | v0.1.0 | v1.0.0 |
|---------|--------|--------|--------|
| Framework | HTML/JS vanilla | **Vue 3 + Vite + Element Plus** | **Igual** |
| Vistas | 2 (login, admin) | **19** | 19 |
| Router Guards | ❌ | `requiereAuth`, `soloInvitado` | **Igual** |
| Auth Storage | ❌ | `sessionStorage` (token + admin) | **Igual** |
| Re-autenticación | ❌ | Modal en EditarAdmin | **Igual** |
| CSS | Inline/embebido | **Modular BEM-like** (assets/css/) | **Igual** |
| API Base URL | Hardcodeada | `VITE_API_URL` (.env) | **Igual** (apunta a API Principal 5127) |
| Integración API Académica | ❌ | ❌ | **Implementada** (`useAcademicaApi.js` → `GET /api/listado` público, `POST /api/inscripcion` público) |

---

## Tests

| Proyecto | v0.0.1 | v0.1.0 | v1.0.0 |
|----------|--------|--------|--------|
| AD Test | 0 | 20 | 20+ |
| BR Test | 0 | 17 | 17+ |
| API Test | 0 | 7 | 7+ |
| **Total** | **0** | **44** | **44+** |

---

## Documentación

| Archivo | v0.0.1 | v0.1.0 | v1.0.0 |
|---------|--------|--------|--------|
| README.md | Básico | Completo | **Actualizado** (2 APIs, módulo académico) |
| PROYECTO.md | ❌ | Completo (v1.1.0) | **Actualizado** |
| DATABASE.md | ❌ | Básico (5 tablas) | **Completo** (7 tablas, 3 FKs, ~52 SPs) |
| Docs/DATABASE/ | ❌ | Parcial | **12 archivos modulares** |
| Docs/sql/ | 1 archivo (.sql suelto) | 1 archivo | **2 archivos oficiales** |
| Versionado | ❌ | ❌ | **✅ Esta carpeta** |

---

## Migraciones de Datos / Breaking Changes

### v0.0.1 → v0.1.0
- **BD completa reescrita**: Nueva estructura 5 tablas, datos de prueba insertados
- **Auth**: Password plano → BCrypt hash (requiere recrear admins)
- **Frontend**: Reescrito completo (HTML/JS → Vue 3)

### v0.1.0 → v1.0.0
- **BD**: +2 tablas (`Inf_Academica`, `Inf_Academica_Est`), +2 FKs, +1 UNIQUE compuesto
- **SP `sp_Listado_GetAll`**: Cambio breaking (columnas distintas, N filas/alumno)
  - Frontend `ListadoView` compatible porque usa `ListadoService` que procesa ambas
  - **Pero** `AlumnoListadoDto` pierde `Edad` del SP (ahora se calcula en BR)
- **API**: Separada en 2 (puerto 5127 + 5128)
  - Frontend sigue apuntando a 5127 (API Principal)
  - Endpoints académicos en 5128 requieren JWT + AdminPolicy
- **DI**: 2 `AddDbContext` (uno por API)
- **Config**: API Académica usa User Secrets (no appsettings.json)

---

## Checklist de Migración v0.1.0 → v1.0.0

- [x] Ejecutar `04_InfAcademica.sql` (crea tablas + SPs académicas + sobrescribe `sp_Listado_GetAll`)
- [x] Verificar `SpInvoker.cs` registrado en DI (extensión `InstitutoDbContext`)
- [x] Registrar repositorios académicos en ambas APIs
- [x] Registrar servicios académicos en API Académica
- [x] Configurar JWT + AdminPolicy en API Académica
- [x] User Secrets para Jwt:Key en API Académica
- [x] CORS permitido para `http://localhost:5176` en API Académica
- [x] Actualizar `Frontend/.env` si se usa API Académica directamente
- [ ] **Pendiente**: Frontend consume endpoints académicos (actualmente usa API Principal legacy)
- [ ] **Pendiente**: Corregir `InscripciónView.vue` `CarreraId: string` → `number`

---

## Decisiones Técnicas Clave

| Decisión | Versión | Justificación |
|----------|---------|---------------|
| N-Tier (AD/BR/API) | v0.1.0 | Separación de responsabilidades, testabilidad |
| EF Core + SPs | v0.1.0 | Performance SPs + tipado EF, migraciones futuras |
| 2 Minimal APIs | v1.0.0 | Separación de concerns: Auth/CRUD base vs Académico (JWT obligatorio) |
| SpInvoker helpers | v1.0.0 | DRY: centralizar patrones `FromSqlRaw`+`ToListAsync`, `ScalarInsertAsync` |
| Validaciones centralizadas | v1.0.0 | `InfAcademicaValidator` usado por `InscripcionService` + `InfAcademicaEstService` |
| AlumnoId inmutable en Update | v1.0.0 | Regla de negocio: registro académico no cambia de alumno |
| Soft delete solo Administradores | v0.1.0 | Simplicidad; académico usa CASCADE |
| UNIQUE parcial Email (Activo=1) | v0.1.0 | Permite reactivar emails de admins borrados |

---

## Deuda Técnica por Versión

| Item | Introducida en | Resuelta en | Estado Actual |
|------|----------------|-------------|---------------|
| Password plano | v0.0.1 | v0.1.0 (BCrypt) | ✅ |
| Singleton Repository | v0.0.1 | v0.1.0 (Scoped) | ✅ |
| Connection string hardcodeada | v0.0.1 | v0.1.0 (appsettings) | ✅ |
| Sin JWT | v0.0.1 | v0.1.0 | ✅ |
| `sp_Listado_GetAll` Edad en SQL | v0.1.0 | v1.0.0 (Edad en BR) | ✅ |
| Sin módulo académico | v0.1.0 | v1.0.0 | ✅ |
| API monolítica | v0.1.0 | v1.0.0 (2 APIs) | ✅ |
| `PasswordTemp` no en SPs GetById/GetByEmail | v0.1.0 | — | ⚠️ **Pendiente Alta** |
| Alias `AS Exists` en SPs | v0.1.0 | — | ⚠️ Pendiente Media |
| `InscripciónView.vue` `CarreraId: string` | v0.1.0 | — | 🔴 **Pendiente Crítica** |
| Profesores sin Frontend | v0.1.0 | — | ⚠️ Pendiente Media |
| Frontend no usa API Académica | v1.0.0 | — | ⚠️ Pendiente Media |

---

## Métricas de Código (Aproximadas)

| Métrica | v0.0.1 | v0.1.0 | v1.0.0 |
|---------|--------|--------|--------|
| Líneas C# (backend) | ~200 | ~3,500 | ~4,500 |
| Archivos .cs | 8 | 45 | 55 |
| Endpoints | 2 | 35 | 50 |
| SPs | 2 | 15 | 52 |
| Tablas BD | 3-4 | 5 | 7 |
| Tests | 0 | 44 | 44+ |
| Vistas Frontend | 2 | 19 | 19 |