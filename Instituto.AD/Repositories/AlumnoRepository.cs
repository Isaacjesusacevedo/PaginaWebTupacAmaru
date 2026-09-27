using Instituto.AD.Interfaces;
using Instituto.AD.Models;
using Microsoft.Data.SqlClient;

namespace Instituto.AD.Repositories;

public class AlumnoRepository : IAlumnoRepository
{
    private readonly string _connectionString;

    public AlumnoRepository(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    private static Alumno MapReaderToAlumno(SqlDataReader reader)
    {
        return new Alumno
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
            Apellido = reader.GetString(reader.GetOrdinal("Apellido")),
            Email = reader.GetString(reader.GetOrdinal("Email")),
            DNI = reader.GetInt32(reader.GetOrdinal("DNI")),
            FechaNacimiento = reader.GetDateTime(reader.GetOrdinal("FechaNacimiento")),
            Direccion = reader.IsDBNull(reader.GetOrdinal("Direccion")) ? null : reader.GetString(reader.GetOrdinal("Direccion")),
            Nacionalidad = reader.IsDBNull(reader.GetOrdinal("Nacionalidad")) ? null : reader.GetString(reader.GetOrdinal("Nacionalidad")),
            FechaInscripcion = reader.IsDBNull(reader.GetOrdinal("FechaInscripcion")) ? null : reader.GetDateTime(reader.GetOrdinal("FechaInscripcion")),
            Telefono = reader.IsDBNull(reader.GetOrdinal("Telefono")) ? null : reader.GetString(reader.GetOrdinal("Telefono")),
            TituloSecundario = reader.IsDBNull(reader.GetOrdinal("TituloSecundario")) ? null : reader.GetString(reader.GetOrdinal("TituloSecundario")),
            Turno = reader.IsDBNull(reader.GetOrdinal("Turno")) ? null : reader.GetString(reader.GetOrdinal("Turno")),
            CarreraId = reader.GetInt32(reader.GetOrdinal("CarreraId"))
        };
    }

    public List<Alumno> GetAll()
    {
        var alumnos = new List<Alumno>();
        using var db = new AccesoDB(_connectionString);
        using var reader = db.GetData("SELECT * FROM Alumnos ORDER BY Id");
        while (reader.Read())
        {
            alumnos.Add(MapReaderToAlumno(reader));
        }
        return alumnos;
    }

    public Alumno? GetById(int id)
    {
        using var db = new AccesoDB(_connectionString);
        using var reader = db.GetData(
            "SELECT * FROM Alumnos WHERE Id = @Id",
            new DBParameters().Agregar("@Id", id));
        
        return reader.Read() ? MapReaderToAlumno(reader) : null;
    }

    public Alumno Create(Alumno entity)
    {
        using var db = new AccesoDB(_connectionString);
        var parametros = new DBParameters()
            .Agregar("@Nombre", entity.Nombre)
            .Agregar("@Apellido", entity.Apellido)
            .Agregar("@Email", entity.Email)
            .Agregar("@DNI", entity.DNI)
            .Agregar("@FechaNacimiento", entity.FechaNacimiento)
            .Agregar("@Direccion", entity.Direccion ?? (object)DBNull.Value)
            .Agregar("@Nacionalidad", entity.Nacionalidad ?? (object)DBNull.Value)
            .Agregar("@FechaInscripcion", entity.FechaInscripcion ?? (object)DateTime.Now)
            .Agregar("@Telefono", entity.Telefono ?? (object)DBNull.Value)
            .Agregar("@TituloSecundario", entity.TituloSecundario ?? (object)DBNull.Value)
            .Agregar("@Turno", entity.Turno ?? (object)DBNull.Value)
            .Agregar("@CarreraId", entity.CarreraId);

        var id = Convert.ToInt32(db.ExecuteScalar(
            @"INSERT INTO Alumnos (Nombre, Apellido, Email, DNI, FechaNacimiento, Direccion, Nacionalidad, 
              FechaInscripcion, Telefono, TituloSecundario, Turno, CarreraId, FechaCreacion)
              VALUES (@Nombre, @Apellido, @Email, @DNI, @FechaNacimiento, @Direccion, @Nacionalidad, 
                      @FechaInscripcion, @Telefono, @TituloSecundario, @Turno, @CarreraId, GETDATE());
              SELECT SCOPE_IDENTITY();",
            parametros));

        entity.Id = id;
        return entity;
    }

    public void Update(int id, Alumno entity)
    {
        using var db = new AccesoDB(_connectionString);
        var parametros = new DBParameters()
            .Agregar("@Id", id)
            .Agregar("@Nombre", entity.Nombre)
            .Agregar("@Apellido", entity.Apellido)
            .Agregar("@Email", entity.Email)
            .Agregar("@DNI", entity.DNI)
            .Agregar("@FechaNacimiento", entity.FechaNacimiento)
            .Agregar("@Direccion", entity.Direccion ?? (object)DBNull.Value)
            .Agregar("@Nacionalidad", entity.Nacionalidad ?? (object)DBNull.Value)
            .Agregar("@Telefono", entity.Telefono ?? (object)DBNull.Value)
            .Agregar("@TituloSecundario", entity.TituloSecundario ?? (object)DBNull.Value)
            .Agregar("@Turno", entity.Turno ?? (object)DBNull.Value)
            .Agregar("@CarreraId", entity.CarreraId);

        db.Execute(
            @"UPDATE Alumnos SET Nombre = @Nombre, Apellido = @Apellido, Email = @Email, DNI = @DNI, 
              FechaNacimiento = @FechaNacimiento, Direccion = @Direccion, Nacionalidad = @Nacionalidad,
              Telefono = @Telefono, TituloSecundario = @TituloSecundario, Turno = @Turno, CarreraId = @CarreraId
              WHERE Id = @Id",
            parametros);
    }

    public void Delete(int id)
    {
        using var db = new AccesoDB(_connectionString);
        db.Execute("DELETE FROM Alumnos WHERE Id = @Id", new DBParameters().Agregar("@Id", id));
    }

    public bool Exists(int id)
    {
        using var db = new AccesoDB(_connectionString);
        var result = db.ExecuteScalar("SELECT COUNT(*) FROM Alumnos WHERE Id = @Id", new DBParameters().Agregar("@Id", id));
        return Convert.ToInt32(result) > 0;
    }

    public bool ExistsByDNI(int dni)
    {
        using var db = new AccesoDB(_connectionString);
        var result = db.ExecuteScalar("SELECT COUNT(*) FROM Alumnos WHERE DNI = @DNI", new DBParameters().Agregar("@DNI", dni));
        return Convert.ToInt32(result) > 0;
    }

    public bool ExistsByEmail(string email)
    {
        using var db = new AccesoDB(_connectionString);
        var result = db.ExecuteScalar("SELECT COUNT(*) FROM Alumnos WHERE Email = @Email", new DBParameters().Agregar("@Email", email.ToLower().Trim()));
        return Convert.ToInt32(result) > 0;
    }

    public bool ExistsByCarreraId(int carreraId)
    {
        using var db = new AccesoDB(_connectionString);
        var result = db.ExecuteScalar("SELECT COUNT(*) FROM Alumnos WHERE CarreraId = @CarreraId", new DBParameters().Agregar("@CarreraId", carreraId));
        return Convert.ToInt32(result) > 0;
    }
}