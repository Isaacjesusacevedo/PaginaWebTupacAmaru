using Instituto.AD.Interfaces;
using Instituto.AD.Models;
using Microsoft.Data.SqlClient;

namespace Instituto.AD.Repositories;

public class AdministradorRepository : IAdministradorRepository
{
    private readonly string _connectionString;

    public AdministradorRepository(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
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

    public List<Administrador> GetAll()
    {
        var admins = new List<Administrador>();
        using var db = new AccesoDB(_connectionString);
        using var reader = db.GetData("SELECT * FROM Administradores WHERE Activo = 1 ORDER BY Id");
        while (reader.Read())
        {
            admins.Add(MapReaderToAdministrador(reader));
        }
        return admins;
    }

    public Administrador? GetById(int id)
    {
        using var db = new AccesoDB(_connectionString);
        using var reader = db.GetData(
            "SELECT * FROM Administradores WHERE Id = @Id AND Activo = 1",
            new DBParameters().Agregar("@Id", id));
        
        return reader.Read() ? MapReaderToAdministrador(reader) : null;
    }

    public Administrador? GetByEmail(string email)
    {
        using var db = new AccesoDB(_connectionString);
        using var reader = db.GetData(
            "SELECT * FROM Administradores WHERE Email = @Email AND Activo = 1",
            new DBParameters().Agregar("@Email", email.ToLower().Trim()));
        
        return reader.Read() ? MapReaderToAdministrador(reader) : null;
    }

    public Administrador Create(Administrador entity)
    {
        using var db = new AccesoDB(_connectionString);
        var parametros = new DBParameters()
            .Agregar("@Nombre", entity.Nombre)
            .Agregar("@Apellido", entity.Apellido)
            .Agregar("@Email", entity.Email.ToLower().Trim())
            .Agregar("@PasswordHash", entity.PasswordHash)
            .Agregar("@Role", entity.Role)
            .Agregar("@Activo", true)
            .Agregar("@FechaCreacion", DateTime.Now);

        var id = Convert.ToInt32(db.ExecuteScalar(
            @"INSERT INTO Administradores (Nombre, Apellido, Email, PasswordHash, Role, Activo, FechaCreacion)
              VALUES (@Nombre, @Apellido, @Email, @PasswordHash, @Role, @Activo, @FechaCreacion);
              SELECT SCOPE_IDENTITY();",
            parametros));

        entity.Id = id;
        return entity;
    }

    public void Update(int id, Administrador entity)
    {
        using var db = new AccesoDB(_connectionString);
        var parametros = new DBParameters()
            .Agregar("@Id", id)
            .Agregar("@Nombre", entity.Nombre)
            .Agregar("@Apellido", entity.Apellido)
            .Agregar("@Email", entity.Email.ToLower().Trim())
            .Agregar("@Role", entity.Role);

        db.Execute(
            @"UPDATE Administradores SET Nombre = @Nombre, Apellido = @Apellido, Email = @Email, Role = @Role 
              WHERE Id = @Id AND Activo = 1",
            parametros);
    }

    public void Delete(int id)
    {
        using var db = new AccesoDB(_connectionString);
        db.Execute(
            "UPDATE Administradores SET Activo = 0 WHERE Id = @Id AND Activo = 1",
            new DBParameters().Agregar("@Id", id));
    }

    public bool Exists(int id)
    {
        using var db = new AccesoDB(_connectionString);
        var result = db.ExecuteScalar("SELECT COUNT(*) FROM Administradores WHERE Id = @Id AND Activo = 1", new DBParameters().Agregar("@Id", id));
        return Convert.ToInt32(result) > 0;
    }

    public bool ExistsByEmail(string email)
    {
        using var db = new AccesoDB(_connectionString);
        var result = db.ExecuteScalar("SELECT COUNT(*) FROM Administradores WHERE Email = @Email AND Activo = 1", new DBParameters().Agregar("@Email", email.ToLower().Trim()));
        return Convert.ToInt32(result) > 0;
    }

    public int Count()
    {
        using var db = new AccesoDB(_connectionString);
        var result = db.ExecuteScalar("SELECT COUNT(*) FROM Administradores");
        return Convert.ToInt32(result);
    }

    public void UpdatePasswordHash(int id, string newHash)
    {
        using var db = new AccesoDB(_connectionString);
        db.Execute(
            "UPDATE Administradores SET PasswordHash = @PasswordHash WHERE Id = @Id",
            new DBParameters().Agregar("@Id", id).Agregar("@PasswordHash", newHash));
    }
}