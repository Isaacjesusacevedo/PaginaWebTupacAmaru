namespace Backend.Models;

public class Alumno
{
    public int Id { get; set; }
    public string? Nombre { get; set; }
    public string? Apellido { get; set; }
    public int DNI { get; set; }
    public string? Email { get; set; }
    public DateTime FechaNacimiento { get; set; }
    public string? Direccion { get; set; }
    public string? Nacionalidad { get; set; }
    public DateTime? FechaInscripcion { get; set; }
    public string? Telefono { get; set; }
    public string? TituloSecundario { get; set; }
    public string? Turno { get; set; }
    public int CarreraId { get; set; }

    public int Edad => (int)((DateTime.Now - FechaNacimiento).TotalDays / 365.25);
}
