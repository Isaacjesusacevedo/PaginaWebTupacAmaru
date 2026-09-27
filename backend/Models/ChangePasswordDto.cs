using System.ComponentModel.DataAnnotations;

namespace Backend.Models;

public record ChangePasswordDto
{
    [Required(ErrorMessage = "La contraseña actual es obligatoria.")]
    public string PasswordActual { get; init; } = string.Empty;

    [Required(ErrorMessage = "La nueva contraseña es obligatoria.")]
    [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
    public string NuevaPassword { get; init; } = string.Empty;
}