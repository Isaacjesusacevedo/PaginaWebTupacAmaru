using Microsoft.AspNetCore.Mvc;
using Backend.Models;
using Backend.Services;

namespace Backend.Controllers;

[ApiController]
[Route("api/alumnos")]
public class AlumnosController : ControllerBase
{
    private readonly ICrudJsonService<Alumno> _service;

    public AlumnosController()
    {
        _service = new CrudJsonService<Alumno>("Data/Alumno.json");
    }

    [HttpGet]
    public IActionResult GetAll() => Ok(_service.GetAll());

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var alumno = _service.GetById(id);
        return alumno != null ? Ok(alumno) : NotFound();
    }

    [HttpPost]
    public IActionResult Create([FromBody] Alumno alumno)
    {
        var list = _service.GetAll();
        alumno.Id = list.Count == 0 ? 1 : list.Max(a => a.Id) + 1;
        alumno.FechaInscripcion ??= DateTime.Now;
        _service.Create(alumno);
        return Ok(alumno);
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] Alumno alumno)
    {
        _service.Update(id, alumno);
        return Ok(alumno);
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        _service.Delete(id);
        return NoContent();
    }
}
