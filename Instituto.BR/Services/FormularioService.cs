using Instituto.AD.Interfaces;
using Instituto.AD.Models;
using Instituto.BR.DTOs;
using Instituto.BR.Interfaces;

namespace Instituto.BR.Services;

public class FormularioService : IFormularioService
{
    private readonly IFormularioRepository _repository;

    public FormularioService(IFormularioRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<List<Formulario>> GetAllAsync()
        => await _repository.GetAllAsync();

    public async Task<Formulario?> GetByIdAsync(int id)
        => await _repository.GetByIdAsync(id);

    public async Task<ServiceResult<Formulario>> CreateAsync(Formulario formulario)
    {
        if (string.IsNullOrWhiteSpace(formulario.Nombre))
            return ServiceResult<Formulario>.Fail("El nombre del formulario es obligatorio.");

        // ✅ Opción A: FechaApertura y FechaCierre son DateTime NO nullable,
        //    así que no se usa .HasValue. Se comparan directamente.
        if (formulario.FechaApertura == default)
            return ServiceResult<Formulario>.Fail("La fecha de apertura es obligatoria.");

        if (formulario.FechaCierre == default)
            return ServiceResult<Formulario>.Fail("La fecha de cierre es obligatoria.");

        if (formulario.FechaApertura >= formulario.FechaCierre)
            return ServiceResult<Formulario>.Fail("La fecha de apertura debe ser anterior a la fecha de cierre.");

        var creado = await _repository.CreateAsync(formulario);
        return ServiceResult<Formulario>.Ok(creado, "Formulario creado correctamente.");
    }

    public async Task<ServiceResult<Formulario>> UpdateAsync(int id, Formulario formulario)
    {
        var existente = await _repository.GetByIdAsync(id);
        if (existente is null)
            return ServiceResult<Formulario>.Fail($"Formulario con Id {id} no fue encontrado.");

        if (string.IsNullOrWhiteSpace(formulario.Nombre))
            return ServiceResult<Formulario>.Fail("El nombre del formulario es obligatorio.");

        // ✅ Mismo criterio: sin .HasValue, comparación directa.
        if (formulario.FechaApertura == default)
            return ServiceResult<Formulario>.Fail("La fecha de apertura es obligatoria.");

        if (formulario.FechaCierre == default)
            return ServiceResult<Formulario>.Fail("La fecha de cierre es obligatoria.");

        if (formulario.FechaApertura >= formulario.FechaCierre)
            return ServiceResult<Formulario>.Fail("La fecha de apertura debe ser anterior a la fecha de cierre.");

        await _repository.UpdateAsync(id, formulario);

        formulario.Id = id;
        return ServiceResult<Formulario>.Ok(formulario, "Formulario actualizado correctamente.");
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var existente = await _repository.GetByIdAsync(id);
        if (existente is null)
            return ServiceResult.Fail($"Formulario con Id {id} no fue encontrado.");

        await _repository.DeleteAsync(id);
        return ServiceResult.Ok("Formulario eliminado correctamente.");
    }
}