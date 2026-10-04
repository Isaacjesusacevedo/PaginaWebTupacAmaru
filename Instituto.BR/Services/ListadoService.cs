using Instituto.AD.Interfaces;
using Instituto.AD.Models;
using Instituto.BR.DTOs;
using Instituto.BR.Interfaces;

namespace Instituto.BR.Services;

public class ListadoService : IListadoService
{
    private readonly IListadoRepository _repository;

    public ListadoService(IListadoRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<List<AlumnoListadoDto>> GetListadoAsync()
    {
        var items = await _repository.GetListadoAsync();

        return items
            .GroupBy(i => i.AlumnoId)
            .Select(ElegirInformacionAcademica)
            .OrderBy(dto => dto.AlumnoId)
            .ToList();
    }

    private static AlumnoListadoDto ElegirInformacionAcademica(IGrouping<int, ListadoItem> grupo)
    {
        var elegido = grupo
            .OrderBy(i => PrioridadTipoAcademico(i.TipoAcademico))
            .ThenByDescending(i => i.FechaEmision)
            .First();

        return new AlumnoListadoDto
        {
            AlumnoId = elegido.AlumnoId,
            NombreCompleto = elegido.NombreCompleto,
            DNI = elegido.DNI,
            Email = elegido.Email,
            Carrera = elegido.Carrera,
            Turno = elegido.Turno,
            Edad = CalcularEdad(elegido.FechaNacimiento),
            TipoAcademico = elegido.TipoAcademico,
            EstadoTitulo = elegido.EstadoTitulo,
            FechaEmision = elegido.FechaEmision,
            TituloSecundario = elegido.TipoAcademico == InfAcademicaConstants.TipoTitulo ? elegido.TituloSecundario : null
        };
    }

    private static int PrioridadTipoAcademico(string? tipo) => tipo switch
    {
        InfAcademicaConstants.TipoTitulo => 1,
        InfAcademicaConstants.TipoTituloEnTramite => 2,
        InfAcademicaConstants.TipoConstanciaMateriasAdeudadas => 3,
        InfAcademicaConstants.TipoConstanciaAlumnoRegular => 3,
        _ => 4
    };

    private static int CalcularEdad(DateTime fechaNacimiento)
    {
        var hoy = DateTime.Today;
        var edad = hoy.Year - fechaNacimiento.Year;
        if (fechaNacimiento.Date > hoy.AddYears(-edad))
            edad--;
        return edad;
    }
}
