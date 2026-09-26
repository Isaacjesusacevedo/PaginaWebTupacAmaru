using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Backend.Models;
using Backend.Services;
using Backend.Exceptions;

namespace Backend.Controllers;

[ApiController]
[Authorize]
[Route("api/formularios")]
public class FormularioController : ControllerBase
{
    private readonly ICrudJsonService<Formulario> _service;

    public FormularioController(ICrudJsonService<Formulario> service)
    {
        _service = service;
    }

    // GET: api/formularios
    [HttpGet]
    public IActionResult GetAll()
    {
        try
        {
            return Ok(_service.GetAll());
        }
        catch (PersistenceException ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Error interno al obtener los formularios." });
        }
    }

    // GET: api/formularios/1
    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        try
        {
            return Ok(_service.GetById(id));
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
            return StatusCode(500, new { error = "Error interno al buscar el formulario." });
        }
    }

    // POST: api/formularios
    [HttpPost]
    public IActionResult Create([FromBody] Formulario formulario)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var creado = _service.Create(formulario);
            return CreatedAtAction(nameof(GetById), new { id = creado.Id }, creado);
        }
        catch (PersistenceException ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Error interno al crear el formulario." });
        }
    }

    // PUT: api/formularios/1
    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] Formulario formulario)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            _service.Update(id, formulario);
            return Ok(formulario);
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
            return StatusCode(500, new { error = "Error interno al actualizar el formulario." });
        }
    }

    // DELETE: api/formularios/1
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        try
        {
            _service.Delete(id);
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
            return StatusCode(500, new { error = "Error interno al eliminar el formulario." });
        }
    }
}