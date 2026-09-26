using Microsoft.Data.SqlClient;
using Backend.Exceptions;
using Backend.Models;

namespace Backend.Services;

public class CarreraSqlServerService : SqlServerBaseService<Carrera>, ICrudJsonService<Carrera>
{
    public CarreraSqlServerService(IConfiguration config) : base(config) { }

    protected override Carrera MapReaderToEntity(SqlDataReader reader)
    {
        return new Carrera
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
            DuracionAnios = reader.GetInt32(reader.GetOrdinal("DuracionAnios")),
            Turno = reader.IsDBNull(reader.GetOrdinal("Turno")) ? null : reader.GetString(reader.GetOrdinal("Turno")),
            Modalidad = reader.IsDBNull(reader.GetOrdinal("Modalidad")) ? null : reader.GetString(reader.GetOrdinal("Modalidad")),
            Horario = reader.IsDBNull(reader.GetOrdinal("Horario")) ? null : reader.GetString(reader.GetOrdinal("Horario")),
            Estado = reader.IsDBNull(reader.GetOrdinal("Estado")) ? null : reader.GetString(reader.GetOrdinal("Estado"))
        };
    }

    public List<Carrera> GetAll() =>
        ExecuteQuery(
            @"SELECT Id, Nombre, DuracionAnios, Turno, Modalidad, Horario, Estado
              FROM Carreras
              ORDER BY Id");

    public Carrera GetById(int id) =>
        ExecuteQuerySingle("Carrera", id,
            @"SELECT Id, Nombre, DuracionAnios, Turno, Modalidad, Horario, Estado
              FROM Carreras
              WHERE Id = @Id",
            cmd => cmd.Parameters.AddWithValue("@Id", id));

    public Carrera Create(Carrera entity)
    {
        try
        {
            var newId = ExecuteScalar(
                @"INSERT INTO Carreras (Nombre, DuracionAnios, Turno, Modalidad, Horario, Estado)
                  VALUES (@Nombre, @DuracionAnios, @Turno, @Modalidad, @Horario, @Estado);
                  SELECT CAST(SCOPE_IDENTITY() AS INT);",
                cmd =>
                {
                    cmd.Parameters.AddWithValue("@Nombre", entity.Nombre);
                    cmd.Parameters.AddWithValue("@DuracionAnios", entity.DuracionAnios);
                    cmd.Parameters.AddWithValue("@Turno", (object?)entity.Turno ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Modalidad", (object?)entity.Modalidad ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Horario", (object?)entity.Horario ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Estado", (object?)entity.Estado ?? "Activa");
                });

            entity.Id = newId;
            return entity;
        }
        catch (Exception ex) when (ex is not PersistenceException)
        {
            throw new PersistenceException("Error al crear la carrera en SQL Server.", ex);
        }
    }

    public void Update(int id, Carrera entity)
    {
        try
        {
            var rows = ExecuteNonQuery(
                @"UPDATE Carreras
                  SET Nombre = @Nombre, DuracionAnios = @DuracionAnios, Turno = @Turno,
                      Modalidad = @Modalidad, Horario = @Horario, Estado = @Estado
                  WHERE Id = @Id",
                cmd =>
                {
                    cmd.Parameters.AddWithValue("@Nombre", entity.Nombre);
                    cmd.Parameters.AddWithValue("@DuracionAnios", entity.DuracionAnios);
                    cmd.Parameters.AddWithValue("@Turno", (object?)entity.Turno ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Modalidad", (object?)entity.Modalidad ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Horario", (object?)entity.Horario ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Estado", (object?)entity.Estado ?? "Activa");
                    cmd.Parameters.AddWithValue("@Id", id);
                });

            if (rows == 0) throw new EntityNotFoundException("Carrera", id);
        }
        catch (Exception ex) when (ex is not EntityNotFoundException and not PersistenceException)
        {
            throw new PersistenceException("Error al actualizar la carrera.", ex);
        }
    }

    public void Delete(int id)
    {
        try
        {
            var rows = ExecuteNonQuery(
                "DELETE FROM Carreras WHERE Id = @Id",
                cmd => cmd.Parameters.AddWithValue("@Id", id));

            if (rows == 0) throw new EntityNotFoundException("Carrera", id);
        }
        catch (Exception ex) when (ex is not EntityNotFoundException and not PersistenceException)
        {
            throw new PersistenceException("Error al eliminar la carrera.", ex);
        }
    }
}