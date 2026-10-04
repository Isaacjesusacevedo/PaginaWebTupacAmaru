# Stored Procedures — Inventario

> **Nota importante:** Existen **dos versiones de `sp_Listado_GetAll`** en los scripts SQL. La que queda en la BD depende del orden de ejecución (ver sección [LISTADO_GETALL.md](./LISTADO_GETALL.md)).

---

## SPs Base (script `sp_stored_procedures.sql`)

| SP | Tipo | Parámetros | Retorna | Notas |
|----|------|------------|---------|-------|
| `sp_Carreras_GetAll` | SELECT | — | Tabla | |
| `sp_Carreras_GetById` | SELECT | `@Id INT` | Tabla | |
| `sp_Carreras_Create` | INSERT | `@Nombre, @DuracionAnios, @Turno, @Modalidad, @Horario, @Estado` | `SCOPE_IDENTITY() AS Id` | |
| `sp_Carreras_Update` | UPDATE | `@Id, @Nombre, @DuracionAnios, @Turno, @Modalidad, @Horario, @Estado` | — | |
| `sp_Carreras_Delete` | DELETE | `@Id INT` | — | Hard delete |
| `sp_Carreras_Exists` | EXISTS | `@Id INT` | `AS Exists` (0/1) | ⚠️ Alias reservado |
| `sp_Alumnos_GetAll` | SELECT | — | Tabla | |
| `sp_Alumnos_GetById` | SELECT | `@Id INT` | Tabla | |
| `sp_Alumnos_Create` | INSERT | `@Nombre, @Apellido, @Email, @DNI, @FechaNacimiento, @Direccion, @Nacionalidad, @FechaInscripcion, @Telefono, @TituloSecundario, @Turno, @CarreraId` | `SCOPE_IDENTITY() AS Id` | Usa `@FechaInscripcionFinal` interno (ver bug conocido) |
| `sp_Alumnos_Update` | UPDATE | `@Id, @Nombre, @Apellido, @Email, @DNI, @FechaNacimiento, @Direccion, @Nacionalidad, @Telefono, @TituloSecundario, @Turno, @CarreraId` | — | No actualiza FechaInscripcion |
| `sp_Alumnos_Delete` | DELETE | `@Id INT` | — | Hard delete |
| `sp_Alumnos_Exists` | EXISTS | `@Id INT` | `AS Exists` (0/1) | ⚠️ Alias reservado |
| `sp_Alumnos_ExistsByDNI` | EXISTS | `@DNI INT` | `AS Exists` (0/1) | ⚠️ Alias reservado |
| `sp_Alumnos_ExistsByEmail` | EXISTS | `@Email NVARCHAR(150)` | `AS Exists` (0/1) | ⚠️ Alias reservado |
| `sp_Alumnos_ExistsByCarreraId` | EXISTS | `@CarreraId INT` | `AS Exists` (0/1) | ⚠️ Alias reservado |
| `sp_Administradores_GetAll` | SELECT | — | Tabla | **Solo `Activo = 1`** |
| `sp_Administradores_GetById` | SELECT | `@Id INT` | Tabla | **Solo `Activo = 1`** + `PasswordHash` |
| `sp_Administradores_GetByEmail` | SELECT | `@Email NVARCHAR(150)` | Tabla | **Solo `Activo = 1`** + `PasswordHash` |
| `sp_Administradores_Create` | INSERT | `@Nombre, @Apellido, @Email, @PasswordHash, @Role` | `SCOPE_IDENTITY() AS Id` | |
| `sp_Administradores_Update` | UPDATE | `@Id, @Nombre, @Apellido, @Email, @Role` | — | Solo `Activo = 1` |
| `sp_Administradores_UpdatePasswordHash` | UPDATE | `@Id, @PasswordHash` | — | |
| `sp_Administradores_SoftDelete` | UPDATE | `@Id INT` | — | `Activo = 0` |
| `sp_Administradores_ExistsByEmail` | EXISTS | `@Email NVARCHAR(150)` | `AS Exists` (0/1) | Solo `Activo = 1`, ⚠️ Alias reservado |
| `sp_Administradores_Count` | SELECT | — | `COUNT(*)` | |
| `sp_Profesores_GetAll` | SELECT | — | Tabla | |
| `sp_Profesores_GetById` | SELECT | `@Id INT` | Tabla | |
| `sp_Profesores_Create` | INSERT | `@Nombre, @Apellido, @Email, @Telefono, @Especialidad` | `SCOPE_IDENTITY() AS Id` | |
| `sp_Profesores_Update` | UPDATE | `@Id, @Nombre, @Apellido, @Email, @Telefono, @Especialidad` | — | |
| `sp_Profesores_Delete` | DELETE | `@Id INT` | — | Hard delete |
| `sp_Profesores_Exists` | EXISTS | `@Id INT` | `AS Exists` (0/1) | ⚠️ Alias reservado |
| `sp_Profesores_ExistsByEmail` | EXISTS | `@Email NVARCHAR(150)` | `AS Exists` (0/1) | ⚠️ Alias reservado |
| `sp_Formularios_GetAll` | SELECT | — | Tabla | |
| `sp_Formularios_GetById` | SELECT | `@Id INT` | Tabla | |
| `sp_Formularios_Create` | INSERT | `@Nombre, @Estado, @FechaApertura, @FechaCierre, @Descripcion` | `SCOPE_IDENTITY() AS Id` | |
| `sp_Formularios_Update` | UPDATE | `@Id, @Nombre, @Estado, @FechaApertura, @FechaCierre, @Descripcion` | — | |
| `sp_Formularios_Delete` | DELETE | `@Id INT` | — | Hard delete |
| `sp_Formularios_Exists` | EXISTS | `@Id INT` | `AS Exists` (0/1) | ⚠️ Alias reservado |
| `sp_Listado_GetAll` | SELECT (join) | — | Tabla | **VERSIÓN ANTIGUA** (ver discrepancia abajo) |
| **`sp_Listado_GetAll` (versión nueva en BD)** | SELECT (join académico) | — | Tabla | **Versión nueva** (joins Inf_Academica_Est + Inf_Academica, SIN Edad). Endpoint `GET /api/listado` en API Académica es **público (AllowAnonymous)** |

