using System.ComponentModel.DataAnnotations;

namespace Backend.Models;

public record VerifyPasswordDto
{
    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    public string Password { get; init; } = string.Empty;
}