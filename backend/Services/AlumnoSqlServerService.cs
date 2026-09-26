using Microsoft.Data.SqlClient;
using Backend.Exceptions;
using Backend.Models;

namespace Backend.Services;

public class AlumnoSqlServerService : SqlServerBaseService<Alumno>, ICrudJsonService<Alumno>
{
    public AlumnoSqlServerService(IConfiguration config) : base(config) { }

    protected override Alumno MapReaderToEntity(SqlDataReader reader)
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

    public List<Alumno> GetAll() =>
        ExecuteQuery(
            @"SELECT Id, Nombre, Apellido, Email, DNI, FechaNacimiento, Direccion, Nacionalidad,
                     FechaInscripcion, Telefono, TituloSecundario, Turno, CarreraId
              FROM Alumnos
              ORDER BY Id");

    public Alumno GetById(int id) =>
        ExecuteQuerySingle("Alumno", id,
            @"SELECT Id, Nombre, Apellido, Email, DNI, FechaNacimiento, Direccion, Nacionalidad,
                     FechaInscripcion, Telefono, TituloSecundario, Turno, CarreraId
              FROM Alumnos
              WHERE Id = @Id",
            cmd => cmd.Parameters.AddWithValue("@Id", id));

    public Alumno Create(Alumno entity)
    {
        try
        {
            var newId = ExecuteScalar(
                @"INSERT INTO Alumnos
                    (Nombre, Apellido, Email, DNI, FechaNacimiento, Direccion,
                     Nacionalidad, FechaInscripcion, Telefono, TituloSecundario, Turno, CarreraId)
                  VALUES
                    (@Nombre, @Apellido, @Email, @DNI, @FechaNacimiento, @Direccion,
                     @Nacionalidad, @FechaInscripcion, @Telefono, @TituloSecundario, @Turno, @CarreraId);
                  SELECT CAST(SCOPE_IDENTITY() AS INT);",
                cmd =>
                {
                    cmd.Parameters.AddWithValue("@Nombre", entity.Nombre);
                    cmd.Parameters.AddWithValue("@Apellido", entity.Apellido);
                    cmd.Parameters.AddWithValue("@Email", entity.Email);
                    cmd.Parameters.AddWithValue("@DNI", entity.DNI);
                    cmd.Parameters.AddWithValue("@FechaNacimiento", entity.FechaNacimiento);
                    cmd.Parameters.AddWithValue("@Direccion", (object?)entity.Direccion ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Nacionalidad", (object?)entity.Nacionalidad ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@FechaInscripcion", (object?)entity.FechaInscripcion ?? (object)DateTime.Now);
                    cmd.Parameters.AddWithValue("@Telefono", (object?)entity.Telefono ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@TituloSecundario", (object?)entity.TituloSecundario ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Turno", (object?)entity.Turno ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@CarreraId", entity.CarreraId);
                });

            entity.Id = newId;
            return entity;
        }
        catch (Exception ex) when (ex is not PersistenceException)
        {
            throw new PersistenceException("Error al crear el alumno en SQL Server.", ex);
        }
    }

    public void Update(int id, Alumno entity)
    {
        try
        {
            var rows = ExecuteNonQuery(
                @"UPDATE Alumnos
                  SET Nombre = @Nombre, Apellido = @Apellido, Email = @Email, DNI = @DNI,
                      FechaNacimiento = @FechaNacimiento, Direccion = @Direccion, Nacionalidad = @Nacionalidad,
                      FechaInscripcion = @FechaInscripcion, Telefono = @Telefono, TituloSecundario = @TituloSecundario,
                      Turno = @Turno, CarreraId = @CarreraId
                  WHERE Id = @Id",
                cmd =>
                {
                    cmd.Parameters.AddWithValue("@Nombre", entity.Nombre);
                    cmd.Parameters.AddWithValue("@Apellido", entity.Apellido);
                    cmd.Parameters.AddWithValue("@Email", entity.Email);
                    cmd.Parameters.AddWithValue("@DNI", entity.DNI);
                    cmd.Parameters.AddWithValue("@FechaNacimiento", entity.FechaNacimiento);
                    cmd.Parameters.AddWithValue("@Direccion", (object?)entity.Direccion ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Nacionalidad", (object?)entity.Nacionalidad ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@FechaInscripcion", (object?)entity.FechaInscripcion ?? (object)DateTime.Now);
                    cmd.Parameters.AddWithValue("@Telefono", (object?)entity.Telefono ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@TituloSecundario", (object?)entity.TituloSecundario ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Turno", (object?)entity.Turno ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@CarreraId", entity.CarreraId);
                    cmd.Parameters.AddWithValue("@Id", id);
                });

            if (rows == 0) throw new EntityNotFoundException("Alumno", id);
        }
        catch (Exception ex) when (ex is not EntityNotFoundException and not PersistenceException)
        {
            throw new PersistenceException("Error al actualizar el alumno.", ex);
        }
    }

    public void Delete(int id)
    {
        try
        {
            var rows = ExecuteNonQuery(
                "DELETE FROM Alumnos WHERE Id = @Id",
                cmd => cmd.Parameters.AddWithValue("@Id", id));

            if (rows == 0) throw new EntityNotFoundException("Alumno", id);
        }
        catch (Exception ex) when (ex is not EntityNotFoundException and not PersistenceException)
        {
            throw new PersistenceException("Error al eliminar el alumno.", ex);
        }
    }
}