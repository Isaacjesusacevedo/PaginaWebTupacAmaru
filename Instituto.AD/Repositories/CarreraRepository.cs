using Instituto.AD.Interfaces;
using Instituto.AD.Models;
using Microsoft.Data.SqlClient;

namespace Instituto.AD.Repositories;

public class CarreraRepository : ICarreraRepository
{
    private readonly string _connectionString;

    public CarreraRepository(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    private static Carrera MapReaderToCarrera(SqlDataReader reader)
    {
        return new Carrera
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
            DuracionAnios = reader.GetInt32(reader.GetOrdinal("DuracionAnios")),
            Turno = reader.IsDBNull(reader.GetOrdinal("Turno")) ? null : reader.GetString(reader.GetOrdinal("Turno")),
            Modalidad = reader.IsDBNull(reader.GetOrdinal("Modalidad")) ? null : reader.GetString(reader.GetOrdinal("Modalidad")),
            Horario = reader.IsDBNull(reader.GetOrdinal("Horario")) ? null : reader.GetString(reader.GetOrdinal("Horario")),
            Estado = reader.IsDBNull(reader.GetOrdinal("Estado")) ? null : reader.GetString(reader.GetOrdinal("Estado")),
            FechaCreacion = reader.GetDateTime(reader.GetOrdinal("FechaCreacion"))
        };
    }

    public List<Carrera> GetAll()
    {
        var carreras = new List<Carrera>();
        using var db = new AccesoDB(_connectionString);
        using var reader = db.GetData("SELECT Id, Nombre, DuracionAnios, Turno, Modalidad, Horario, Estado, FechaCreacion FROM Carreras ORDER BY Id");
        while (reader.Read())
        {
            carreras.Add(MapReaderToCarrera(reader));
        }
        return carreras;
    }

    public Carrera? GetById(int id)
    {
        using var db = new AccesoDB(_connectionString);
        using var reader = db.GetData(
            "SELECT Id, Nombre, DuracionAnios, Turno, Modalidad, Horario, Estado, FechaCreacion FROM Carreras WHERE Id = @Id",
            new DBParameters().Agregar("@Id", id));
        
        return reader.Read() ? MapReaderToCarrera(reader) : null;
    }

    public Carrera Create(Carrera entity)
    {
        using var db = new AccesoDB(_connectionString);
        var parametros = new DBParameters()
            .Agregar("@Nombre", entity.Nombre)
            .Agregar("@DuracionAnios", entity.DuracionAnios)
            .Agregar("@Turno", entity.Turno ?? (object)DBNull.Value)
            .Agregar("@Modalidad", entity.Modalidad ?? (object)DBNull.Value)
            .Agregar("@Horario", entity.Horario ?? (object)DBNull.Value)
            .Agregar("@Estado", entity.Estado ?? "Activa")
            .Agregar("@FechaCreacion", DateTime.Now);

        var id = Convert.ToInt32(db.ExecuteScalar(
            @"INSERT INTO Carreras (Nombre, DuracionAnios, Turno, Modalidad, Horario, Estado, FechaCreacion)
              VALUES (@Nombre, @DuracionAnios, @Turno, @Modalidad, @Horario, @Estado, @FechaCreacion);
              SELECT SCOPE_IDENTITY();",
            parametros));

        entity.Id = id;
        return entity;
    }

    public void Update(int id, Carrera entity)
    {
        using var db = new AccesoDB(_connectionString);
        var parametros = new DBParameters()
            .Agregar("@Id", id)
            .Agregar("@Nombre", entity.Nombre)
            .Agregar("@DuracionAnios", entity.DuracionAnios)
            .Agregar("@Turno", entity.Turno ?? (object)DBNull.Value)
            .Agregar("@Modalidad", entity.Modalidad ?? (object)DBNull.Value)
            .Agregar("@Horario", entity.Horario ?? (object)DBNull.Value)
            .Agregar("@Estado", entity.Estado ?? (object)DBNull.Value);

        db.Execute(
            @"UPDATE Carreras SET Nombre = @Nombre, DuracionAnios = @DuracionAnios, Turno = @Turno, 
              Modalidad = @Modalidad, Horario = @Horario, Estado = @Estado WHERE Id = @Id",
            parametros);
    }

    public void Delete(int id)
    {
        using var db = new AccesoDB(_connectionString);
        db.Execute("DELETE FROM Carreras WHERE Id = @Id", new DBParameters().Agregar("@Id", id));
    }

    public bool Exists(int id)
    {
        using var db = new AccesoDB(_connectionString);
        var result = db.ExecuteScalar("SELECT COUNT(*) FROM Carreras WHERE Id = @Id", new DBParameters().Agregar("@Id", id));
        return Convert.ToInt32(result) > 0;
    }
}