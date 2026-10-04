using Instituto.AD.Interfaces;
using Instituto.AD.Models;
using Instituto.BR.DTOs;
using Instituto.BR.Interfaces;
using Microsoft.Data.SqlClient;

namespace Instituto.BR.Services;

public class InfAcademicaEstService : IInfAcademicaEstService
{
    private readonly IInfAcademicaEstRepository _repository;
    private readonly IAlumnoRepository _alumnoRepository;

    public InfAcademicaEstService(
        IInfAcademicaEstRepository repository,
        IAlumnoRepository alumnoRepository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _alumnoRepository = alumnoRepository ?? throw new ArgumentNullException(nameof(alumnoRepository));
    }

    public async Task<List<InfAcademicaEst>> GetAllAsync()
        => await _repository.GetAllAsync();

    public async Task<InfAcademicaEst?> GetByIdAsync(int id)
        => await _repository.GetByIdAsync(id);

    public async Task<InfAcademicaEst?> GetByAlumnoIdAsync(int alumnoId)
        => await _repository.GetByAlumnoIdAsync(alumnoId);

    public async Task<ServiceResult<InfAcademicaEst>> CreateAsync(InfAcademicaEst entity)
    {
        if (!await _alumnoRepository.ExistsAsync(entity.AlumnoId))
            return ServiceResult<InfAcademicaEst>.Fail($"El alumno con Id {entity.AlumnoId} no existe.");

        var error = Validar(entity);
        if (error is not null)
            return ServiceResult<InfAcademicaEst>.Fail(error);

        if (await _repository.ExistsByAlumnoIdAsync(entity.AlumnoId))
            return ServiceResult<InfAcademicaEst>.Fail("El alumno ya tiene un registro de información académica.");

        try
        {
            entity.Id = await _repository.CreateAsync(entity);
            return ServiceResult<InfAcademicaEst>.Ok(entity, "Información académica creada correctamente.");
        }
        catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
        {
            return ServiceResult<InfAcademicaEst>.Fail("El alumno ya tiene un registro de información académica.");
        }
    }

    public async Task<ServiceResult<InfAcademicaEst>> UpdateAsync(int id, InfAcademicaEst entity)
    {
        var existente = await _repository.GetByIdAsync(id);
        if (existente is null)
            return ServiceResult<InfAcademicaEst>.Fail($"La información académica con Id {id} no existe.");

        entity.Id = id;
        entity.AlumnoId = existente.AlumnoId;

        var error = Validar(entity);
        if (error is not null)
            return ServiceResult<InfAcademicaEst>.Fail(error);

        await _repository.UpdateAsync(entity);
        return ServiceResult<InfAcademicaEst>.Ok(entity, "Información académica actualizada correctamente.");
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var existente = await _repository.GetByIdAsync(id);
        if (existente is null)
            return ServiceResult.Fail($"La información académica con Id {id} no existe.");

        await _repository.DeleteAsync(id);
        return ServiceResult.Ok("Información académica eliminada correctamente.");
    }

    private static string? Validar(InfAcademicaEst entity)
    {
        // Si posee título o está en trámite, se exige el título secundario
        if ((entity.PoseeTitulo || entity.TituloEnTramite) && string.IsNullOrWhiteSpace(entity.TituloSecundario))
            return "Si posee título (o está en trámite), debe informar el título secundario.";

        if (entity.TituloSecundario?.Length > 100)
            return "El título secundario no puede superar los 100 caracteres.";

        // Título y Título en trámite son mutuamente excluyentes
        if (entity.PoseeTitulo && entity.TituloEnTramite)
            return "No puede poseer el título y tenerlo en trámite al mismo tiempo.";

        // Si posee título, la fecha de egreso es obligatoria
        if (entity.PoseeTitulo && entity.FechaEgreso is null)
            return "Si posee el título, debe informar la fecha de egreso.";

        // Debe marcar al menos una opción
        if (!entity.PoseeTitulo && !entity.TituloEnTramite && !entity.ConsMaterias && !entity.ConsAlumnoRegular)
            return "Debe informar al menos un dato académico.";

        return null;
    }
}