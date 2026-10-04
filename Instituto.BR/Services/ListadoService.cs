using Instituto.AD.Interfaces;
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
            .Select(i => new AlumnoListadoDto
            {
                AlumnoId = i.AlumnoId,
                NombreCompleto = i.NombreCompleto,
                DNI = i.DNI,
                Email = i.Email,
                Carrera = i.Carrera,
                Turno = i.Turno,
                Edad = CalcularEdad(i.FechaNacimiento),
                FechaEgreso = i.FechaEgreso,
                TituloSecundario = i.TituloSecundario,
                PoseeTitulo = i.PoseeTitulo,
                TituloEnTramite = i.TituloEnTramite,
                ConsMaterias = i.ConsMaterias,
                ConsAlumnoRegular = i.ConsAlumnoRegular
            })
            .OrderBy(dto => dto.AlumnoId)
            .ToList();
    }

    private static int CalcularEdad(DateTime fechaNacimiento)
    {
        var hoy = DateTime.Today;
        var edad = hoy.Year - fechaNacimiento.Year;
        if (fechaNacimiento.Date > hoy.AddYears(-edad))
            edad--;
        return edad;
    }
}