# Lecciones Aprendidas (Integración EF Core + SPs)

Bugs encontrados durante la integración del módulo de información académica y refactor de repositorios. Documentados para no repetirlos.

---

## Bugs Históricos (Ya Corregidos)

| Bug | Causa | Solución | Estado |
|-----|-------|----------|--------|
| `'FromSql' or 'SqlQuery' was called with non-composable SQL` | `.FirstOrDefaultAsync()` sobre `EXEC` | Cambiar a `.ToListAsync()` + `.FirstOrDefault()` en memoria | ✅ Corregido |
| `The required column 'PasswordTemp' was not present` | SP no devolvía una columna mapeada por EF | Agregar la columna al `SELECT` de la SP | ✅ Corregido |
| `Sintaxis incorrecta cerca de 'Exists'` | `Exists` es palabra reservada en SQL Server | Usar `AS Result` como alias (pendiente en SPs) | ⚠️ Parcial |
| `@FechaInscripcion` ya declarado | `DECLARE` con el mismo nombre que un parámetro | Usar nombre distinto (`@FechaInscripcionFinal`) | ✅ En SP |
| Doble creación de registros (Profesor, Formulario) | `ExecuteScalarAsync()` llamado 2 veces en `CreateAsync` | Eliminar la segunda llamada | ✅ Corregido |
| `QUOTED_IDENTIFIER` error en DELETE | Índice UNIQUE filtrado requiere `QUOTED_IDENTIFIER ON` | Ejecutar con `-I` o `SET QUOTED_IDENTIFIER ON` | ✅ Documentado |

---

## Reglas Derivadas (Obligatorias)

1. **NUNCA** uses `FirstOrDefaultAsync()` / `SingleOrDefaultAsync()` sobre `FromSqlRaw`/`SqlQueryRaw` con `EXEC`. Siempre `ToListAsync()` + `FirstOrDefault()` / `SingleOrDefault()` en memoria.

2. **TODA** SP que devuelva una entidad **debe incluir todas las columnas mapeadas** en `OnModelCreating` (aunque sean `NULL`).

3. Los alias de columnas en SPs **no pueden coincidir con palabras reservadas** de SQL Server (`Exists`, `Key`, `Order`, etc.). Usar `AS Result`, `AS Existe`, etc.

4. Los parámetros y las variables `DECLARE` **no pueden compartir nombre** en el mismo SP.

5. Llamar `ExecuteScalarAsync()` **una sola vez** por operación `CreateAsync`.

6. Usar `SET NOCOUNT ON` al inicio de **cada** SP.

7. Usar `SCOPE_IDENTITY()` (no `@@IDENTITY`) para retornar ID en `INSERT`.

---

## Patrones Actuales de Repositorios (2026-10-02)

### Patrón A: Repositorios Base (ADO.NET directo para Create/Update/Delete)
**Archivos:** `AlumnoRepository`, `AdministradorRepository`, `CarreraRepository`, `ProfesorRepository`, `FormularioRepository`

```csharp
// CreateAsync - ADO.NET directo, ExecuteScalarAsync UNA VEZ
public async Task<T> CreateAsync(T entity)
{
    var connection = _context.Database.GetDbConnection();
    await connection.OpenAsync();
    using var command = connection.CreateCommand();
    command.CommandText = "EXEC sp_Tabla_Create @Param1, @Param2, ...";
    command.Parameters.Add(new SqlParameter("@Param1", entity.Prop1));
    // ...
    var result = await command.ExecuteScalarAsync();  // UNA SOLA VEZ
    entity.Id = Convert.ToInt32(result);
    return entity;
}

// GetByIdAsync - FromSqlRaw + ToListAsync + FirstOrDefault()
public async Task<T?> GetByIdAsync(int id)
{
    var result = await _context.Set<T>()
        .FromSqlRaw("EXEC sp_Tabla_GetById @Id", new SqlParameter("@Id", id))
        .ToListAsync();
    return result.FirstOrDefault();
}

// ExistsAsync - SqlQueryRaw + ToListAsync + FirstOrDefault()
public async Task<bool> ExistsAsync(int id)
{
    var result = await _context.Database
        .SqlQueryRaw<int>("EXEC sp_Tabla_Exists @Id", new SqlParameter("@Id", id))
        .ToListAsync();
    return result.FirstOrDefault() == 1;
}
```

