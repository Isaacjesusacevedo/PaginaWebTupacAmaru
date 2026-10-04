using Instituto.AD.Interfaces;
using Instituto.AD.Models;
using Instituto.BR.DTOs;
using Instituto.BR.Interfaces;
using Microsoft.Data.SqlClient;

namespace Instituto.BR.Services;

public class InfAcademicaEstService : IInfAcademicaEstService
{
    private readonly IInfAcademicaEstRepository _repository;
    private readonly IInfAcademicaRepository _infAcademicaRepository;
    private readonly IAlumnoRepository _alumnoRepository;

    public InfAcademicaEstService(
        IInfAcademicaEstRepository repository,
        IInfAcademicaRepository infAcademicaRepository,
        IAlumnoRepository alumnoRepository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _infAcademicaRepository = infAcademicaRepository ?? throw new ArgumentNullException(nameof(infAcademicaRepository));
        _alumnoRepository = alumnoRepository ?? throw new ArgumentNullException(nameof(alumnoRepository));
    }

    public async Task<List<InfAcademicaEst>> GetAllAsync()
        => await _repository.GetAllAsync();

    public async Task<InfAcademicaEst?> GetByIdAsync(int id)
        => await _repository.GetByIdAsync(id);

    public async Task<List<InfAcademicaEst>> GetByAlumnoAsync(int alumnoId)
        => await _repository.GetByAlumnoAsync(alumnoId);

    public async Task<ServiceResult<InfAcademicaEst>> CreateAsync(InfAcademicaEst entity)
    {
        var tipo = await _infAcademicaRepository.GetByIdAsync(entity.InfAcademicaId);
        if (tipo is null)
            return ServiceResult<InfAcademicaEst>.Fail($"El tipo de información académica con Id {entity.InfAcademicaId} no existe.");

        if (tipo.Estado != InfAcademicaConstants.CatalogoHabilitado)
            return ServiceResult<InfAcademicaEst>.Fail($"El tipo '{tipo.Descripcion}' no está habilitado.");

        if (!await _alumnoRepository.ExistsAsync(entity.AlumnoId))
            return ServiceResult<InfAcademicaEst>.Fail($"El alumno con Id {entity.AlumnoId} no existe.");

        entity.EstadoTitulo = InfAcademicaValidator.ResolverEstadoPorDefecto(entity.EstadoTitulo, tipo.Descripcion);

        var error = ValidarCamposComunes(entity, tipo.Descripcion, out var tituloSecundario, out var institucion);
        if (error is not null)
            return ServiceResult<InfAcademicaEst>.Fail(error);

        entity.TituloSecundario = tituloSecundario;
        entity.Institucion = institucion;

        if (await _repository.ExistsByCombinationAsync(entity.InfAcademicaId, entity.AlumnoId))
            return ServiceResult<InfAcademicaEst>.Fail("El alumno ya tiene un registro de ese tipo de información académica.");

        try
        {
            entity.Id = await _repository.CreateAsync(entity);
            return ServiceResult<InfAcademicaEst>.Ok(entity, "Información académica creada correctamente.");
        }
        catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
        {
            return ServiceResult<InfAcademicaEst>.Fail("El alumno ya tiene un registro de ese tipo de información académica.");
        }
    }

    public async Task<ServiceResult<InfAcademicaEst>> UpdateAsync(int id, InfAcademicaEst entity)
    {
        var existente = await _repository.GetByIdAsync(id);
        if (existente is null)
            return ServiceResult<InfAcademicaEst>.Fail($"La información académica con Id {id} no existe.");

        var tipo = await _infAcademicaRepository.GetByIdAsync(entity.InfAcademicaId);
        if (tipo is null)
            return ServiceResult<InfAcademicaEst>.Fail($"El tipo de información académica con Id {entity.InfAcademicaId} no existe.");

        if (tipo.Estado != InfAcademicaConstants.CatalogoHabilitado)
            return ServiceResult<InfAcademicaEst>.Fail($"El tipo '{tipo.Descripcion}' no está habilitado.");

        entity.Id = id;
        entity.AlumnoId = existente.AlumnoId; // el alumno no cambia al actualizar

        entity.EstadoTitulo = InfAcademicaValidator.ResolverEstadoPorDefecto(entity.EstadoTitulo, tipo.Descripcion);

        var error = ValidarCamposComunes(entity, tipo.Descripcion, out var tituloSecundario, out var institucion);
        if (error is not null)
            return ServiceResult<InfAcademicaEst>.Fail(error);

        entity.TituloSecundario = tituloSecundario;
        entity.Institucion = institucion;

        if (await _repository.ExistsByCombinationAsync(entity.InfAcademicaId, entity.AlumnoId, id))
            return ServiceResult<InfAcademicaEst>.Fail("El alumno ya tiene un registro de ese tipo de información académica.");

        try
        {
            await _repository.UpdateAsync(entity);
            return ServiceResult<InfAcademicaEst>.Ok(entity, "Información académica actualizada correctamente.");
        }
        catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
        {
            return ServiceResult<InfAcademicaEst>.Fail("El alumno ya tiene un registro de ese tipo de información académica.");
        }
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        if (!await _repository.ExistsAsync(id))
            return ServiceResult.Fail($"La información académica con Id {id} no existe.");

        await _repository.DeleteAsync(id);
        return ServiceResult.Ok("Información académica eliminada correctamente.");
    }

    private static string? ValidarCamposComunes(InfAcademicaEst entity, string tipoDescripcion, out string? tituloSecundario, out string? institucion)
    {
        tituloSecundario = null;
        institucion = null;

        var errorEstado = InfAcademicaValidator.ValidarEstado(entity.EstadoTitulo);
        if (errorEstado is not null) return errorEstado;

        var errorFecha = InfAcademicaValidator.ValidarFechaEmision(entity.FechaEmision, entity.EstadoTitulo);
        if (errorFecha is not null) return errorFecha;

        var (errorCampos, ts, inst) = InfAcademicaValidator.ResolverCamposPorTipo(tipoDescripcion, entity.TituloSecundario, entity.Institucion);
        if (errorCampos is not null) return errorCampos;

        tituloSecundario = ts;
        institucion = inst;
        return null;
    }
}
