using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Backend.Models;
using Backend.Services;
using Backend.Exceptions;

namespace Backend.Controllers;

[ApiController]
[Authorize]
[Route("api/alumnos")]
public class AlumnosController : ControllerBase
{
    private readonly ICrudJsonService<Alumno> _alumnoService;
    private readonly ICrudJsonService<Carrera> _carreraService;

    public AlumnosController(
        ICrudJsonService<Alumno> alumnoService,
        ICrudJsonService<Carrera> carreraService)
    {
        _alumnoService = alumnoService;
        _carreraService = carreraService;
    }

    // GET: api/alumnos
    [HttpGet]
    public IActionResult GetAll()
    {
        try
        {
            return Ok(_alumnoService.GetAll());
        }
        catch (PersistenceException ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Error interno al obtener los alumnos." });
        }
    }

    // GET: api/alumnos/1
    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        try
        {
            return Ok(_alumnoService.GetById(id));
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
            return StatusCode(500, new { error = "Error interno al buscar el alumno." });
        }
    }

    // POST: api/alumnos  → público para el formulario de inscripción
    [AllowAnonymous]
    [HttpPost]
    public IActionResult Create([FromBody] Alumno alumno)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            // Validar que la carrera existe
            _carreraService.GetById(alumno.CarreraId);

            alumno.FechaInscripcion ??= DateTime.Now;

            var creado = _alumnoService.Create(alumno);
            return CreatedAtAction(nameof(GetById), new { id = creado.Id }, creado);
        }
        catch (EntityNotFoundException)
        {
            return BadRequest(new { error = $"La carrera con Id {alumno.CarreraId} no existe." });
        }
        catch (PersistenceException ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Error interno al crear el alumno." });
        }
    }

    // PUT: api/alumnos/1
    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] Alumno alumno)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            // Validar que la carrera existe
            _carreraService.GetById(alumno.CarreraId);

            _alumnoService.Update(id, alumno);
            return Ok(alumno);
        }
        catch (EntityNotFoundException ex)
        {
            // Distinguir si es la carrera o el alumno quien no existe
            if (ex.Message.Contains("Carrera"))
                return BadRequest(new { error = $"La carrera con Id {alumno.CarreraId} no existe." });

            return NotFound(new { error = ex.Message });
        }
        catch (PersistenceException ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Error interno al actualizar el alumno." });
        }
    }

    // DELETE: api/alumnos/1
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        try
        {
            _alumnoService.Delete(id);
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
            return StatusCode(500, new { error = "Error interno al eliminar el alumno." });
        }
    }
}
