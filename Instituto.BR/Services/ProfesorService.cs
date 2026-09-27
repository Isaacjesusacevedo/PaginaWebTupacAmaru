using Instituto.AD.Interfaces;
using Instituto.AD.Models;
using Instituto.BR.DTOs;
using Instituto.BR.Interfaces;

namespace Instituto.BR.Services;

public class ProfesorService : IProfesorService
{
    private readonly IProfesorRepository _repository;

    public ProfesorService(IProfesorRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public List<Profesor> GetAll() => _repository.GetAll();

    public Profesor? GetById(int id) => _repository.GetById(id);

    public ServiceResult<Profesor> Create(Profesor profesor)
    {
        if (string.IsNullOrWhiteSpace(profesor.Nombre))
            return ServiceResult<Profesor>.Fail("El nombre es obligatorio.");

        if (string.IsNullOrWhiteSpace(profesor.Apellido))
            return ServiceResult<Profesor>.Fail("El apellido es obligatorio.");

        if (string.IsNullOrWhiteSpace(profesor.Email))
            return ServiceResult<Profesor>.Fail("El email es obligatorio.");

        if (_repository.ExistsByEmail(profesor.Email))
            return ServiceResult<Profesor>.Fail("Ya existe un profesor con ese email.");

        var created = _repository.Create(profesor);
        return ServiceResult<Profesor>.Ok(created, "Profesor creado correctamente.");
    }

    public ServiceResult<Profesor> Update(int id, Profesor profesor)
    {
        if (!_repository.Exists(id))
            return ServiceResult<Profesor>.Fail($"El profesor con Id {id} no existe.");

        if (string.IsNullOrWhiteSpace(profesor.Nombre))
            return ServiceResult<Profesor>.Fail("El nombre es obligatorio.");

        if (string.IsNullOrWhiteSpace(profesor.Apellido))
            return ServiceResult<Profesor>.Fail("El apellido es obligatorio.");

        if (string.IsNullOrWhiteSpace(profesor.Email))
            return ServiceResult<Profesor>.Fail("El email es obligatorio.");

        var existing = _repository.GetById(id);
        if (existing == null)
            return ServiceResult<Profesor>.Fail($"El profesor con Id {id} no existe.");

        if (!existing.Email.Equals(profesor.Email, StringComparison.OrdinalIgnoreCase))
        {
            if (_repository.ExistsByEmail(profesor.Email))
                return ServiceResult<Profesor>.Fail("Ya existe un profesor con ese email.");
        }

        _repository.Update(id, profesor);
        return ServiceResult<Profesor>.Ok(profesor, "Profesor actualizado correctamente.");
    }

    public ServiceResult Delete(int id)
    {
        if (!_repository.Exists(id))
            return ServiceResult.Fail($"El profesor con Id {id} no existe.");

        _repository.Delete(id);
        return ServiceResult.Ok("Profesor eliminado correctamente.");
    }
}