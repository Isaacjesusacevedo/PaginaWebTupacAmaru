# Capa de Servicios

## Arquitectura de Servicios

```
┌─────────────────────────────────────────────────────────────────┐
│                      ICrudJsonService<T>                        │
│  GetAll() | GetById(id) | Create(e) | Update(id,e) | Delete(id) │
└────────────────────────────┬────────────────────────────────────┘
                             │
         ┌───────────────────┼───────────────────┐
         ▼                   ▼                   ▼
┌─────────────────┐ ┌─────────────────┐ ┌─────────────────┐
│AlumnoSqlServer  │ │CarreraSqlServer │ │ProfesorSqlServer│
│    Service      │ │    Service      │ │    Service      │
└────────┬────────┘ └────────┬────────┘ └────────┬────────┘
         │                   │                   │
         └───────────────────┼───────────────────┘
                             ▼
                  ┌─────────────────────┐
                  │ SqlServerBaseService │
                  │  (Template Methods)  │
                  └──────────┬──────────┘
                             │
                    ┌────────┴────────┐
                    ▼                 ▼
            ┌─────────────┐   ┌─────────────┐
            │  SqlClient  │   │  BCrypt     │
            │ (ADO.NET)   │   │ (Hashing)   │
            └─────────────┘   └─────────────┘
```

## Registro en DI Container

**Archivo**: `Program.cs` (lines 72-75)

```csharp
builder.Services.AddScoped<ICrudJsonService<Carrera>,       CarreraSqlServerService>();
builder.Services.AddScoped<ICrudJsonService<Alumno>,        AlumnoSqlServerService>();
builder.Services.AddScoped<ICrudJsonService<Administrador>, AdministradorSqlServerService>();
builder.Services.AddScoped<ICrudJsonService<Profesor>,      ProfesorSqlServerService>();

builder.Services.AddScoped<IAdminAuthService, AdminAuthService>();
```

- **Scoped**: Una instancia por request HTTP
- **Interfaces**: Desacopla controllers de implementación concreta
- **Genérico**: `ICrudJsonService<T>` reutilizable para cualquier entidad

---

## SqlServerBaseService (Clase Base)

**Archivo**: `Services/SqlServerBaseService.cs`

### Responsabilidades
- Gestión de conexiones (`OpenConnection()`)
- Ejecución de queries parameterizadas
- Mapeo `SqlDataReader → Entidad` (abstracto)
- Manejo de excepciones → `PersistenceException`

### Métodos Protegidos (Template Methods)

| Método | Uso | Retorna |
|--------|-----|---------|
| `ExecuteQuery(sql, params?)` | SELECT múltiple | `List<T>` |
| `ExecuteQuerySingle(entityName, id, sql, params?)` | SELECT único | `T` (lanza `EntityNotFoundException`) |
| `ExecuteScalar(sql, params)` | INSERT con RETURNING / COUNT | `int` |
| `ExecuteNonQuery(sql, params)` | UPDATE / DELETE | `int` (rows affected) |

### Método Abstracto (Implementar en Subclase)

```csharp
protected abstract T MapReaderToEntity(SqlDataReader reader);
```

**Ejemplo implementación** (`AdministradorSqlServerService`):
```csharp
protected override Administrador MapReaderToEntity(SqlDataReader reader)
{
    return new Administrador
    {
        Id = reader.GetInt32(reader.GetOrdinal("Id")),
        Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
        Apellido = reader.GetString(reader.GetOrdinal("Apellido")),
        Email = reader.GetString(reader.GetOrdinal("Email")),
        Role = reader.GetString(reader.GetOrdinal("Role"))
    };
}
```

### Manejo de NULLs en Lectura
```csharp
Direccion = reader.IsDBNull(reader.GetOrdinal("Direccion")) 
    ? null 
    : reader.GetString(reader.GetOrdinal("Direccion")),
```

---

## Servicios CRUD Específicos

### AdministradorSqlServerService
**Archivo**: `Services/AdministradorSqlServerService.cs`

| Método | SQL | Notas |
|--------|-----|-------|
| `GetAll()` | `SELECT ... WHERE Activo = 1 ORDER BY Id` | Soft delete filter |
| `GetById(id)` | `SELECT ... WHERE Id = @Id AND Activo = 1` | |
| `Create(entity)` | `INSERT ... VALUES (@Nombre, @Apellido, @Email, @PasswordHash, @Role, 1)` | Hash BCrypt work factor 12 |
| `Update(id, entity)` | `UPDATE ... SET Nombre=@Nombre... WHERE Id=@Id AND Activo=1` | |
| `Delete(id)` | `UPDATE ... SET Activo=0 WHERE Id=@Id AND Activo=1` | **Soft delete** |

**PasswordTemp**: Se usa solo en `Create` para hashear. No se persiste.

---

### AlumnoSqlServerService
**Archivo**: `Services/AlumnoSqlServerService.cs`

| Método | SQL | Notas |
|--------|-----|-------|
| `GetAll()` | `SELECT ... ORDER BY Id` | Todos los campos |
| `GetById(id)` | `SELECT ... WHERE Id = @Id` | |
| `Create(entity)` | `INSERT 12 columnas + CarreraId` | `FechaInscripcion` default NOW |
| `Update(id, entity)` | `UPDATE 12 columnas WHERE Id=@Id` | |
| `Delete(id)` | `DELETE FROM Alumnos WHERE Id=@Id` | **Hard delete** |

