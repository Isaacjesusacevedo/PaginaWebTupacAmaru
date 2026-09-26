using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Backend.Models;
using Backend.Services;
using Backend.Exceptions;

namespace Backend.Controllers;

[ApiController]
[Route("api/carreras")]
public class CarrerasController : ControllerBase
{
    private readonly ICrudJsonService<Carrera> _carreraService;
    private readonly ICrudJsonService<Alumno> _alumnoService;

    public CarrerasController(
        ICrudJsonService<Carrera> carreraService,
        ICrudJsonService<Alumno> alumnoService)
    {
        _carreraService = carreraService;
        _alumnoService = alumnoService;
    }

    // GET: api/carreras
    [HttpGet]
    public IActionResult GetAll()
    {
        try
        {
            return Ok(_carreraService.GetAll());
        }
        catch (PersistenceException ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Error interno al obtener las carreras." });
        }
    }

    // GET: api/carreras/1
    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        try
        {
            return Ok(_carreraService.GetById(id));
        }
        catch (EntityNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (PersistenceException ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Error interno al buscar la carrera." });
        }
    }

    // POST: api/carreras
    [Authorize]
    [HttpPost]
    public IActionResult Create([FromBody] Carrera carrera)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            carrera.Estado ??= "Activa";
            var creada = _carreraService.Create(carrera);
            return CreatedAtAction(nameof(GetById), new { id = creada.Id }, creada);
        }
        catch (PersistenceException ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Error interno al crear la carrera." });
        }
    }

    // PUT: api/carreras/1
    [Authorize]
    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] Carrera carrera)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            _carreraService.Update(id, carrera);
            return Ok(carrera);
        }
        catch (EntityNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (PersistenceException ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Error interno al actualizar la carrera." });
        }
    }

    // DELETE: api/carreras/1
    [Authorize]
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        try
        {
            // Regla de negocio: no eliminar si hay alumnos inscriptos
            var alumnos = _alumnoService.GetAll();
            if (alumnos.Any(a => a.CarreraId == id))
                return BadRequest(new { error = "No se puede eliminar la carrera porque tiene alumnos inscriptos." });

            _carreraService.Delete(id);
            return NoContent();
        }
        catch (EntityNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (PersistenceException ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Error interno al eliminar la carrera." });
        }
    }
}
