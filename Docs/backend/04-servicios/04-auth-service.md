# Servicio de Autenticación (AdminAuthService)

## Visión General

**Archivo**: `Services/AdminAuthService.cs`  
**Interface**: `IAdminAuthService`  
**Responsabilidad**: Autenticación y gestión de administradores (login, registro inicial, verificación existencia).

---

## Interface

```csharp
public interface IAdminAuthService
{
    Task<AdminResult?> LoginAsync(string email, string password);
    Task<bool> HayAdminsAsync();
    Task CrearAdminAsync(string nombre, string apellido, string email, string password, string role);
    Task ChangePasswordAsync(string email, string passwordActual, string nuevaPassword);
}
```

> **Nota**: No hay método `VerifyPasswordAsync` separado. El endpoint `POST /api/auth/verify-password` reutiliza `LoginAsync` internamente en el controller.

---

## Métodos

### 1. LoginAsync(email, password)

**Propósito**: Validar credenciales y retornar datos del admin (sin password hash).

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
    if (!await reader.ReadAsync()) return null;  // No encontrado o inactivo

    var hash = reader.GetString(reader.GetOrdinal("PasswordHash"));
    if (!BCrypt.Net.BCrypt.Verify(password, hash)) return null;  // Password incorrecto

    return new AdminResult(
        reader.GetInt32(reader.GetOrdinal("Id")),
        reader.GetString(reader.GetOrdinal("Nombre")),
        reader.GetString(reader.GetOrdinal("Apellido")),
        reader.GetString(reader.GetOrdinal("Email")),
        reader.GetString(reader.GetOrdinal("Role"))
    );
}
```

**Flujo**:
1. Buscar admin por email (case-insensitive, solo activos)
2. Si no existe → `null`
3. Verificar password con `BCrypt.Verify(plain, hash)`
4. Si inválido → `null`
5. Retornar `AdminResult` (record inmutable sin hash)

**Seguridad**:
- **Timing attack**: BCrypt tiene tiempo constante
- **Email normalization**: `.Trim().ToLower()` antes de query
- **Soft delete**: `AND Activo = 1` evita login de admins "borrados"

---

### 2. HayAdminsAsync()

**Propósito**: Verificar si existe al menos un admin (para setup inicial).

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

**Uso**: `SetupController.CrearPrimerAdmin()` llama a esto. Si `true` → 409 Conflict.

---

### 3. CrearAdminAsync(nombre, apellido, email, password, role)

**Propósito**: Crear nuevo administrador con password hasheado.

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

**Detalles**:
- **Work factor 12**: ~250ms en CPU moderno (2024), seguro contra GPU cracking
- **Activo = 1**: Por defecto activo
- **Email normalizado**: Lowercase + trim
- **Sin validación de unicidad en código**: Confía en constraint UNIQUE de BD (lanza SqlException → PersistenceException → 500)

---

### 4. ChangePasswordAsync(email, passwordActual, nuevaPassword)

**Propósito**: Cambiar contraseña de admin verificando la actual.  
Usado por `AdministradorController.ChangePassword()` → `PUT /api/administradores/{id}/password`.

```csharp
public async Task ChangePasswordAsync(string email, string passwordActual, string nuevaPassword)
{
    await using var conn = new SqlConnection(_connectionString);
    await conn.OpenAsync();

    // Verificar contraseña actual
    await using var cmd = new SqlCommand(
        @"SELECT PasswordHash FROM Administradores WHERE Email = @Email AND Activo = 1",
        conn);
    cmd.Parameters.AddWithValue("@Email", email.Trim().ToLower());

    await using var reader = await cmd.ExecuteReaderAsync();
    if (!await reader.ReadAsync())
        throw new UnauthorizedAccessException("Usuario no encontrado");

    var hash = reader.GetString(reader.GetOrdinal("PasswordHash"));
    if (!BCrypt.Net.BCrypt.Verify(passwordActual, hash))
        throw new UnauthorizedAccessException("Contraseña actual incorrecta");

    // Actualizar contraseña
    var newHash = BCrypt.Net.BCrypt.HashPassword(nuevaPassword, workFactor: 12);
    await reader.CloseAsync();

    await using var updateCmd = new SqlCommand(
        @"UPDATE Administradores SET PasswordHash = @PasswordHash WHERE Email = @Email",
        conn);
    updateCmd.Parameters.AddWithValue("@PasswordHash", newHash);
    updateCmd.Parameters.AddWithValue("@Email", email.Trim().ToLower());

    await updateCmd.ExecuteNonQueryAsync();
}
```

**Flujo**:
1. Buscar admin por email (solo activos)
2. Si no existe → `UnauthorizedAccessException("Usuario no encontrado")`
3. Verificar `passwordActual` con `BCrypt.Verify`
4. Si inválido → `UnauthorizedAccessException("Contraseña actual incorrecta")`
5. Hash nueva contraseña con workFactor 12
6. UPDATE PasswordHash en BD

**Excepciones**:
- `UnauthorizedAccessException` → 401 en controller
- `Exception` genérico → 500 en controller

---

## BCrypt Configuration

```csharp
// Work factor 12 = 2^12 = 4096 iteraciones
BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
BCrypt.Net.BCrypt.Verify(password, hash);
```

| Work Factor | Tiempo aprox (CPU 2024) | Seguridad |
|-------------|-------------------------|-----------|
| 10 | ~60ms | Mínimo aceptable |
| **12** | **~250ms** | **Recomendado 2024+** |
| 14 | ~1000ms | Alta seguridad, UX impactado |

**Versión**: `BCrypt.Net-Next 4.0.3` (paquete NuGet)

---

## AdminResult (Response DTO)

```csharp
public record AdminResult(
    int    Id,
    string Nombre,
    string Apellido,
    string Email,
    string Role
);
```

- **Inmutable** (record)
- **Sin PasswordHash** → Nunca expone credenciales
- Serializado directamente en JWT claims y response JSON

---

## Integración con AuthController

```csharp
// AuthController.Login()
var admin = await _authService.LoginAsync(dto.Email, dto.Password);

