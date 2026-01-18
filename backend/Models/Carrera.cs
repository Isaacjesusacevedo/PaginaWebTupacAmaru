namespace Backend.Models;

public class Carrera
{
    public int Id { get; set; }
    public string? Nombre { get; set; }
    public int DuracionAnios { get; set; }
    public string? Turno { get; set; }
    public string? Modalidad { get; set; }
    public string? Horario { get; set; }
    public string? Estado { get; set; }
}
