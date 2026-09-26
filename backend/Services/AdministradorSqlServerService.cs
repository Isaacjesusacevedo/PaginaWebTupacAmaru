using Microsoft.Data.SqlClient;
using Backend.Exceptions;
using Backend.Models;

namespace Backend.Services;

public class AdministradorSqlServerService : SqlServerBaseService<Administrador>, ICrudJsonService<Administrador>
{
    public AdministradorSqlServerService(IConfiguration config) : base(config) { }

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

    public List<Administrador> GetAll() =>
        ExecuteQuery(
            @"SELECT Id, Nombre, Apellido, Email, Role
              FROM Administradores
              WHERE Activo = 1
              ORDER BY Id");

    public Administrador GetById(int id) =>
        ExecuteQuerySingle("Administrador", id,
            @"SELECT Id, Nombre, Apellido, Email, Role
              FROM Administradores
              WHERE Id = @Id AND Activo = 1",
            cmd => cmd.Parameters.AddWithValue("@Id", id));

    public Administrador Create(Administrador entity)
    {
        var rawPassword = entity.PasswordTemp ?? "Cambiar1234!";
        var hash = BCrypt.Net.BCrypt.HashPassword(rawPassword, workFactor: 12);

        try
        {
            var newId = ExecuteScalar(
                @"INSERT INTO Administradores (Nombre, Apellido, Email, PasswordHash, Role, Activo)
                  VALUES (@Nombre, @Apellido, @Email, @PasswordHash, @Role, 1);
                  SELECT CAST(SCOPE_IDENTITY() AS INT);",
                cmd =>
                {
                    cmd.Parameters.AddWithValue("@Nombre", entity.Nombre);
                    cmd.Parameters.AddWithValue("@Apellido", entity.Apellido);
                    cmd.Parameters.AddWithValue("@Email", entity.Email.Trim().ToLower());
                    cmd.Parameters.AddWithValue("@PasswordHash", hash);
                    cmd.Parameters.AddWithValue("@Role", entity.Role);
                });

            entity.Id = newId;
            return entity;
        }
        catch (Exception ex) when (ex is not PersistenceException)
        {
            throw new PersistenceException("Error al crear el administrador en SQL Server.", ex);
        }
    }

    public void Update(int id, Administrador entity)
    {
        try
        {
            var rows = ExecuteNonQuery(
                @"UPDATE Administradores
                  SET Nombre = @Nombre, Apellido = @Apellido, Email = @Email, Role = @Role
                  WHERE Id = @Id AND Activo = 1",
                cmd =>
                {
                    cmd.Parameters.AddWithValue("@Nombre", entity.Nombre);
                    cmd.Parameters.AddWithValue("@Apellido", entity.Apellido);
                    cmd.Parameters.AddWithValue("@Email", entity.Email.Trim().ToLower());
                    cmd.Parameters.AddWithValue("@Role", entity.Role);
                    cmd.Parameters.AddWithValue("@Id", id);
                });

            if (rows == 0) throw new EntityNotFoundException("Administrador", id);
        }
        catch (Exception ex) when (ex is not EntityNotFoundException and not PersistenceException)
        {
            throw new PersistenceException("Error al actualizar el administrador.", ex);
        }
    }

    public void Delete(int id)
    {
        try
        {
            var rows = ExecuteNonQuery(
                "UPDATE Administradores SET Activo = 0 WHERE Id = @Id AND Activo = 1",
                cmd => cmd.Parameters.AddWithValue("@Id", id));

            if (rows == 0) throw new EntityNotFoundException("Administrador", id);
        }
        catch (Exception ex) when (ex is not EntityNotFoundException and not PersistenceException)
        {
            throw new PersistenceException("Error al eliminar el administrador.", ex);
        }
    }
}