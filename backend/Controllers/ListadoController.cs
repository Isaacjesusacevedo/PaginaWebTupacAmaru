using Microsoft.AspNetCore.Mvc;
using Backend.Models;
using Backend.DTOs;
using Backend.Services;

namespace Backend.Controllers;

[ApiController]
[Route("api/listado")]
public class ListadoController : ControllerBase
{
    private readonly ICrudJsonService<Alumno> _alumnoService;
    private readonly ICrudJsonService<Carrera> _carreraService;

    public ListadoController()
    {
        _alumnoService = new CrudJsonService<Alumno>("Data/Alumno.json");
        _carreraService = new CrudJsonService<Carrera>("Data/Carrera.json");
    }

    [HttpGet]
    public IActionResult GetListado()
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
}
