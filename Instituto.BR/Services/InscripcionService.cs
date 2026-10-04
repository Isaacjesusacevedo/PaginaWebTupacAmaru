using Instituto.AD.Models;
using Instituto.BR.DTOs;
using Instituto.BR.Interfaces;

namespace Instituto.BR.Services;

public class InscripcionService : IInscripcionService
{
    private readonly IAlumnoService _alumnoService;
    private readonly IInfAcademicaEstService _infAcademicaEstService;

    public InscripcionService(
        IAlumnoService alumnoService,
        IInfAcademicaEstService infAcademicaEstService)
    {
        _alumnoService = alumnoService ?? throw new ArgumentNullException(nameof(alumnoService));
        _infAcademicaEstService = infAcademicaEstService ?? throw new ArgumentNullException(nameof(infAcademicaEstService));
    }

    public async Task<ServiceResult<InscripcionResultDto>> InscribirAsync(InscripcionDto dto)
    {
        // 1. Validar el bloque académico PRIMERO (sin tocar la BD)
        if (!dto.PoseeTitulo && !dto.TituloEnTramite && !dto.ConsMaterias && !dto.ConsAlumnoRegular)
            return ServiceResult<InscripcionResultDto>.Fail("Debe informar al menos un dato académico.");

        if (dto.PoseeTitulo && dto.TituloEnTramite)
            return ServiceResult<InscripcionResultDto>.Fail("No puede poseer el título y tenerlo en trámite al mismo tiempo.");

        if ((dto.PoseeTitulo || dto.TituloEnTramite) && string.IsNullOrWhiteSpace(dto.TituloSecundario))
            return ServiceResult<InscripcionResultDto>.Fail("Si posee título (o está en trámite), debe informar el título secundario.");

        if (dto.PoseeTitulo && dto.FechaEgreso is null)
            return ServiceResult<InscripcionResultDto>.Fail("Si posee el título, debe informar la fecha de egreso.");

        // 2. Crear el alumno
        var alumno = new Alumno
        {
            Nombre = dto.Nombre,
            Apellido = dto.Apellido,
            Email = dto.Email.ToLower().Trim(),
            DNI = dto.Dni,
            FechaNacimiento = dto.FechaNacimiento,
            Direccion = dto.Direccion,
            Nacionalidad = dto.Nacionalidad,
            Telefono = dto.Telefono,
            Turno = dto.Turno,
            CarreraId = dto.CarreraId,
            FechaInscripcion = DateTime.Now
        };

        var resultAlumno = await _alumnoService.CreateAsync(alumno);
        if (!resultAlumno.Success)
            return ServiceResult<InscripcionResultDto>.Fail(resultAlumno.Message!);

        var alumnoId = resultAlumno.Data!.Id;

        // 3. Crear la info académica (si falla, borrar el alumno)
        var infoAcademica = new InfAcademicaEst
        {
            AlumnoId = alumnoId,
            FechaEgreso = dto.FechaEgreso,
            TituloSecundario = dto.TituloSecundario,
            PoseeTitulo = dto.PoseeTitulo,
            TituloEnTramite = dto.TituloEnTramite,
            ConsMaterias = dto.ConsMaterias,
            ConsAlumnoRegular = dto.ConsAlumnoRegular
        };

        var resultInfo = await _infAcademicaEstService.CreateAsync(infoAcademica);

        if (!resultInfo.Success)
        {
            // Rollback manual
            await _alumnoService.DeleteAsync(alumnoId);
            return ServiceResult<InscripcionResultDto>.Fail(
                "Error al guardar la información académica. Se canceló la inscripción.");
        }

        return ServiceResult<InscripcionResultDto>.Ok(
            new InscripcionResultDto { AlumnoId = alumnoId },
            "Inscripción completada correctamente.");
    }
}