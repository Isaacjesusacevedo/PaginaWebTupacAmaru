using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Backend.Models;
using Backend.Services;
using Backend.Exceptions;

namespace Backend.Controllers;

[ApiController]
[Authorize]
[Route("api/administradores")]
public class AdministradorController : ControllerBase
{
    private readonly ICrudJsonService<Administrador> _service;

    public AdministradorController(ICrudJsonService<Administrador> service)
    {
        _service = service;
    }

    // GET: api/administradores
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
            return StatusCode(500, new { error = "Error interno al obtener los administradores." });
        }
    }

    // GET: api/administradores/1
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
            return StatusCode(500, new { error = "Error interno al buscar el administrador." });
        }
    }

    // POST: api/administradores
    [HttpPost]
    public IActionResult Create([FromBody] Administrador administrador)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var creado = _service.Create(administrador);
            return CreatedAtAction(nameof(GetById), new { id = creado.Id }, creado);
        }
        catch (PersistenceException ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Error interno al crear el administrador." });
        }
    }

    // PUT: api/administradores/1
    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] Administrador administrador)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            _service.Update(id, administrador);
            return Ok(administrador);
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
            return StatusCode(500, new { error = "Error interno al actualizar el administrador." });
        }
    }

    // DELETE: api/administradores/1
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
            return StatusCode(500, new { error = "Error interno al eliminar el administrador." });
        }
    }
}
