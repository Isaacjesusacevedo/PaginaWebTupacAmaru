using Instituto.BR.DTOs;
using Instituto.BR.Interfaces;
using Instituto.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace Instituto.API.Controllers;

[ApiController]
[Route("api/setup")]
public class SetupController : ControllerBase
{
    private readonly IAdministradorService _authService;
    private readonly IWebHostEnvironment _env;

    public SetupController(IAdministradorService authService, IWebHostEnvironment env)
    {
        _authService = authService;
        _env = env;
    }

    [HttpPost("admin")]
    public async Task<IActionResult> CrearPrimerAdmin([FromBody] SetupAdminRequest dto)
    {
        if (!_env.IsDevelopment())
            return NotFound();

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (_authService.HayAdmins())
            return Conflict(ApiResponse<string>.Error("Ya existe al menos un administrador. Este endpoint está deshabilitado."));

        var setupDto = new SetupAdminDto
        {
            Nombre = dto.Nombre,
            Apellido = dto.Apellido,
            Email = dto.Email,
            Password = dto.Password,
            Role = dto.Role
        };

        var admin = await _authService.CrearPrimerAdminAsync(setupDto);

        if (admin is null)
            return StatusCode(500, ApiResponse<string>.Error("Error al crear el administrador inicial."));

        return Ok(ApiResponse<string>.Success($"Administrador '{admin.Email}' creado correctamente. Este endpoint ya no puede volver a usarse."));
    }
}