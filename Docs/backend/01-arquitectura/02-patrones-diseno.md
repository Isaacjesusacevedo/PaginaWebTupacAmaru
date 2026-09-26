# Patrones de Diseño Utilizados

## 1. Repository Pattern (Variación)

**Ubicación**: `Services/ICrudJsonService.cs` + implementaciones `*SqlServerService.cs`

```csharp
public interface ICrudJsonService<T> where T : class
{
    List<T> GetAll();
    T GetById(int id);
    T Create(T entity);
    void Update(int id, T entity);
    void Delete(int id);
}
```

- Abstrae el acceso a datos detrás de una interfaz genérica
- Permite testing con mocks y cambio de implementación (ej. JSON files → SQL Server)
- Cada entidad tiene su implementación concreta

## 2. Template Method Pattern

**Ubicación**: `Services/SqlServerBaseService.cs`

```csharp
public abstract class SqlServerBaseService<T> where T : class
{
    // Métodos template (concretos en base)
    protected List<T> ExecuteQuery(string sql, Action<SqlCommand>? addParams = null) { ... }
    protected T ExecuteQuerySingle(string entityName, int id, string sql, ...) { ... }
    protected int ExecuteScalar(string sql, Action<SqlCommand> addParams) { ... }
    protected int ExecuteNonQuery(string sql, Action<SqlCommand> addParams) { ... }

    // Método abstracto (debe implementar subclase)
    protected abstract T MapReaderToEntity(SqlDataReader reader);
}
```

- Define el esqueleto de operaciones de BD en la clase base
- Subclases solo implementan el mapeo `reader → entidad`
- Elimina duplicación de código de conexión/comando/lectura

## 3. Dependency Injection (Constructor Injection)

**Ubicación**: `Program.cs` + Controllers + Services

```csharp
// Registro
builder.Services.AddScoped<ICrudJsonService<Alumno>, AlumnoSqlServerService>();

// Uso en Controller
public AlumnosController(ICrudJsonService<Alumno> alumnoService) { ... }

// Uso en Service
public AdminAuthService(IConfiguration config) { ... }
```

- Inversión de control completa
- Ciclo de vida `Scoped` (por request HTTP)
- Facilita testing y desacoplamiento

## 4. Strategy Pattern (Auth)

**Ubicación**: `Services/IAdminAuthService.cs` + `AdminAuthService.cs`

```csharp
public interface IAdminAuthService
{
    Task<AdminResult?> LoginAsync(string email, string password);
    Task<bool> HayAdminsAsync();
    Task CrearAdminAsync(...);
}
```

- Permite cambiar implementación de auth sin tocar controllers
- Útil para testing (mock) o migración a IdentityServer/OAuth futuro

## 5. DTO Pattern (Data Transfer Objects)

**Ubicación**: `Models/` + `DTOs/`

| Tipo | Uso |
|------|-----|
| `LoginDto` | Input de login (validado con DataAnnotations) |
| `SetupAdminDto` | Input de setup inicial |
| `AdminResult` | Output de login (sin password hash) |
| `AlumnoListadoDto` | Vista aplanada para listados (join Alumno+Carrera) |

- Separa modelo de dominio de contrato de API
- Evita over-posting y expone solo datos necesarios

## 6. Exception Filter Pattern (Middleware)

**Ubicación**: `Program.cs` (lines 83-93)

```csharp
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(
            new { error = "Ocurrió un error interno del servidor." }));
    });
});
```

- Captura excepciones no manejadas globalmente
- Respuesta JSON consistente (no HTML)
- Logs internos vs mensajes seguros al cliente

## 7. Options Pattern (Configuration)

**Ubicación**: `Program.cs` + `appsettings.json`

```csharp
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw ...;
builder.Services.AddAuthentication(...)
    .AddJwtBearer(options => { ... });
```

- Configuración tipada vía `IConfiguration`
- Secrets fuera del código (appsettings.Development.json, env vars)
- Validación temprana (`?? throw`)

## 8. Soft Delete Pattern

**Ubicación**: `AdministradorSqlServerService.cs` (Delete method)

```csharp
// Soft delete: marcamos activo = FALSE en lugar de borrar físicamente
var rows = ExecuteNonQuery(
    "UPDATE Administradores SET Activo = 0 WHERE Id = @Id AND Activo = 1", ...);
```

- Preserva integridad referencial e historial
- Filtro `WHERE Activo = 1` en todas las consultas
- Fácil recuperación y auditoría