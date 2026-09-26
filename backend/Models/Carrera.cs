using System.ComponentModel.DataAnnotations;

namespace Backend.Models;

public class Carrera
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre de la carrera es obligatorio.")]
    [StringLength(200, ErrorMessage = "El nombre no puede superar los 200 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Range(1, 10, ErrorMessage = "La duración debe estar entre 1 y 10 años.")]
    public int DuracionAnios { get; set; }

    public string? Turno { get; set; }

    public string? Modalidad { get; set; }

    public string? Horario { get; set; }

    public string? Estado { get; set; }
}
