namespace Backend.Models;

/// <summary>Datos del administrador autenticado, sin exponer el hash de contraseña.</summary>
public record AdminResult(
    int    Id,
    string Nombre,
    string Apellido,
    string Email,
    string Role
);