---

## SPs de Información Académica (script `04_InfAcademica.sql`)

| SP | Tipo | Parámetros | Retorna | Notas |
|----|------|------------|---------|-------|
| `Inf_Academica_List` | SELECT | — | Tabla | Catálogo completo |
| `Inf_Academica_GetById` | SELECT | `@ID_Inf_Aca INT` | Tabla | |
| `Inf_Academica_Insert` | INSERT | `@Inf_Aca_Descripcion, @Inf_Aca_Fecha, @Inf_Aca_Estado` | `SCOPE_IDENTITY() AS ID_Inf_Aca` | |
| `Inf_Academica_Update` | UPDATE | `@ID_Inf_Aca, @Inf_Aca_Descripcion, @Inf_Aca_Fecha, @Inf_Aca_Estado` | — | |
| `Inf_Academica_Delete` | DELETE | `@ID_Inf_Aca INT` | — | Hard delete |
| `Inf_Academica_Est_List` | SELECT | — | Tabla | Todos los registros |
| `Inf_Academica_Est_GetById` | SELECT | `@ID_Inf_Academica_Est INT` | Tabla | |
| `Inf_Academica_Est_GetByAlumno` | SELECT | `@ID_Est INT` | Tabla | Registros de un alumno |
| `Inf_Academica_Est_Insert` | INSERT | `@ID_Inf_Aca, @ID_Est, @Fecha_Emision, @Titulo_Secundario, @Institucion, @Estado_Titulo` | `SCOPE_IDENTITY() AS ID_Inf_Academica_Est` | |
| `Inf_Academica_Est_Update` | UPDATE | `@ID_Inf_Academica_Est, @ID_Inf_Aca, @Fecha_Emision, @Titulo_Secundario, @Institucion, @Estado_Titulo` | — | **NO recibe `ID_Est`** (alumno inmutable) |
| `Inf_Academica_Est_Delete` | DELETE | `@ID_Inf_Academica_Est INT` | — | Hard delete |
| `Inf_Academica_Est_Exists` | EXISTS | `@ID_Inf_Academica_Est INT` | `AS Exists` (0/1) | ⚠️ Alias reservado |
| `Inf_Academica_Est_ExistsByCombination` | EXISTS | `@ID_Inf_Aca, @ID_Est, @ExcludeId INT = NULL` | `AS Exists` (0/1) | Para validar duplicados en update, ⚠️ Alias reservado |
| `Inf_Academica_Est_ExistsByInfAca` | EXISTS | `@ID_Inf_Aca INT` | `AS Exists` (0/1) | Para no borrar tipos con registros, ⚠️ Alias reservado |

---

## Discrepancia Crítica: `sp_Listado_GetAll`

**En `sp_stored_procedures.sql` (línea 546-566):**
```sql
-- Versión ANTIGUA - calcula Edad en SQL, SIN joins académicos
SELECT 
    a.Id AS AlumnoId,
    a.Nombre + ' ' + a.Apellido AS NombreCompleto,
    a.DNI, a.Email,
    c.Nombre AS Carrera,
    a.Turno,
    CASE WHEN a.FechaNacimiento IS NOT NULL 
         THEN FLOOR(DATEDIFF(DAY, a.FechaNacimiento, GETDATE()) / 365.25)
         ELSE NULL END AS Edad
FROM Alumnos a LEFT JOIN Carreras c ON a.CarreraId = c.Id
```

**En `04_InfAcademica.sql` (línea 272-293):**
```sql
-- Versión NUEVA - SIN Edad, CON joins académicos (N filas por alumno)
SELECT
    a.Id AS AlumnoId,
    a.Nombre + ' ' + a.Apellido AS NombreCompleto,
    a.DNI, a.Email,
    c.Nombre AS Carrera,
    a.Turno,
    a.FechaNacimiento,
    ia.Inf_Aca_Descripcion AS TipoAcademico,
    iae.Titulo_Secundario AS TituloSecundario,
    iae.Fecha_Emision AS FechaEmision,
    iae.Estado_Titulo AS EstadoTitulo
FROM dbo.Alumnos a
LEFT JOIN dbo.Carreras c ON a.CarreraId = c.Id
LEFT JOIN dbo.Inf_Academica_Est iae ON iae.ID_Est = a.Id
LEFT JOIN dbo.Inf_Academica ia ON ia.ID_Inf_Aca = iae.ID_Inf_Aca
```