if (admin is null)
    return Unauthorized(new { error = "Email o contraseña incorrectos." });

var token = GenerarToken(admin);  // JWT con claims: sub, email, name, role, jti
```

**Claims en JWT**:
| Claim | Valor | Fuente |
|-------|-------|--------|
| `sub` | `admin.Id.ToString()` | `AdminResult.Id` |
| `email` | `admin.Email` | `AdminResult.Email` |
| `name` | `"{Nombre} {Apellido}"` | `AdminResult.Nombre + Apellido` |
| `role` | `admin.Role` | `AdminResult.Role` |
| `jti` | `Guid.NewGuid()` | Único por token |

---

## Setup Inicial (Development Only)

**Endpoint**: `POST /api/setup/admin`  
**Controller**: `SetupController.CrearPrimerAdmin()`

```csharp
if (!_env.IsDevelopment()) return NotFound();  // Solo Dev

if (await _authService.HayAdminsAsync())
    return Conflict(new { error = "Ya existe al menos un administrador..." });

await _authService.CrearAdminAsync(dto.Nombre, dto.Apellido, dto.Email, dto.Password, dto.Role);
```

- **Solo Development**: `_env.IsDevelopment()` gate
- **Idempotente**: Si ya hay admins → 409
- **Único uso**: Tras primer admin, endpoint inutilizado

---

## Seguridad Adicional Recomendada (Futuro)

| Mejora | Prioridad | Implementación |
|--------|-----------|----------------|
| Rate limiting login | Alta | `Microsoft.AspNetCore.RateLimiting` |
| Account lockout tras N fallos | Media | Tabla `LoginAttempts` + contador |
| Password history (no reusar últimas N) | Baja | Tabla `PasswordHistory` |
| 2FA (TOTP) | Media | `GoogleAuthenticator` lib |
| Refresh tokens | Alta | Tabla `RefreshTokens` + rotación |
| Auditoria login (IP, user-agent, timestamp) | Media | Tabla `LoginLogs` |

---

## Testing

```csharp
// Mock IAdminAuthService
var mockAuth = new Mock<IAdminAuthService>();
mockAuth.Setup(x => x.LoginAsync("admin@test.com", "password123"))
        .ReturnsAsync(new AdminResult(1, "Admin", "User", "admin@test.com", "Admin"));
mockAuth.Setup(x => x.HayAdminsAsync()).ReturnsAsync(false);
```