using Instituto.AD.Interfaces;
using Instituto.AD.Models;
using Instituto.BR.DTOs;
using Instituto.BR.Interfaces;
using Microsoft.Data.SqlClient;

namespace Instituto.BR.Services;

public class InfAcademicaService : IInfAcademicaService
{
    private readonly IInfAcademicaRepository _repository;
    private readonly IInfAcademicaEstRepository _estRepository;

    public InfAcademicaService(IInfAcademicaRepository repository, IInfAcademicaEstRepository estRepository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _estRepository = estRepository ?? throw new ArgumentNullException(nameof(estRepository));
    }

    public async Task<List<InfAcademica>> GetAllAsync()
        => await _repository.GetAllAsync();

    public async Task<InfAcademica?> GetByIdAsync(int id)
        => await _repository.GetByIdAsync(id);

    public async Task<ServiceResult<InfAcademica>> CreateAsync(InfAcademica entity)
    {
        var error = ValidarCatalogo(entity);
        if (error is not null)
            return ServiceResult<InfAcademica>.Fail(error);

        try
        {
            entity.Id = await _repository.CreateAsync(entity);
            return ServiceResult<InfAcademica>.Ok(entity, "Tipo de información académica creado correctamente.");
        }
        catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
        {
            return ServiceResult<InfAcademica>.Fail("Ya existe un tipo de información académica con esa descripción.");
        }
    }

    public async Task<ServiceResult<InfAcademica>> UpdateAsync(int id, InfAcademica entity)
    {
        if (await _repository.GetByIdAsync(id) is null)
            return ServiceResult<InfAcademica>.Fail($"El tipo de información académica con Id {id} no existe.");

        var error = ValidarCatalogo(entity);
        if (error is not null)
            return ServiceResult<InfAcademica>.Fail(error);

        entity.Id = id;

        try
        {
            await _repository.UpdateAsync(entity);
            return ServiceResult<InfAcademica>.Ok(entity, "Tipo de información académica actualizado correctamente.");
        }
        catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
        {
            return ServiceResult<InfAcademica>.Fail("Ya existe un tipo de información académica con esa descripción.");
        }
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        if (await _repository.GetByIdAsync(id) is null)
            return ServiceResult.Fail($"El tipo de información académica con Id {id} no existe.");

        if (await _estRepository.ExistsByInfAcademicaAsync(id))
            return ServiceResult.Fail("No se puede eliminar un tipo que ya está asignado a un alumno.");

        await _repository.DeleteAsync(id);
        return ServiceResult.Ok("Tipo de información académica eliminado correctamente.");
    }

    private static string? ValidarCatalogo(InfAcademica entity)
    {
        if (string.IsNullOrWhiteSpace(entity.Descripcion))
            return "La descripción es obligatoria.";

        if (entity.Descripcion.Length > 100)
            return "La descripción no puede superar los 100 caracteres.";

        if (entity.Fecha == default)
            return "La fecha es obligatoria.";

        if (entity.Estado != InfAcademicaConstants.CatalogoHabilitado && entity.Estado != InfAcademicaConstants.CatalogoDeshabilitado)
            return "El estado debe ser HABILITADO o DESHABILITADO.";

        return null;
    }
}