### Patrón B: Repositorios Académicos (SpInvoker helpers)
**Archivos:** `InfAcademicaRepository`, `InfAcademicaEstRepository`

```csharp
// Usa extension methods de SpInvoker.cs
public async Task<List<InfAcademica>> GetAllAsync()
    => await _context.QueryEntityAsync<InfAcademica>("EXEC Inf_Academica_List");

public async Task<InfAcademica?> GetByIdAsync(int id)
{
    var items = await _context.QueryEntityAsync<InfAcademica>(
        "EXEC Inf_Academica_GetById @ID_Inf_Aca", new SqlParameter("@ID_Inf_Aca", id));
    return items.FirstOrDefault();
}

public async Task<int> CreateAsync(InfAcademica entity)
    => await _context.ScalarInsertAsync(
        "EXEC Inf_Academica_Insert @Inf_Aca_Descripcion, @Inf_Aca_Fecha, @Inf_Aca_Estado",
        new SqlParameter("@Inf_Aca_Descripcion", entity.Descripcion),
        new SqlParameter("@Inf_Aca_Fecha", entity.Fecha),
        new SqlParameter("@Inf_Aca_Estado", entity.Estado));

public async Task UpdateAsync(InfAcademica entity)
    => await _context.NonQueryAsync(
        "EXEC Inf_Academica_Update @ID_Inf_Aca, @Inf_Aca_Descripcion, @Inf_Aca_Fecha, @Inf_Aca_Estado",
        new SqlParameter("@ID_Inf_Aca", entity.Id),
        new SqlParameter("@Inf_Aca_Descripcion", entity.Descripcion),
        new SqlParameter("@Inf_Aca_Fecha", entity.Fecha),
        new SqlParameter("@Inf_Aca_Estado", entity.Estado));
```

### SpInvoker.cs (Helper Centralizado)

```csharp
public static class SpInvoker
{
    // SELECT entidad -> List<T>
    public static async Task<List<T>> QueryEntityAsync<T>(this InstitutoDbContext context, 
        string sql, params SqlParameter[] parameters) where T : class
        => await context.Set<T>().FromSqlRaw(sql, parameters).ToListAsync();

    // EXISTS -> bool
    public static async Task<bool> ExistsAsync(this InstitutoDbContext context, 
        string sql, params SqlParameter[] parameters)
    {
        var rows = await context.Database.SqlQueryRaw<int>(sql, parameters).ToListAsync();
        return rows.Count > 0 && rows[0] == 1;
    }

    // INSERT con SCOPE_IDENTITY -> int ID
    public static async Task<int> ScalarInsertAsync(this InstitutoDbContext context, 
        string sql, params SqlParameter[] parameters)
    {
        await context.Database.OpenConnectionAsync();
        try
        {
            using var command = context.Database.GetDbConnection().CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddRange(parameters);
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }
        finally
        {
            await context.Database.CloseConnectionAsync();
        }
    }

    // UPDATE/DELETE -> void
    public static async Task NonQueryAsync(this InstitutoDbContext context, 
        string sql, params SqlParameter[] parameters)
        => await context.Database.ExecuteSqlRawAsync(sql, parameters);
}
```

---

## Validaciones de Negocio (BR) — Dónde Están

