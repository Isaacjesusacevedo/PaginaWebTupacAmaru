using System.ComponentModel.DataAnnotations;

namespace Backend.Models;

public class Alumno : Persona
{
    [Required(ErrorMessage = "El DNI es obligatorio.")]
    [Range(1000000, 99999999, ErrorMessage = "El DNI debe tener entre 7 y 8 dígitos.")]
    public int DNI { get; set; }

    [Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
    public DateTime FechaNacimiento { get; set; }

    [StringLength(200, ErrorMessage = "La dirección no puede superar los 200 caracteres.")]
    public string? Direccion { get; set; }

    [StringLength(100, ErrorMessage = "La nacionalidad no puede superar los 100 caracteres.")]
    public string? Nacionalidad { get; set; }

    public DateTime? FechaInscripcion { get; set; }

    [Phone(ErrorMessage = "El teléfono no tiene un formato válido.")]
    public string? Telefono { get; set; }

    public string? TituloSecundario { get; set; }

    public string? Turno { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una carrera válida.")]
    public int CarreraId { get; set; }

    public int Edad => (int)((DateTime.Now - FechaNacimiento).TotalDays / 365.25);
}
