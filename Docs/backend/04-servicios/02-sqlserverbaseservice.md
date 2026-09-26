# SqlServerBaseService - Documentación Detallada

## Propósito

Clase base abstracta que encapsula **todo el boilerplate de ADO.NET** para SQL Server, exponiendo métodos template seguros y reutilizables.

**Ubicación**: `Services/SqlServerBaseService.cs`

---

## Firma de Clase

```csharp
public abstract class SqlServerBaseService<T> where T : class
{
    protected readonly string _connectionString;
    protected readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    protected SqlServerBaseService(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("SqlServer")
            ?? throw new InvalidOperationException("SqlServer connection string no configurada.");
    }
    // ...
}
```

- **Genérico**: `T` = entidad de dominio (`Administrador`, `Alumno`, etc.)
- **Constructor**: Inyecta `IConfiguration`, lee connection string `"SqlServer"`
- **Fail-fast**: Lanza excepción si connection string no existe

---

## Métodos Públicos/Protegidos

### 1. OpenConnection()

```csharp
protected SqlConnection OpenConnection()
{
    var conn = new SqlConnection(_connectionString);
    conn.Open();
    return conn;
}
```
- Crea y abre conexión
- **Caller responsable** de `using` / `Dispose()`

---

### 2. ExecuteQuery()

```csharp
protected List<T> ExecuteQuery(string sql, Action<SqlCommand>? addParams = null)
```
**Uso**: `SELECT` que retorna múltiples filas.

**Parámetros**:
- `sql`: Query SQL con parámetros `@Nombre`
- `addParams`: Action para agregar parámetros al comando

**Flujo interno**:
```
1. OpenConnection()
2. Create SqlCommand(sql, conn)
3. addParams?.Invoke(cmd)  // Agrega @params
4. ExecuteReader()
5. While reader.Read(): MapReaderToEntity(reader) → List<T>
6. Return list
```

**Excepciones**: Wrappea todo en `PersistenceException` (excepto si ya es `PersistenceException`)

---

### 3. ExecuteQuerySingle()

```csharp
protected T ExecuteQuerySingle(string entityName, int id, string sql, Action<SqlCommand>? addParams = null)
```
**Uso**: `SELECT` que debe retornar **exactamente una fila** (GET by ID).

**Comportamiento**:
- Llama `ExecuteQuery()`
- Si `list.Count == 0` → lanza `EntityNotFoundException(entityName, id)`
- Retorna `list[0]`

---

### 4. ExecuteScalar()

```csharp
protected int ExecuteScalar(string sql, Action<SqlCommand> addParams)
```
**Uso**: `INSERT` con `RETURNING` / `SCOPE_IDENTITY()`, `COUNT(*)`, etc.

**Retorna**: `int` (0 si null)

---

### 5. ExecuteNonQuery()

```csharp
protected int ExecuteNonQuery(string sql, Action<SqlCommand> addParams)
```
**Uso**: `UPDATE`, `DELETE`, `INSERT` sin retorno.

**Retorna**: `int` = filas afectadas (para validar existencia)

---

## Método Abstracto Requerido

```csharp
protected abstract T MapReaderToEntity(SqlDataReader reader);
```

**Implementación típica**:
```csharp
protected override Alumno MapReaderToEntity(SqlDataReader reader)
{
    return new Alumno
    {
        Id = reader.GetInt32(reader.GetOrdinal("Id")),
        Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
        // ... campos requeridos
        Direccion = reader.IsDBNull(reader.GetOrdinal("Direccion")) 
            ? null 
            : reader.GetString(reader.GetOrdinal("Direccion")),
        // ... campos opcionales
    };
}
```

**Reglas**:
- Usar `reader.GetOrdinal("ColumnName")` → seguro contra orden de columnas
- `IsDBNull()` **antes** de `GetX()` para nullable
- Mapear **todas** las propiedades de la entidad

---

## Manejo de Excepciones

```csharp
catch (Exception ex) when (ex is not PersistenceException)
{
    throw new PersistenceException(
        $"[{typeof(T).Name}] SQL error: {ex.GetType().Name} — {ex.Message}", ex);
}
```

- **No traga** excepciones originales (inner exception preservado)
- **Contexto**: Incluye nombre de entidad `[Alumno]`, `[Carrera]`, etc.
- **No envuelve** `PersistenceException` ni `EntityNotFoundException` (burbujean)

---

## Ejemplo de Uso en Servicio Concreto

```csharp
public class AlumnoSqlServerService : SqlServerBaseService<Alumno>, ICrudJsonService<Alumno>
{
    public AlumnoSqlServerService(IConfiguration config) : base(config) { }

    protected override Alumno MapReaderToEntity(SqlDataReader reader) { ... }

    public List<Alumno> GetAll() =>
        ExecuteQuery(@"SELECT Id, Nombre, Apellido, Email, DNI, FechaNacimiento, 
                              Direccion, Nacionalidad, FechaInscripcion, Telefono, 
                              TituloSecundario, Turno, CarreraId
                       FROM Alumnos ORDER BY Id");

    public Alumno GetById(int id) =>
        ExecuteQuerySingle("Alumno", id,
            @"SELECT Id, Nombre, Apellido, Email, DNI, FechaNacimiento, 
                     Direccion, Nacionalidad, FechaInscripcion, Telefono, 
                     TituloSecundario, Turno, CarreraId
              FROM Alumnos WHERE Id = @Id",
            cmd => cmd.Parameters.AddWithValue("@Id", id));

    public Alumno Create(Alumno entity)
    {
        try
        {
            var newId = ExecuteScalar(
                @"INSERT INTO Alumnos (Nombre, Apellido, Email, DNI, FechaNacimiento, 
                                       Direccion, Nacionalidad, FechaInscripcion, Telefono, 
                                       TituloSecundario, Turno, CarreraId)
                  VALUES (@Nombre, @Apellido, @Email, @DNI, @FechaNacimiento, 
                          @Direccion, @Nacionalidad, @FechaInscripcion, @Telefono, 
                          @TituloSecundario, @Turno, @CarreraId);
                  SELECT CAST(SCOPE_IDENTITY() AS INT);",
                cmd =>
                {
                    cmd.Parameters.AddWithValue("@Nombre", entity.Nombre);
                    cmd.Parameters.AddWithValue("@Apellido", entity.Apellido);
                    // ... resto
                    cmd.Parameters.AddWithValue("@Direccion", (object?)entity.Direccion ?? DBNull.Value);
                    // ...
                });

            entity.Id = newId;
            return entity;
        }
        catch (Exception ex) when (ex is not PersistenceException)
        {
            throw new PersistenceException("Error al crear el alumno en SQL Server.", ex);
        }
    }
    // Update, Delete similares...
}
```

---

## Ventajas de Este Diseño

| Ventaja | Descripción |
|---------|-------------|
| **DRY** | Conexión, comando, reader, exception handling en un solo lugar |
| **Type Safety** | `MapReaderToEntity` forzado por compile-time |
| **Parameterized Queries** | Prevención SQL injection obligatoria |
| **Testability** | Fácil mock de `ICrudJsonService<T>` |
| **Consistencia** | Todos los servicios siguen mismo patrón |
| **Performance** | `GetOrdinal` cacheado implícitamente por reader |

---

## Extensibilidad Futura

Si se necesita:
- **Transacciones**: Agregar `ExecuteInTransaction(Action<SqlTransaction>)`
- **Paginación**: Agregar `ExecutePagedQuery(page, pageSize, ...)`
- **Bulk Insert**: Agregar `BulkInsert(IEnumerable<T>)`
- **Dapper**: Reemplazar implementación interna sin cambiar interfaz