using Instituto.AD.Interfaces;
using Instituto.AD.Models;
using Microsoft.Data.SqlClient;

namespace Instituto.AD.Repositories;

public class FormularioRepository : IFormularioRepository
{
    private readonly string _connectionString;

    public FormularioRepository(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    private static Formulario MapReaderToFormulario(SqlDataReader reader)
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

    public List<Formulario> GetAll()
    {
        var formularios = new List<Formulario>();
        using var db = new AccesoDB(_connectionString);
        using var reader = db.GetData("SELECT * FROM Formularios ORDER BY Id");
        while (reader.Read())
        {
            formularios.Add(MapReaderToFormulario(reader));
        }
        return formularios;
    }

    public Formulario? GetById(int id)
    {
        using var db = new AccesoDB(_connectionString);
        using var reader = db.GetData(
            "SELECT * FROM Formularios WHERE Id = @Id",
            new DBParameters().Agregar("@Id", id));
        
        return reader.Read() ? MapReaderToFormulario(reader) : null;
    }

    public Formulario Create(Formulario entity)
    {
        using var db = new AccesoDB(_connectionString);
        var parametros = new DBParameters()
            .Agregar("@Nombre", entity.Nombre)
            .Agregar("@Estado", entity.Estado)
            .Agregar("@FechaApertura", entity.FechaApertura)
            .Agregar("@FechaCierre", entity.FechaCierre)
            .Agregar("@Descripcion", entity.Descripcion ?? (object)DBNull.Value)
            .Agregar("@FechaCreacion", DateTime.Now);

        var id = Convert.ToInt32(db.ExecuteScalar(
            @"INSERT INTO Formularios (Nombre, Estado, FechaApertura, FechaCierre, Descripcion, FechaCreacion)
              VALUES (@Nombre, @Estado, @FechaApertura, @FechaCierre, @Descripcion, @FechaCreacion);
              SELECT SCOPE_IDENTITY();",
            parametros));

        entity.Id = id;
        return entity;
    }

    public void Update(int id, Formulario entity)
    {
        using var db = new AccesoDB(_connectionString);
        var parametros = new DBParameters()
            .Agregar("@Id", id)
            .Agregar("@Nombre", entity.Nombre)
            .Agregar("@Estado", entity.Estado)
            .Agregar("@FechaApertura", entity.FechaApertura)
            .Agregar("@FechaCierre", entity.FechaCierre)
            .Agregar("@Descripcion", entity.Descripcion ?? (object)DBNull.Value);

        db.Execute(
            @"UPDATE Formularios SET Nombre = @Nombre, Estado = @Estado, FechaApertura = @FechaApertura, 
              FechaCierre = @FechaCierre, Descripcion = @Descripcion WHERE Id = @Id",
            parametros);
    }

    public void Delete(int id)
    {
        using var db = new AccesoDB(_connectionString);
        db.Execute("DELETE FROM Formularios WHERE Id = @Id", new DBParameters().Agregar("@Id", id));
    }

    public bool Exists(int id)
    {
        using var db = new AccesoDB(_connectionString);
        var result = db.ExecuteScalar("SELECT COUNT(*) FROM Formularios WHERE Id = @Id", new DBParameters().Agregar("@Id", id));
        return Convert.ToInt32(result) > 0;
    }
}