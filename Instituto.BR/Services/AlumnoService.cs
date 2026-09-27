using Instituto.AD.Interfaces;
using Instituto.AD.Models;
using Instituto.BR.DTOs;
using Instituto.BR.Interfaces;

namespace Instituto.BR.Services;

public class AlumnoService : IAlumnoService
{
    private readonly IAlumnoRepository _repository;
    private readonly ICarreraRepository _carreraRepository;

    public AlumnoService(IAlumnoRepository repository, ICarreraRepository carreraRepository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _carreraRepository = carreraRepository ?? throw new ArgumentNullException(nameof(carreraRepository));
    }

    public List<Alumno> GetAll() => _repository.GetAll();

    public Alumno? GetById(int id) => _repository.GetById(id);

    public ServiceResult<Alumno> Create(Alumno alumno)
    {
        if (string.IsNullOrWhiteSpace(alumno.Nombre))
            return ServiceResult<Alumno>.Fail("El nombre es obligatorio.");

        if (string.IsNullOrWhiteSpace(alumno.Apellido))
            return ServiceResult<Alumno>.Fail("El apellido es obligatorio.");

        if (string.IsNullOrWhiteSpace(alumno.Email))
            return ServiceResult<Alumno>.Fail("El email es obligatorio.");

        if (alumno.DNI < 1000000 || alumno.DNI > 99999999)
            return ServiceResult<Alumno>.Fail("El DNI debe tener entre 7 y 8 dígitos.");

        if (alumno.FechaNacimiento == default)
            return ServiceResult<Alumno>.Fail("La fecha de nacimiento es obligatoria.");

        if (alumno.CarreraId <= 0)
            return ServiceResult<Alumno>.Fail("Debe seleccionar una carrera válida.");

        if (!_carreraRepository.Exists(alumno.CarreraId))
            return ServiceResult<Alumno>.Fail($"La carrera con Id {alumno.CarreraId} no existe.");

        if (_repository.ExistsByDNI(alumno.DNI))
            return ServiceResult<Alumno>.Fail("Ya existe un alumno con ese DNI.");

        if (_repository.ExistsByEmail(alumno.Email))
            return ServiceResult<Alumno>.Fail("Ya existe un alumno con ese email.");

        alumno.FechaInscripcion ??= DateTime.Now;
        var created = _repository.Create(alumno);
        return ServiceResult<Alumno>.Ok(created, "Alumno inscrito correctamente.");
    }

    public ServiceResult<Alumno> Update(int id, Alumno alumno)
    {
        if (!_repository.Exists(id))
            return ServiceResult<Alumno>.Fail($"El alumno con Id {id} no existe.");

        if (string.IsNullOrWhiteSpace(alumno.Nombre))
            return ServiceResult<Alumno>.Fail("El nombre es obligatorio.");

        if (string.IsNullOrWhiteSpace(alumno.Apellido))
            return ServiceResult<Alumno>.Fail("El apellido es obligatorio.");

        if (string.IsNullOrWhiteSpace(alumno.Email))
            return ServiceResult<Alumno>.Fail("El email es obligatorio.");

        if (alumno.DNI < 1000000 || alumno.DNI > 99999999)
            return ServiceResult<Alumno>.Fail("El DNI debe tener entre 7 y 8 dígitos.");

        if (alumno.CarreraId <= 0)
            return ServiceResult<Alumno>.Fail("Debe seleccionar una carrera válida.");

        if (!_carreraRepository.Exists(alumno.CarreraId))
            return ServiceResult<Alumno>.Fail($"La carrera con Id {alumno.CarreraId} no existe.");

        _repository.Update(id, alumno);
        return ServiceResult<Alumno>.Ok(alumno, "Alumno actualizado correctamente.");
    }

    public ServiceResult Delete(int id)
    {
        if (!_repository.Exists(id))
            return ServiceResult.Fail($"El alumno con Id {id} no existe.");

        _repository.Delete(id);
        return ServiceResult.Ok("Alumno eliminado correctamente.");
    }
}