| Validación | Clase / Método |
|------------|----------------|
| Tipo académico existe y está habilitado | `InfAcademicaEstService.CreateAsync` + `InscripcionService.InscribirAsync` |
| Alumno existe | `InfAcademicaEstService.CreateAsync` + `InscripcionService.InscribirAsync` |
| No duplicar tipo por alumno | `InfAcademicaEstService.CreateAsync/UpdateAsync` → `ExistsByCombinationAsync` |
| Campos obligatorios por tipo (Título/Título en trámite/Constancias) | `InfAcademicaValidator.ResolverCamposPorTipo` |
| Estado válido (TRAMITE/MANO/PAUSA) | `InfAcademicaValidator.ValidarEstado` |
| Fecha emisión (no futura salvo TRAMITE) | `InfAcademicaValidator.ValidarFechaEmision` |
| Estado por defecto según tipo | `InfAcademicaValidator.ResolverEstadoPorDefecto` |
| Prioridad de tipo académico en listado | `ListadoService.PrioridadTipoAcademico` |
| Cálculo de edad | `ListadoService.CalcularEdad` / `Alumno.Edad` (propiedad computada) |

---

## Repositorios Corregidos (2026-10-02)

| Archivo | Métodos Corregidos (Patrón ToListAsync + FirstOrDefault) |
|---------|----------------------------------------------------------|
| `AdministradorRepository.cs` | `GetByIdAsync`, `GetByEmailAsync`, `ExistsAsync`, `ExistsByEmailAsync`, `CountAsync` |
| `AlumnoRepository.cs` | `GetByIdAsync`, `ExistsAsync`, `ExistsByDNIAsync`, `ExistsByEmailAsync`, `ExistsByCarreraIdAsync` |
| `CarreraRepository.cs` | `GetByIdAsync`, `ExistsAsync` |
| `ProfesorRepository.cs` | `GetByIdAsync`, `ExistsAsync`, `ExistsByEmailAsync` + fix doble `ExecuteScalarAsync()` en `CreateAsync` |
| `FormularioRepository.cs` | `GetByIdAsync`, `ExistsAsync` + fix doble `ExecuteScalarAsync()` en `CreateAsync` |

---

## Pendientes Conocidos

| Item | Descripción | Prioridad |
|------|-------------|-----------|
| Alias `AS Exists` en SPs | Cambiar a `AS Result` en todas las SPs `*_Exists*` | Media |
| `sp_Administradores_GetById` / `GetByEmail` | No devuelven `PasswordTemp` (requerido por EF) | Alta |
| `sp_Alumnos_Create` | `DECLARE @FechaInscripcion` redeclara parámetro (funciona pero confuso) | Baja |
| Navigation properties FK | `InfAcademicaEst` no tiene navigation a `Alumno` ni `InfAcademica` en EF | Media |
| Soft delete automático | `Administradores` soft delete no se propaga a entidades relacionadas | Baja |

---

## Fixes Frontend (2026-10-03)

### Fix: Verificación de Contraseña en EditarAdministradorView.vue

**Problema:** El endpoint `POST /api/auth/verify-password` requiere `email` y `password`, pero el frontend enviaba el email del admin que se está editando (que está vacío al momento de la verificación) en lugar del email del admin logueado.

**Causa:** La verificación de seguridad se ejecuta **antes** de cargar los datos del admin a editar (`loadAdminData()`), por lo que `admin.email` estaba vacío.

**Solución:** Usar el admin logueado (guardado en `sessionStorage` via `useAuth().getAdmin()`) que es quien realiza la acción.

**Código corregido en `EditarAdministradorView.vue`:**
```javascript
import { useAuth } from '@/composables/useAuth'
const { getAdmin } = useAuth()

const confirmReauth = async () => {
  // ...
  const loggedAdmin = getAdmin()  // Admin logueado (quien hace la acción)
  if (!loggedAdmin?.email) {
    reauthError.value = 'No se pudo obtener el email del administrador logueado. Cerrá sesión y volvé a entrar.'
    return
  }
  // ...
  body: JSON.stringify({ 
    email: loggedAdmin.email,      // Email del admin LOGUEADO
    password: reauthPassword.value 
  })
}
```

**Archivo modificado:** `Frontend/src/views/administradores/EditarAdministradorView.vue`
**Fecha:** 2026-10-03