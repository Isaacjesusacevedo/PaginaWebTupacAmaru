using Microsoft.Data.SqlClient;
using Backend.Models;

namespace Backend.Services;

public class AdminAuthService : IAdminAuthService
{
    private readonly string _connectionString;

    public AdminAuthService(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("SqlServer")
            ?? throw new InvalidOperationException(
                "La cadena de conexión 'SqlServer' no está configurada en appsettings.json.");
    }

    public async Task<AdminResult?> LoginAsync(string email, string password)
    {
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new SqlCommand(
            @"SELECT Id, Nombre, Apellido, Email, PasswordHash, Role
              FROM Administradores
              WHERE Email = @Email AND Activo = 1",
            conn);
        cmd.Parameters.AddWithValue("@Email", email.Trim().ToLower());

        await using var reader = await cmd.ExecuteReaderAsync();
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

    public async Task<bool> HayAdminsAsync()
    {
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new SqlCommand(
            "SELECT COUNT(*) FROM Administradores", conn);

        var count = Convert.ToInt64(await cmd.ExecuteScalarAsync());
        return count > 0;
    }

    public async Task CrearAdminAsync(
        string nombre, string apellido, string email, string password, string role)
    {
        var hash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new SqlCommand(
            @"INSERT INTO Administradores (Nombre, Apellido, Email, PasswordHash, Role, Activo)
              VALUES (@Nombre, @Apellido, @Email, @PasswordHash, @Role, 1)",
            conn);

        cmd.Parameters.AddWithValue("@Nombre", nombre.Trim());
        cmd.Parameters.AddWithValue("@Apellido", apellido.Trim());
        cmd.Parameters.AddWithValue("@Email", email.Trim().ToLower());
        cmd.Parameters.AddWithValue("@PasswordHash", hash);
        cmd.Parameters.AddWithValue("@Role", role);

        await cmd.ExecuteNonQueryAsync();
    }
}
