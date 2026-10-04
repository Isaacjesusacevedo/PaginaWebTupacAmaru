using Instituto.AD.Interfaces;
using Instituto.AD.Models;
using Instituto.BR.DTOs;
using Instituto.BR.Interfaces;
using Microsoft.Data.SqlClient;

namespace Instituto.BR.Services;

public class InscripcionService : IInscripcionService
{
    private readonly IInfAcademicaRepository _infAcademicaRepository;
    private readonly IInfAcademicaEstRepository _infAcademicaEstRepository;
    private readonly IAlumnoRepository _alumnoRepository;
    private readonly IAlumnoService _alumnoService;

    public InscripcionService(
        IInfAcademicaRepository infAcademicaRepository,
        IInfAcademicaEstRepository infAcademicaEstRepository,
        IAlumnoRepository alumnoRepository,
        IAlumnoService alumnoService)
    {
        _infAcademicaRepository = infAcademicaRepository ?? throw new ArgumentNullException(nameof(infAcademicaRepository));
        _infAcademicaEstRepository = infAcademicaEstRepository ?? throw new ArgumentNullException(nameof(infAcademicaEstRepository));
        _alumnoRepository = alumnoRepository ?? throw new ArgumentNullException(nameof(alumnoRepository));
        _alumnoService = alumnoService ?? throw new ArgumentNullException(nameof(alumnoService));
    }

    public async Task<ServiceResult<InscripcionResultDto>> InscribirAsync(InscripcionDto dto)
    {
        if (dto.InformacionAcademica is null || dto.InformacionAcademica.Count == 0)
            return ServiceResult<InscripcionResultDto>.Fail("Debe informar al menos un dato académico.");

        var catalogo = await _infAcademicaRepository.GetAllAsync();
        var registros = new List<InfAcademicaEst>();
        var tiposUsados = new HashSet<int>();

        foreach (var item in dto.InformacionAcademica)
        {
            if (string.IsNullOrWhiteSpace(item.Tipo))
                return ServiceResult<InscripcionResultDto>.Fail("El tipo de información académica es obligatorio.");

            var tipo = catalogo.FirstOrDefault(c => c.Descripcion == item.Tipo);
            if (tipo is null)
                return ServiceResult<InscripcionResultDto>.Fail($"El tipo '{item.Tipo}' no existe en el catálogo.");

            if (tipo.Estado != InfAcademicaConstants.CatalogoHabilitado)
                return ServiceResult<InscripcionResultDto>.Fail($"El tipo '{item.Tipo}' no está habilitado.");

            if (!tiposUsados.Add(tipo.Id))
                return ServiceResult<InscripcionResultDto>.Fail($"El tipo '{item.Tipo}' está repetido.");

            var estado = InfAcademicaValidator.ResolverEstadoPorDefecto(item.EstadoTitulo, tipo.Descripcion);

            var errorEstado = InfAcademicaValidator.ValidarEstado(estado);
            if (errorEstado is not null)
                return ServiceResult<InscripcionResultDto>.Fail(errorEstado);

            var errorFecha = InfAcademicaValidator.ValidarFechaEmision(item.FechaEmision, estado);
            if (errorFecha is not null)
                return ServiceResult<InscripcionResultDto>.Fail(errorFecha);

            var (errorCampos, tituloSecundario, institucion) = InfAcademicaValidator.ResolverCamposPorTipo(
                tipo.Descripcion, item.TituloSecundario, item.Institucion);
            if (errorCampos is not null)
                return ServiceResult<InscripcionResultDto>.Fail(errorCampos);

            registros.Add(new InfAcademicaEst
            {
                InfAcademicaId = tipo.Id,
                FechaEmision = item.FechaEmision,
                TituloSecundario = tituloSecundario,
                Institucion = institucion,
                EstadoTitulo = estado
            });
        }

        var alumno = new Alumno
        {
            Nombre = dto.Nombre,
            Apellido = dto.Apellido,
            Email = dto.Email,
            DNI = dto.Dni,
            FechaNacimiento = dto.FechaNacimiento,
            Direccion = dto.Direccion,
            Nacionalidad = dto.Nacionalidad,
            Telefono = dto.Telefono,
            Turno = dto.Turno,
            CarreraId = dto.CarreraId
        };

        var alumnoResult = await _alumnoService.CreateAsync(alumno);
        if (!alumnoResult.Success || alumnoResult.Data is null)
            return ServiceResult<InscripcionResultDto>.Fail(alumnoResult.Message ?? "No se pudo crear el alumno.");

        var alumnoCreado = alumnoResult.Data;

        try
        {
            foreach (var registro in registros)
            {
                registro.AlumnoId = alumnoCreado.Id;
                registro.Id = await _infAcademicaEstRepository.CreateAsync(registro);
            }
        }
        catch (Exception ex)
        {
            await _alumnoRepository.DeleteAsync(alumnoCreado.Id);

            var mensaje = ex is SqlException sqlEx && (sqlEx.Number == 2627 || sqlEx.Number == 2601)
                ? "El alumno ya tiene un registro de ese tipo de información académica."
                : "No se pudo guardar la información académica. Se revirtió la inscripción.";

            return ServiceResult<InscripcionResultDto>.Fail(mensaje);
        }

        return ServiceResult<InscripcionResultDto>.Ok(
            new InscripcionResultDto { AlumnoId = alumnoCreado.Id },
            "Inscripción realizada correctamente.");
    }
}