**NULL handling**: `(object?)prop ?? DBNull.Value` para opcionales.

---

### CarreraSqlServerService
**Archivo**: `Services/CarreraSqlServerService.cs`

| Método | SQL | Notas |
|--------|-----|-------|
| `GetAll()` | `SELECT ... ORDER BY Id` | |
| `GetById(id)` | `SELECT ... WHERE Id = @Id` | |
| `Create(entity)` | `INSERT 6 columnas` | `Estado` default 'Activa' |
| `Update(id, entity)` | `UPDATE 6 columnas WHERE Id=@Id` | |
| `Delete(id)` | `DELETE FROM Carreras WHERE Id=@Id` | **Hard delete** (validado en controller) |

---

### ProfesorSqlServerService
**Archivo**: `Services/ProfesorSqlServerService.cs`

| Método | SQL | Notas |
|--------|-----|-------|
| `GetAll()` | `SELECT ... ORDER BY Id` | |
| `GetById(id)` | `SELECT ... WHERE Id = @Id` | |
| `Create(entity)` | `INSERT 5 columnas` | |
| `Update(id, entity)` | `UPDATE 5 columnas WHERE Id=@Id` | |
| `Delete(id)` | `DELETE FROM Profesores WHERE Id=@Id` | **Hard delete** |

---

## AdminAuthService (Autenticación)

**Archivo**: `Services/AdminAuthService.cs`  
**Interface**: `IAdminAuthService`

```csharp
public interface IAdminAuthService
{
    Task<AdminResult?> LoginAsync(string email, string password);
    Task<bool> HayAdminsAsync();
    Task CrearAdminAsync(string nombre, string apellido, string email, string password, string role);
}
```

### Implementación Detallada

#### LoginAsync
```csharp
public async Task<AdminResult?> LoginAsync(string email, string password)
{
    using var conn = new SqlConnection(_connectionString);
    await conn.OpenAsync();

    using var cmd = new SqlCommand(
        @"SELECT Id, Nombre, Apellido, Email, PasswordHash, Role
          FROM Administradores
          WHERE Email = @Email AND Activo = 1", conn);
    cmd.Parameters.AddWithValue("@Email", email.Trim().ToLower());

    using var reader = await cmd.ExecuteReaderAsync();
    if (!await reader.ReadAsync()) return null;

    var hash = reader.GetString(reader.GetOrdinal("PasswordHash"));
    if (!BCrypt.Net.BCrypt.Verify(password, hash)) return null;

    return new AdminResult(
        reader.GetInt32(reader.GetOrdinal("Id")),
        reader.GetString(reader.GetOrdinal("Nombre")),
        reader.GetString(reader.GetOrdinal("Apellido")),
        reader.GetString(reader.GetOrdinal("Email")),
        reader.GetString(reader.GetOrdinal("Role"))
    );
}
```

- **Case-insensitive email**: `.Trim().ToLower()`
- **Solo activos**: `WHERE Activo = 1`
- **BCrypt verify**: Compara password plano vs hash almacenado

#### HayAdminsAsync
```csharp
public async Task<bool> HayAdminsAsync()
{
    using var conn = new SqlConnection(_connectionString);
    await conn.OpenAsync();
    using var cmd = new SqlCommand("SELECT COUNT(*) FROM Administradores", conn);
    var count = (long)(await cmd.ExecuteScalarAsync())!;
    return count > 0;
}
```
- Usado por `SetupController` para permitir/denegar creación inicial

#### CrearAdminAsync
```csharp
public async Task CrearAdminAsync(string nombre, string apellido, string email, string password, string role)
{
    var hash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);

    using var conn = new SqlConnection(_connectionString);
    await conn.OpenAsync();

    using var cmd = new SqlCommand(
        @"INSERT INTO Administradores (Nombre, Apellido, Email, PasswordHash, Role, Activo)
          VALUES (@Nombre, @Apellido, @Email, @PasswordHash, @Role, 1)", conn);

    cmd.Parameters.AddWithValue("@Nombre", nombre.Trim());
    cmd.Parameters.AddWithValue("@Apellido", apellido.Trim());
    cmd.Parameters.AddWithValue("@Email", email.Trim().ToLower());
    cmd.Parameters.AddWithValue("@PasswordHash", hash);
    cmd.Parameters.AddWithValue("@Role", role);

    await cmd.ExecuteNonQueryAsync();
}
```
- **Work factor 12**: Balance seguridad/performance (2024+)
- **Activo = 1**: Por defecto activo

---

## CrudJsonService (Legado - JSON Files)

**Archivo**: `Services/CrudJsonService.cs`  
**Estado**: **NO USADO** (reemplazado por SqlServer services)

- Persistencia en archivos `.json` locales
- `SemaphoreSlim` para concurrencia
- Útil para desarrollo sin BD / testing
- Mantenido por compatibilidad / referencia

---

## Buenas Prácticas en Servicios

| Práctica | Ejemplo |
|----------|---------|
| **Parámetros SQL** | Siempre `@Param`, nunca string interpolation |
| **Async/Await** | Todas las operaciones I/O |
| **Using** | `SqlConnection`, `SqlCommand`, `SqlDataReader` |
| **Excepciones tipadas** | `EntityNotFoundException`, `PersistenceException` |
| **Null handling** | `reader.IsDBNull(...) ? null : reader.GetX(...)` |
| **DBNull.Value** | Para parámetros opcionales nulos |
| **SCOPE_IDENTITY()** | Para obtener ID tras INSERT |