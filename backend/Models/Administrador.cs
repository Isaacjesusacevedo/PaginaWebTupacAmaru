using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models;

public class Administrador : Persona
{
    [Required(ErrorMessage = "El rol es obligatorio.")]
    [StringLength(50, ErrorMessage = "El rol no puede superar los 50 caracteres.")]
    public string Role { get; set; } = string.Empty;
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [MinLength(8, ErrorMessage = "La contraseña temporal debe tener al menos 8 caracteres.")]
    public string? PasswordTemp { get; set; }
}
