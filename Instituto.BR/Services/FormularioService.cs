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

    public List<Formulario> GetAll() => _repository.GetAll();

    public Formulario? GetById(int id) => _repository.GetById(id);

    public ServiceResult<Formulario> Create(Formulario formulario)
    {
        if (string.IsNullOrWhiteSpace(formulario.Nombre))
            return ServiceResult<Formulario>.Fail("El nombre es obligatorio.");

        if (string.IsNullOrWhiteSpace(formulario.Estado))
            return ServiceResult<Formulario>.Fail("El estado es obligatorio.");

        if (formulario.FechaApertura == default)
            return ServiceResult<Formulario>.Fail("La fecha de apertura es obligatoria.");

        if (formulario.FechaCierre == default)
            return ServiceResult<Formulario>.Fail("La fecha de cierre es obligatoria.");

        if (formulario.FechaCierre <= formulario.FechaApertura)
            return ServiceResult<Formulario>.Fail("La fecha de cierre debe ser posterior a la de apertura.");

        var validEstados = new[] { "Borrador", "Abierto", "Cerrado" };
        if (!validEstados.Contains(formulario.Estado))
            return ServiceResult<Formulario>.Fail("El estado debe ser: Borrador, Abierto o Cerrado.");

        var created = _repository.Create(formulario);
        return ServiceResult<Formulario>.Ok(created, "Formulario creado correctamente.");
    }

    public ServiceResult<Formulario> Update(int id, Formulario formulario)
    {
        if (!_repository.Exists(id))
            return ServiceResult<Formulario>.Fail($"El formulario con Id {id} no existe.");

        if (string.IsNullOrWhiteSpace(formulario.Nombre))
            return ServiceResult<Formulario>.Fail("El nombre es obligatorio.");

        if (string.IsNullOrWhiteSpace(formulario.Estado))
            return ServiceResult<Formulario>.Fail("El estado es obligatorio.");

        if (formulario.FechaApertura == default)
            return ServiceResult<Formulario>.Fail("La fecha de apertura es obligatoria.");

        if (formulario.FechaCierre == default)
            return ServiceResult<Formulario>.Fail("La fecha de cierre es obligatoria.");

        if (formulario.FechaCierre <= formulario.FechaApertura)
            return ServiceResult<Formulario>.Fail("La fecha de cierre debe ser posterior a la de apertura.");

        var validEstados = new[] { "Borrador", "Abierto", "Cerrado" };
        if (!validEstados.Contains(formulario.Estado))
            return ServiceResult<Formulario>.Fail("El estado debe ser: Borrador, Abierto o Cerrado.");

        _repository.Update(id, formulario);
        return ServiceResult<Formulario>.Ok(formulario, "Formulario actualizado correctamente.");
    }

    public ServiceResult Delete(int id)
    {
        if (!_repository.Exists(id))
            return ServiceResult.Fail($"El formulario con Id {id} no existe.");

        _repository.Delete(id);
        return ServiceResult.Ok("Formulario eliminado correctamente.");
    }
}