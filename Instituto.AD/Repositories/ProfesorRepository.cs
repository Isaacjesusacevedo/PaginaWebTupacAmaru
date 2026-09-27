using Instituto.AD.Interfaces;
using Instituto.AD.Models;
using Microsoft.Data.SqlClient;

namespace Instituto.AD.Repositories;

public class ProfesorRepository : IProfesorRepository
{
    private readonly string _connectionString;

    public ProfesorRepository(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    private static Profesor MapReaderToProfesor(SqlDataReader reader)
    {
        return new Profesor
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
            Apellido = reader.GetString(reader.GetOrdinal("Apellido")),
            Email = reader.GetString(reader.GetOrdinal("Email")),
            Telefono = reader.IsDBNull(reader.GetOrdinal("Telefono")) ? null : reader.GetString(reader.GetOrdinal("Telefono")),
            Especialidad = reader.IsDBNull(reader.GetOrdinal("Especialidad")) ? null : reader.GetString(reader.GetOrdinal("Especialidad"))
        };
    }

    public List<Profesor> GetAll()
    {
        var profesores = new List<Profesor>();
        using var db = new AccesoDB(_connectionString);
        using var reader = db.GetData("SELECT * FROM Profesores ORDER BY Id");
        while (reader.Read())
        {
            profesores.Add(MapReaderToProfesor(reader));
        }
        return profesores;
    }

    public Profesor? GetById(int id)
    {
        using var db = new AccesoDB(_connectionString);
        using var reader = db.GetData(
            "SELECT * FROM Profesores WHERE Id = @Id",
            new DBParameters().Agregar("@Id", id));
        
        return reader.Read() ? MapReaderToProfesor(reader) : null;
    }

    public Profesor Create(Profesor entity)
    {
        using var db = new AccesoDB(_connectionString);
        var parametros = new DBParameters()
            .Agregar("@Nombre", entity.Nombre)
            .Agregar("@Apellido", entity.Apellido)
            .Agregar("@Email", entity.Email.ToLower().Trim())
            .Agregar("@Telefono", entity.Telefono ?? (object)DBNull.Value)
            .Agregar("@Especialidad", entity.Especialidad ?? (object)DBNull.Value);

        var id = Convert.ToInt32(db.ExecuteScalar(
            @"INSERT INTO Profesores (Nombre, Apellido, Email, Telefono, Especialidad, FechaCreacion)
              VALUES (@Nombre, @Apellido, @Email, @Telefono, @Especialidad, GETDATE());
              SELECT SCOPE_IDENTITY();",
            parametros));

        entity.Id = id;
        return entity;
    }

    public void Update(int id, Profesor entity)
    {
        using var db = new AccesoDB(_connectionString);
        var parametros = new DBParameters()
            .Agregar("@Id", id)
            .Agregar("@Nombre", entity.Nombre)
            .Agregar("@Apellido", entity.Apellido)
            .Agregar("@Email", entity.Email.ToLower().Trim())
            .Agregar("@Telefono", entity.Telefono ?? (object)DBNull.Value)
            .Agregar("@Especialidad", entity.Especialidad ?? (object)DBNull.Value);

        db.Execute(
            @"UPDATE Profesores SET Nombre = @Nombre, Apellido = @Apellido, Email = @Email, Telefono = @Telefono, Especialidad = @Especialidad
              WHERE Id = @Id",
            parametros);
    }

    public void Delete(int id)
    {
        using var db = new AccesoDB(_connectionString);
        db.Execute("DELETE FROM Profesores WHERE Id = @Id", new DBParameters().Agregar("@Id", id));
    }

    public bool Exists(int id)
    {
        using var db = new AccesoDB(_connectionString);
        var result = db.ExecuteScalar("SELECT COUNT(*) FROM Profesores WHERE Id = @Id", new DBParameters().Agregar("@Id", id));
        return Convert.ToInt32(result) > 0;
    }

    public bool ExistsByEmail(string email)
    {
        using var db = new AccesoDB(_connectionString);
        var result = db.ExecuteScalar("SELECT COUNT(*) FROM Profesores WHERE Email = @Email", new DBParameters().Agregar("@Email", email.ToLower().Trim()));
        return Convert.ToInt32(result) > 0;
    }
}