**Cuál queda en la BD:** La que se ejecute **última**. El orden recomendado es:
1. `sp_stored_procedures.sql` (crea versión antigua)
2. `04_InfAcademica.sql` (sobrescribe con versión nueva) ✅

Ver [LISTADO_GETALL.md](./LISTADO_GETALL.md) para detalle completo.

---

## Reglas de Oro de las SPs (Aplicadas en el Código)

| Regla | Implementación |
|-------|----------------|
| **Solo Data Access** | Cero validaciones de negocio en SPs; validaciones en BR (`InfAcademicaValidator`, `InfAcademicaEstService`, `InscripcionService`, `ListadoService`) |
| **`SET NOCOUNT ON`** | Al inicio de cada SP |
| **`SCOPE_IDENTITY()`** | Devuelve el ID en `INSERT` |
| **Alias seguros** | ⚠️ **INCUMPLIDO:** varias SPs usan `AS Exists` (palabra reservada). Funciona pero genera warning. Recomendado: `AS Result` |
| **Columnas requeridas por EF** | Toda columna mapeada en `InstitutoDbContext.cs` debe aparecer en el `SELECT` de las SPs de lectura |

---

## Patrones de Invocación desde Repositorios (C#)

### Patrón 1: `FromSqlRaw` + `ToListAsync()` (Lectura entidad)
```csharp
// AlumnoRepository, AdministradorRepository, CarreraRepository, ProfesorRepository, FormularioRepository
return await _context.Alumnos
    .FromSqlRaw("EXEC sp_Alumnos_GetById @Id", new SqlParameter("@Id", id))
    .ToListAsync();  // NUNCA FirstOrDefaultAsync() sobre EXEC
```

### Patrón 2: `SpInvoker.QueryEntityAsync` (Lectura entidad genérica)
```csharp
// InfAcademicaRepository, InfAcademicaEstRepository
return await _context.QueryEntityAsync<InfAcademica>("EXEC Inf_Academica_List");
```

### Patrón 3: `SqlQueryRaw` + `ToListAsync()` (Escalar/Exists)
```csharp
var result = await _context.Database
    .SqlQueryRaw<int>("EXEC sp_Alumnos_Exists @Id", new SqlParameter("@Id", id))
    .ToListAsync();
return result.FirstOrDefault() == 1;
```

### Patrón 4: `SpInvoker.ScalarInsertAsync` (INSERT con ID retorno)
```csharp
return await _context.ScalarInsertAsync(
    "EXEC Inf_Academica_Insert @Inf_Aca_Descripcion, @Inf_Aca_Fecha, @Inf_Aca_Estado",
    new SqlParameter("@Inf_Aca_Descripcion", entity.Descripcion),
    ...
);
```

### Patrón 5: `SpInvoker.NonQueryAsync` (UPDATE/DELETE)
```csharp
await _context.NonQueryAsync(
    "EXEC Inf_Academica_Est_Update @ID_Inf_Academica_Est, ...", parameters);
```

### Patrón 6: ADO.NET directo (Create/Update/Delete en repos base)
```csharp
// AlumnoRepository, AdministradorRepository, CarreraRepository, ProfesorRepository, FormularioRepository
using var command = connection.CreateCommand();
command.CommandText = "EXEC sp_Alumnos_Create @Nombre, @Apellido, ...";
command.Parameters.Add(...);
var result = await command.ExecuteScalarAsync();  // Una sola llamada
entity.Id = Convert.ToInt32(result);
```

---

## Orden de Ejecución para Recrear SPs

1. `Docs/sql/sp_stored_procedures.sql` — SPs base (incluye `sp_Listado_GetAll` versión antigua)
2. `Docs/sql/04_InfAcademica.sql` — Tablas académicas + catálogo + SPs académicas + **sobrescribe `sp_Listado_GetAll` con versión nueva**

---

## Verificar SPs Creadas

```sql
SELECT name FROM sys.procedures
WHERE name LIKE 'sp_%' OR name LIKE 'Inf_Academica%'
ORDER BY name;
```

---

## Bugs Conocidos en SPs (Documentados en LESSONS_LEARNED.md)

| SP | Bug | Impacto |
|----|-----|---------|
| `sp_Alumnos_Create` | `DECLARE @FechaInscripcion` redeclara parámetro | Funciona por precedencia, pero confuso |
| `sp_Administradores_GetById` / `GetByEmail` | No devuelve `PasswordTemp` | ❌ EF Core falla si la entidad lo tiene mapeado |
| Varias `Exists` | Alias `AS Exists` usa palabra reservada | Warning, pero funciona |