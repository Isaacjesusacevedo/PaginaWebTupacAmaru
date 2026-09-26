using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Backend.Models;
using Backend.DTOs;
using Backend.Services;
using Backend.Exceptions;

namespace Backend.Controllers;

[ApiController]
[Authorize]
[Route("api/listado")]
public class ListadoController : ControllerBase
{
    private readonly ICrudJsonService<Alumno> _alumnoService;
    private readonly ICrudJsonService<Carrera> _carreraService;

    public ListadoController(
        ICrudJsonService<Alumno> alumnoService,
        ICrudJsonService<Carrera> carreraService)
    {
        _alumnoService = alumnoService;
        _carreraService = carreraService;
    }

    // GET: api/listado
    [HttpGet]
    public IActionResult GetListado()
    {
        try
        {
            var alumnos = _alumnoService.GetAll();
            var carreras = _carreraService.GetAll();

            var listado = alumnos.Select(a =>
            {
                var carrera = carreras.FirstOrDefault(c => c.Id == a.CarreraId);
                return new AlumnoListadoDto
                {
                    AlumnoId = a.Id,
                    NombreCompleto = $"{a.Apellido}, {a.Nombre}",
                    DNI = a.DNI,
                    Email = a.Email,
                    Carrera = carrera?.Nombre ?? "Sin carrera",
                    Turno = a.Turno,
                    Edad = a.Edad
                };
            }).ToList();

            return Ok(listado);
        }
        catch (PersistenceException ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Error interno al generar el listado." });
        }
    }
}
