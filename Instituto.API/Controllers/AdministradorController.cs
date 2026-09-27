using Instituto.BR.Interfaces;
using Instituto.BR.DTOs;
using Instituto.AD.Models;
using Instituto.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Instituto.API.Controllers;

[ApiController]
[Authorize]
[Route("api/administradores")]
public class AdministradorController : ControllerBase
{
    private readonly IAdministradorService _service;

    public AdministradorController(IAdministradorService service)
    {
        _service = service;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(ApiResponse<List<Administrador>>.Success(_service.GetAll()));
    }

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var admin = _service.GetById(id);
        if (admin is null)
            return NotFound(ApiResponse<Administrador>.Error($"Administrador con Id {id} no fue encontrado."));

        return Ok(ApiResponse<Administrador>.Success(admin));
    }

    [HttpPost]
    public IActionResult Create([FromBody] Administrador admin)
    {
        return BadRequest(ApiResponse<string>.Error("Use the DTO endpoint for creation with password"));
    }

    [HttpPost("with-password")]
    public IActionResult CreateWithPassword([FromBody] AdminCreateDto dto)
    {
        var admin = new Administrador
        {
            Nombre = dto.Nombre,
            Apellido = dto.Apellido,
            Email = dto.Email,
            Role = dto.Role
        };

        var result = _service.Create(admin, dto.Password);
        if (!result.Success)
            return BadRequest(ApiResponse<Administrador>.Error(result.Message!));

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, ApiResponse<Administrador>.Success(result.Data));
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] Administrador admin)
    {
        var result = _service.Update(id, admin);
        if (!result.Success)
            return NotFound(ApiResponse<Administrador>.Error(result.Message!));

        return Ok(ApiResponse<Administrador>.Success(result.Data));
    }

    [HttpPut("{id:int}/password")]
    public IActionResult ChangePassword(int id, [FromBody] ChangePasswordRequest dto)
    {
        var result = _service.ChangePassword(id, dto.PasswordActual, dto.NuevaPassword);
        if (!result.Success)
            return Unauthorized(ApiResponse<string>.Error(result.Message!));

        return Ok(ApiResponse<string>.Success(result.Message!));
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var result = _service.Delete(id);
        if (!result.Success)
            return NotFound(ApiResponse<string>.Error(result.Message!));

        return NoContent();
    }
}

public record AdminCreateDto
{
    public string Nombre { get; init; } = string.Empty;
    public string Apellido { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Role { get; init; } = "Admin";
    public string Password { get; init; } = string.Empty;
}