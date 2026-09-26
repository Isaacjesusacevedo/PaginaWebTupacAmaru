using Microsoft.Data.SqlClient;
using Backend.Exceptions;
using Backend.Models;

namespace Backend.Services;

public class ProfesorSqlServerService : SqlServerBaseService<Profesor>, ICrudJsonService<Profesor>
{
    public ProfesorSqlServerService(IConfiguration config) : base(config) { }

    protected override Profesor MapReaderToEntity(SqlDataReader reader)
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

    public List<Profesor> GetAll() =>
        ExecuteQuery(
            @"SELECT Id, Nombre, Apellido, Email, Telefono, Especialidad
              FROM Profesores
              ORDER BY Id");

    public Profesor GetById(int id) =>
        ExecuteQuerySingle("Profesor", id,
            @"SELECT Id, Nombre, Apellido, Email, Telefono, Especialidad
              FROM Profesores
              WHERE Id = @Id",
            cmd => cmd.Parameters.AddWithValue("@Id", id));

    public Profesor Create(Profesor entity)
    {
        try
        {
            var newId = ExecuteScalar(
                @"INSERT INTO Profesores (Nombre, Apellido, Email, Telefono, Especialidad)
                  VALUES (@Nombre, @Apellido, @Email, @Telefono, @Especialidad);
                  SELECT CAST(SCOPE_IDENTITY() AS INT);",
                cmd =>
                {
                    cmd.Parameters.AddWithValue("@Nombre", entity.Nombre);
                    cmd.Parameters.AddWithValue("@Apellido", entity.Apellido);
                    cmd.Parameters.AddWithValue("@Email", entity.Email);
                    cmd.Parameters.AddWithValue("@Telefono", (object?)entity.Telefono ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Especialidad", (object?)entity.Especialidad ?? DBNull.Value);
                });

            entity.Id = newId;
            return entity;
        }
        catch (Exception ex) when (ex is not PersistenceException)
        {
            throw new PersistenceException("Error al crear el profesor en SQL Server.", ex);
        }
    }

    public void Update(int id, Profesor entity)
    {
        try
        {
            var rows = ExecuteNonQuery(
                @"UPDATE Profesores
                  SET Nombre = @Nombre, Apellido = @Apellido, Email = @Email,
                      Telefono = @Telefono, Especialidad = @Especialidad
                  WHERE Id = @Id",
                cmd =>
                {
                    cmd.Parameters.AddWithValue("@Nombre", entity.Nombre);
                    cmd.Parameters.AddWithValue("@Apellido", entity.Apellido);
                    cmd.Parameters.AddWithValue("@Email", entity.Email);
                    cmd.Parameters.AddWithValue("@Telefono", (object?)entity.Telefono ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Especialidad", (object?)entity.Especialidad ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Id", id);
                });

            if (rows == 0) throw new EntityNotFoundException("Profesor", id);
        }
        catch (Exception ex) when (ex is not EntityNotFoundException and not PersistenceException)
        {
            throw new PersistenceException("Error al actualizar el profesor.", ex);
        }
    }

    public void Delete(int id)
    {
        try
        {
            var rows = ExecuteNonQuery(
                "DELETE FROM Profesores WHERE Id = @Id",
                cmd => cmd.Parameters.AddWithValue("@Id", id));

            if (rows == 0) throw new EntityNotFoundException("Profesor", id);
        }
        catch (Exception ex) when (ex is not EntityNotFoundException and not PersistenceException)
        {
            throw new PersistenceException("Error al eliminar el profesor.", ex);
        }
    }
}