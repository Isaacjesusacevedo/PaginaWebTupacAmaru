using Instituto.AD.Interfaces;
using Instituto.AD.Models;
using Instituto.AD.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace Instituto.AD.Repositories;

public class AdministradorRepository : IAdministradorRepository
{
    private readonly InstitutoDbContext _context;

    public AdministradorRepository(InstitutoDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    private static Administrador MapReaderToAdministrador(SqlDataReader reader)
    {
        return new Administrador
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
            Apellido = reader.GetString(reader.GetOrdinal("Apellido")),
            Email = reader.GetString(reader.GetOrdinal("Email")),
            Role = reader.GetString(reader.GetOrdinal("Role")),
            PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
            Activo = reader.GetBoolean(reader.GetOrdinal("Activo")),
            FechaCreacion = reader.GetDateTime(reader.GetOrdinal("FechaCreacion"))
        };
    }

    // ✅ Sin cambios: ya usa ToListAsync
    public async Task<List<Administrador>> GetAllAsync()
    {
        return await _context.Administradores
            .FromSqlRaw("EXEC sp_Administradores_GetAll")
            .ToListAsync();
    }

    // ✅ CORREGIDO: ToListAsync() + FirstOrDefault() en memoria
    public async Task<Administrador?> GetByIdAsync(int id)
    {
        var result = await _context.Administradores
            .FromSqlRaw("EXEC sp_Administradores_GetById @Id", new SqlParameter("@Id", id))
            .ToListAsync();
        return result.FirstOrDefault();
    }

    // ✅ CORREGIDO: ToListAsync() + FirstOrDefault() en memoria
    public async Task<Administrador?> GetByEmailAsync(string email)
    {
        var result = await _context.Administradores
            .FromSqlRaw("EXEC sp_Administradores_GetByEmail @Email", new SqlParameter("@Email", email.ToLower().Trim()))
            .ToListAsync();
        return result.FirstOrDefault();
    }

    // ✅ Sin cambios: usa ExecuteScalarAsync directo
    public async Task<Administrador> CreateAsync(Administrador entity)
    {
        var connection = _context.Database.GetDbConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "EXEC sp_Administradores_Create @Nombre, @Apellido, @Email, @PasswordHash, @Role";
        command.Parameters.Add(new SqlParameter("@Nombre", entity.Nombre));
        command.Parameters.Add(new SqlParameter("@Apellido", entity.Apellido));
        command.Parameters.Add(new SqlParameter("@Email", entity.Email.ToLower().Trim()));
        command.Parameters.Add(new SqlParameter("@PasswordHash", entity.PasswordHash));
        command.Parameters.Add(new SqlParameter("@Role", entity.Role));

        var result = await command.ExecuteScalarAsync();
        entity.Id = Convert.ToInt32(result);
        return entity;
    }

    // ✅ Sin cambios: usa ExecuteNonQueryAsync directo
    public async Task UpdateAsync(int id, Administrador entity)
    {
        var connection = _context.Database.GetDbConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "EXEC sp_Administradores_Update @Id, @Nombre, @Apellido, @Email, @Role";
        command.Parameters.Add(new SqlParameter("@Id", id));
        command.Parameters.Add(new SqlParameter("@Nombre", entity.Nombre));
        command.Parameters.Add(new SqlParameter("@Apellido", entity.Apellido));
        command.Parameters.Add(new SqlParameter("@Email", entity.Email.ToLower().Trim()));
        command.Parameters.Add(new SqlParameter("@Role", entity.Role));

        await command.ExecuteNonQueryAsync();
    }

    // ✅ Sin cambios: usa ExecuteNonQueryAsync directo
    public async Task DeleteAsync(int id)
    {
        var connection = _context.Database.GetDbConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "EXEC sp_Administradores_SoftDelete @Id";
        command.Parameters.Add(new SqlParameter("@Id", id));
        await command.ExecuteNonQueryAsync();
    }

    // ✅ CORREGIDO: ToListAsync() + FirstOrDefault() en memoria
    public async Task<bool> ExistsAsync(int id)
    {
        var result = await _context.Database
            .SqlQueryRaw<int>("EXEC sp_Administradores_Exists @Id", new SqlParameter("@Id", id))
            .ToListAsync();
        return result.FirstOrDefault() == 1;
    }

    // ✅ CORREGIDO: ToListAsync() + FirstOrDefault() en memoria
    public async Task<bool> ExistsByEmailAsync(string email)
    {
        var result = await _context.Database
            .SqlQueryRaw<int>("EXEC sp_Administradores_ExistsByEmail @Email", new SqlParameter("@Email", email.ToLower().Trim()))
            .ToListAsync();
        return result.FirstOrDefault() == 1;
    }

    // ✅ CORREGIDO: ToListAsync() + FirstOrDefault() en memoria
    public async Task<int> CountAsync()
    {
        var result = await _context.Database
            .SqlQueryRaw<int>("EXEC sp_Administradores_Count")
            .ToListAsync();
        return result.FirstOrDefault();
    }

    // ✅ Sin cambios: usa ExecuteNonQueryAsync directo
    public async Task UpdatePasswordHashAsync(int id, string newHash)
    {
        var connection = _context.Database.GetDbConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "EXEC sp_Administradores_UpdatePasswordHash @Id, @PasswordHash";
        command.Parameters.Add(new SqlParameter("@Id", id));
        command.Parameters.Add(new SqlParameter("@PasswordHash", newHash));
        await command.ExecuteNonQueryAsync();
    }
}