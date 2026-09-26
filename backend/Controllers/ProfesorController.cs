using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Backend.Models;
using Backend.Services;
using Backend.Exceptions;

namespace Backend.Controllers;

[ApiController]
[Authorize]
[Route("api/profesores")]
public class ProfesorController : ControllerBase
{
    private readonly ICrudJsonService<Profesor> _service;

    public ProfesorController(ICrudJsonService<Profesor> service)
    {
        _service = service;
    }

    // GET: api/profesores
    [HttpGet]
    public IActionResult GetAll()
    {
        try { return Ok(_service.GetAll()); }
        catch (PersistenceException ex) { return StatusCode(500, new { error = ex.Message }); }
        catch (Exception) { return StatusCode(500, new { error = "Error interno al obtener los profesores." }); }
    }

    // GET: api/profesores/1
    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        try { return Ok(_service.GetById(id)); }
        catch (EntityNotFoundException ex) { return NotFound(new { error = ex.Message }); }
        catch (PersistenceException ex) { return StatusCode(500, new { error = ex.Message }); }
        catch (Exception) { return StatusCode(500, new { error = "Error interno al buscar el profesor." }); }
    }

    // POST: api/profesores
    [HttpPost]
    public IActionResult Create([FromBody] Profesor profesor)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var creado = _service.Create(profesor);
            return CreatedAtAction(nameof(GetById), new { id = creado.Id }, creado);
        }
        catch (PersistenceException ex) { return StatusCode(500, new { error = ex.Message }); }
        catch (Exception) { return StatusCode(500, new { error = "Error interno al crear el profesor." }); }
    }

    // PUT: api/profesores/1
    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] Profesor profesor)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            _service.Update(id, profesor);
            return Ok(profesor);
        }
        catch (EntityNotFoundException ex) { return NotFound(new { error = ex.Message }); }
        catch (PersistenceException ex) { return StatusCode(500, new { error = ex.Message }); }
        catch (Exception) { return StatusCode(500, new { error = "Error interno al actualizar el profesor." }); }
    }

    // DELETE: api/profesores/1
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        try
        {
            _service.Delete(id);
            return NoContent();
        }
        catch (EntityNotFoundException ex) { return NotFound(new { error = ex.Message }); }
        catch (PersistenceException ex) { return StatusCode(500, new { error = ex.Message }); }
        catch (Exception) { return StatusCode(500, new { error = "Error interno al eliminar el profesor." }); }
    }
}
