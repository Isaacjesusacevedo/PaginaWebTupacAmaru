using BCryptNet = BCrypt.Net.BCrypt;
using Instituto.AD.Interfaces;
using Instituto.AD.Models;
using Instituto.BR.DTOs;
using Instituto.BR.Interfaces;

namespace Instituto.BR.Services;

public class AdministradorService : IAdministradorService
{
    private readonly IAdministradorRepository _repository;

    public AdministradorService(IAdministradorRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public List<Administrador> GetAll() => _repository.GetAll();

    public Administrador? GetById(int id) => _repository.GetById(id);

    public ServiceResult<Administrador> Create(Administrador admin, string password)
    {
        if (string.IsNullOrWhiteSpace(admin.Nombre))
            return ServiceResult<Administrador>.Fail("El nombre es obligatorio.");

        if (string.IsNullOrWhiteSpace(admin.Apellido))
            return ServiceResult<Administrador>.Fail("El apellido es obligatorio.");

        if (string.IsNullOrWhiteSpace(admin.Email))
            return ServiceResult<Administrador>.Fail("El email es obligatorio.");

        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            return ServiceResult<Administrador>.Fail("La contraseña debe tener al menos 8 caracteres.");

        if (_repository.ExistsByEmail(admin.Email))
            return ServiceResult<Administrador>.Fail("Ya existe un administrador con ese email.");

        admin.PasswordHash = BCryptNet.HashPassword(password, workFactor: 12);
        admin.Email = admin.Email.ToLower().Trim();
        admin.Activo = true;
        admin.FechaCreacion = DateTime.Now;

        var created = _repository.Create(admin);
        return ServiceResult<Administrador>.Ok(created, "Administrador creado correctamente.");
    }

    public ServiceResult<Administrador> Update(int id, Administrador admin)
    {
        if (!_repository.Exists(id))
            return ServiceResult<Administrador>.Fail($"El administrador con Id {id} no existe.");

        if (string.IsNullOrWhiteSpace(admin.Nombre))
            return ServiceResult<Administrador>.Fail("El nombre es obligatorio.");

        if (string.IsNullOrWhiteSpace(admin.Apellido))
            return ServiceResult<Administrador>.Fail("El apellido es obligatorio.");

        if (string.IsNullOrWhiteSpace(admin.Email))
            return ServiceResult<Administrador>.Fail("El email es obligatorio.");

        var existing = _repository.GetById(id);
        if (existing == null)
            return ServiceResult<Administrador>.Fail($"El administrador con Id {id} no existe.");

        if (!existing.Email.Equals(admin.Email, StringComparison.OrdinalIgnoreCase))
        {
            if (_repository.ExistsByEmail(admin.Email))
                return ServiceResult<Administrador>.Fail("Ya existe un administrador con ese email.");
        }

        admin.Email = admin.Email.ToLower().Trim();
        _repository.Update(id, admin);
        
        var updated = _repository.GetById(id);
        return ServiceResult<Administrador>.Ok(updated!, "Administrador actualizado correctamente.");
    }

    public ServiceResult ChangePassword(int id, string passwordActual, string nuevaPassword)
    {
        if (!_repository.Exists(id))
            return ServiceResult.Fail($"El administrador con Id {id} no existe.");

        if (string.IsNullOrWhiteSpace(passwordActual))
            return ServiceResult.Fail("La contraseña actual es obligatoria.");

        if (string.IsNullOrWhiteSpace(nuevaPassword) || nuevaPassword.Length < 8)
            return ServiceResult.Fail("La nueva contraseña debe tener al menos 8 caracteres.");

        var admin = _repository.GetById(id);
        if (admin == null)
            return ServiceResult.Fail($"El administrador con Id {id} no existe.");

        if (!BCryptNet.Verify(passwordActual, admin.PasswordHash))
            return ServiceResult.Fail("La contraseña actual es incorrecta.");

        var newHash = BCryptNet.HashPassword(nuevaPassword, workFactor: 12);
        _repository.UpdatePasswordHash(id, newHash);
        
        return ServiceResult.Ok("Contraseña actualizada correctamente.");
    }

    public ServiceResult Delete(int id)
    {
        if (!_repository.Exists(id))
            return ServiceResult.Fail($"El administrador con Id {id} no existe.");

        _repository.Delete(id);
        return ServiceResult.Ok("Administrador eliminado correctamente.");
    }

    public AdminResult? Login(string email, string password)
    {
        var admin = _repository.GetByEmail(email);
        if (admin == null)
            return null;

        if (!BCryptNet.Verify(password, admin.PasswordHash))
            return null;

        return new AdminResult(admin.Id, admin.Nombre, admin.Apellido, admin.Email, admin.Role);
    }

    public bool HayAdmins() => _repository.Count() > 0;

    public async Task<AdminResult?> CrearPrimerAdminAsync(SetupAdminDto dto)
    {
        if (HayAdmins())
            return null;

        var admin = new Administrador
        {
            Nombre = dto.Nombre,
            Apellido = dto.Apellido,
            Email = dto.Email,
            Role = dto.Role
        };

        var result = Create(admin, dto.Password);
        if (!result.Success)
            return null;

        return new AdminResult(result.Data!.Id, result.Data.Nombre, result.Data.Apellido, result.Data.Email, result.Data.Role);
    }

    public ServiceResult ChangePasswordByEmail(string email, string passwordActual, string nuevaPassword)
    {
        var admin = _repository.GetByEmail(email);
        if (admin == null)
            return ServiceResult.Fail("Usuario no encontrado.");

        if (!BCryptNet.Verify(passwordActual, admin.PasswordHash))
            return ServiceResult.Fail("Contraseña actual incorrecta.");

        if (string.IsNullOrWhiteSpace(nuevaPassword) || nuevaPassword.Length < 8)
            return ServiceResult.Fail("La nueva contraseña debe tener al menos 8 caracteres.");

        var newHash = BCryptNet.HashPassword(nuevaPassword, workFactor: 12);
        _repository.UpdatePasswordHash(admin.Id, newHash);
        
        return ServiceResult.Ok("Contraseña actualizada correctamente.");
    }
}