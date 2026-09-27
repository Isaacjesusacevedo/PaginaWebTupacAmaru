using Instituto.AD.Interfaces;
using Instituto.AD.Models;
using Instituto.BR.DTOs;
using Instituto.BR.Interfaces;

namespace Instituto.BR.Services;

public class CarreraService : ICarreraService
{
    private readonly ICarreraRepository _repository;
    private readonly IAlumnoRepository _alumnoRepository;

    public CarreraService(ICarreraRepository repository, IAlumnoRepository alumnoRepository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _alumnoRepository = alumnoRepository ?? throw new ArgumentNullException(nameof(alumnoRepository));
    }

    public List<Carrera> GetAll() => _repository.GetAll();

    public Carrera? GetById(int id) => _repository.GetById(id);

    public ServiceResult<Carrera> Create(Carrera carrera)
    {
        if (string.IsNullOrWhiteSpace(carrera.Nombre))
            return ServiceResult<Carrera>.Fail("El nombre de la carrera es obligatorio.");

        if (carrera.DuracionAnios < 1 || carrera.DuracionAnios > 10)
            return ServiceResult<Carrera>.Fail("La duración debe estar entre 1 y 10 años.");

        carrera.Estado ??= "Activa";
        var created = _repository.Create(carrera);
        return ServiceResult<Carrera>.Ok(created, "Carrera creada correctamente.");
    }

    public ServiceResult<Carrera> Update(int id, Carrera carrera)
    {
        if (!_repository.Exists(id))
            return ServiceResult<Carrera>.Fail($"La carrera con Id {id} no existe.");

        if (string.IsNullOrWhiteSpace(carrera.Nombre))
            return ServiceResult<Carrera>.Fail("El nombre de la carrera es obligatorio.");

        if (carrera.DuracionAnios < 1 || carrera.DuracionAnios > 10)
            return ServiceResult<Carrera>.Fail("La duración debe estar entre 1 y 10 años.");

        _repository.Update(id, carrera);
        return ServiceResult<Carrera>.Ok(carrera, "Carrera actualizada correctamente.");
    }

    public ServiceResult Delete(int id)
    {
        if (!_repository.Exists(id))
            return ServiceResult.Fail($"La carrera con Id {id} no existe.");

        // Validación: no eliminar si hay alumnos inscriptos
        if (_alumnoRepository.ExistsByCarreraId(id))
            return ServiceResult.Fail("No se puede eliminar la carrera porque tiene alumnos inscriptos.");

        _repository.Delete(id);
        return ServiceResult.Ok("Carrera eliminada correctamente.");
    }
}