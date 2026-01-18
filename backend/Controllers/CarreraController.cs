using Microsoft.AspNetCore.Mvc;
using Backend.Models;
using Backend.Services;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")] // 👈 IMPORTANTE
public class CarrerasController : ControllerBase
{
    private readonly ICrudJsonService<Carrera> _carreraService;
    private readonly ICrudJsonService<Alumno> _alumnoService;

    public CarrerasController()
    {
        _carreraService = new CrudJsonService<Carrera>("Data/Carrera.json");
        _alumnoService = new CrudJsonService<Alumno>("Data/Alumno.json");
    }

    // GET: api/carreras
    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_carreraService.GetAll());
    }

    // GET: api/carreras/1
    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var carrera = _carreraService.GetById(id);
        if (carrera == null)
            return NotFound("Carrera no encontrada");

        return Ok(carrera);
    }

    // POST: api/carreras
    [HttpPost]
    public IActionResult Create([FromBody] Carrera carrera)
    {
        var list = _carreraService.GetAll();
        carrera.Id = list.Count == 0 ? 1 : list.Max(c => c.Id) + 1;
        carrera.Estado ??= "Activa";

        _carreraService.Create(carrera);
        return CreatedAtAction(nameof(GetById), new { id = carrera.Id }, carrera);
    }

    // PUT: api/carreras/1
    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] Carrera carrera)
    {
        var existente = _carreraService.GetById(id);
        if (existente == null)
            return NotFound("Carrera no encontrada");

        carrera.Id = id; // 👈 CLAVE
        _carreraService.Update(id, carrera);

        return Ok(carrera);
    }

    // DELETE: api/carreras/1
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var alumnos = _alumnoService.GetAll();
        bool tieneAlumnos = alumnos.Any(a => a.CarreraId == id);

        if (tieneAlumnos)
            return BadRequest("No se puede eliminar la carrera porque tiene alumnos inscritos.");

        _carreraService.Delete(id);
        return NoContent();
    }
}
