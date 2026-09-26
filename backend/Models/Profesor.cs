using System.ComponentModel.DataAnnotations;

namespace Backend.Models;

public class Profesor : Persona
{
    [Phone(ErrorMessage = "El teléfono no tiene un formato válido.")]
    public string? Telefono { get; set; }

    [StringLength(100, ErrorMessage = "La especialidad no puede superar los 100 caracteres.")]
    public string? Especialidad { get; set; }
}
