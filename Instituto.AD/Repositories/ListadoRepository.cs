using Instituto.AD.Interfaces;
using Instituto.AD.Models;
using Microsoft.Data.SqlClient;

namespace Instituto.AD.Repositories;

public class ListadoRepository : IListadoRepository
{
    private readonly string _connectionString;

    public ListadoRepository(string connectionString)
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

    private static Carrera MapReaderToCarrera(SqlDataReader reader)
    {
        return new Carrera
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            Nombre = reader.GetString(reader.GetOrdinal("Nombre"))
        };
    }

    public List<ListadoItem> GetListado()
    {
        var alumnos = new List<Alumno>();
        var carreras = new Dictionary<int, string>();

        using (var db = new AccesoDB(_connectionString))
        {
            using var reader = db.GetData("SELECT * FROM Alumnos ORDER BY Id");
            while (reader.Read())
            {
                alumnos.Add(MapReaderToAlumno(reader));
            }
        }

        using (var db = new AccesoDB(_connectionString))
        {
            using var reader = db.GetData("SELECT Id, Nombre FROM Carreras");
            while (reader.Read())
            {
                carreras[reader.GetInt32(reader.GetOrdinal("Id"))] = reader.GetString(reader.GetOrdinal("Nombre"));
            }
        }

        return alumnos.Select(a =>
        {
            var carreraNombre = carreras.TryGetValue(a.CarreraId, out var c) ? c : "Sin carrera";
            return new ListadoItem
            {
                AlumnoId = a.Id,
                NombreCompleto = $"{a.Apellido}, {a.Nombre}",
                DNI = a.DNI,
                Email = a.Email,
                Carrera = carreraNombre,
                Turno = a.Turno,
                Edad = a.Edad
            };
        }).ToList();
    }
}