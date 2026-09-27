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
    private readonly IAdminAuthService _authService;

    public AdministradorController(ICrudJsonService<Administrador> service, IAdminAuthService authService)
    {
        _service = service;
        _authService = authService;
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

    // PUT: api/administradores/1/password
    [HttpPut("{id:int}/password")]
    public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            // El email del usuario autenticado se obtiene del token JWT
            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value 
                ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email)?.Value;
            
            if (string.IsNullOrEmpty(email))
                return Unauthorized(new { error = "Token inválido" });

            await _authService.ChangePasswordAsync(email, dto.PasswordActual, dto.NuevaPassword);
            
            return Ok(new { mensaje = "Contraseña actualizada correctamente" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Error interno al cambiar la contraseña." });
        }
    }
}
