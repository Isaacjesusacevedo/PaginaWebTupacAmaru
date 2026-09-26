using Microsoft.Data.SqlClient;
using Backend.Exceptions;
using Backend.Models;

namespace Backend.Services;

public class FormularioSqlServerService : SqlServerBaseService<Formulario>, ICrudJsonService<Formulario>
{
    public FormularioSqlServerService(IConfiguration config) : base(config) { }

    protected override Formulario MapReaderToEntity(SqlDataReader reader)
    {
        return new Formulario
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
            Estado = reader.GetString(reader.GetOrdinal("Estado")),
            FechaApertura = reader.GetDateTime(reader.GetOrdinal("FechaApertura")),
            FechaCierre = reader.GetDateTime(reader.GetOrdinal("FechaCierre")),
            Descripcion = reader.IsDBNull(reader.GetOrdinal("Descripcion")) ? null : reader.GetString(reader.GetOrdinal("Descripcion")),
            FechaCreacion = reader.GetDateTime(reader.GetOrdinal("FechaCreacion"))
        };
    }

    public List<Formulario> GetAll() =>
        ExecuteQuery(
            @"SELECT Id, Nombre, Estado, FechaApertura, FechaCierre, Descripcion, FechaCreacion
              FROM Formularios
              ORDER BY Id");

    public Formulario GetById(int id) =>
        ExecuteQuerySingle("Formulario", id,
            @"SELECT Id, Nombre, Estado, FechaApertura, FechaCierre, Descripcion, FechaCreacion
              FROM Formularios
              WHERE Id = @Id",
            cmd => cmd.Parameters.AddWithValue("@Id", id));

    public Formulario Create(Formulario entity)
    {
        try
        {
            var newId = ExecuteScalar(
                @"INSERT INTO Formularios (Nombre, Estado, FechaApertura, FechaCierre, Descripcion, FechaCreacion)
                  VALUES (@Nombre, @Estado, @FechaApertura, @FechaCierre, @Descripcion, @FechaCreacion);
                  SELECT CAST(SCOPE_IDENTITY() AS INT);",
                cmd =>
                {
                    cmd.Parameters.AddWithValue("@Nombre", entity.Nombre);
                    cmd.Parameters.AddWithValue("@Estado", entity.Estado);
                    cmd.Parameters.AddWithValue("@FechaApertura", entity.FechaApertura);
                    cmd.Parameters.AddWithValue("@FechaCierre", entity.FechaCierre);
                    cmd.Parameters.AddWithValue("@Descripcion", (object?)entity.Descripcion ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@FechaCreacion", entity.FechaCreacion);
                });

            entity.Id = newId;
            return entity;
        }
        catch (Exception ex) when (ex is not PersistenceException)
        {
            throw new PersistenceException("Error al crear el formulario en SQL Server.", ex);
        }
    }

    public void Update(int id, Formulario entity)
    {
        try
        {
            var rows = ExecuteNonQuery(
                @"UPDATE Formularios
                  SET Nombre = @Nombre, Estado = @Estado, FechaApertura = @FechaApertura,
                      FechaCierre = @FechaCierre, Descripcion = @Descripcion
                  WHERE Id = @Id",
                cmd =>
                {
                    cmd.Parameters.AddWithValue("@Nombre", entity.Nombre);
                    cmd.Parameters.AddWithValue("@Estado", entity.Estado);
                    cmd.Parameters.AddWithValue("@FechaApertura", entity.FechaApertura);
                    cmd.Parameters.AddWithValue("@FechaCierre", entity.FechaCierre);
                    cmd.Parameters.AddWithValue("@Descripcion", (object?)entity.Descripcion ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Id", id);
                });

            if (rows == 0) throw new EntityNotFoundException("Formulario", id);
        }
        catch (Exception ex) when (ex is not EntityNotFoundException and not PersistenceException)
        {
            throw new PersistenceException("Error al actualizar el formulario.", ex);
        }
    }

    public void Delete(int id)
    {
        try
        {
            var rows = ExecuteNonQuery(
                "DELETE FROM Formularios WHERE Id = @Id",
                cmd => cmd.Parameters.AddWithValue("@Id", id));

            if (rows == 0) throw new EntityNotFoundException("Formulario", id);
        }
        catch (Exception ex) when (ex is not EntityNotFoundException and not PersistenceException)
        {
            throw new PersistenceException("Error al eliminar el formulario.", ex);
        }
    }
}