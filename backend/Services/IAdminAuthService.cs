using Backend.Models;

namespace Backend.Services;

public interface IAdminAuthService
{
    /// <summary>
    /// Valida las credenciales contra la tabla de Supabase.
    /// Retorna el AdminResult si son correctas, null si son incorrectas.
    /// </summary>
    Task<AdminResult?> LoginAsync(string email, string password);

    /// <summary>Retorna true si ya existe al menos un administrador en la base de datos.</summary>
    Task<bool> HayAdminsAsync();

    /// <summary>Crea un nuevo administrador con la contraseña hasheada con BCrypt.</summary>
    Task CrearAdminAsync(string nombre, string apellido, string email, string password, string role);

    /// <summary>Cambia la contraseña de un administrador verificando la actual.</summary>
    Task ChangePasswordAsync(string email, string passwordActual, string nuevaPassword);
}
