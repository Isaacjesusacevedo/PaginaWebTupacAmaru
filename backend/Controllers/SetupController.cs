using Microsoft.AspNetCore.Mvc;
using Backend.Models;
using Backend.Services;

namespace Backend.Controllers;

/// <summary>
/// Endpoint de configuración inicial — solo disponible en entorno de DESARROLLO
/// y únicamente cuando no existe ningún administrador en la base de datos.
/// Una vez creado el primer admin, este endpoint retorna 409 automáticamente.
/// </summary>
[ApiController]
[Route("api/setup")]
public class SetupController : ControllerBase
{
    private readonly IAdminAuthService _authService;
    private readonly IWebHostEnvironment _env;

    public SetupController(IAdminAuthService authService, IWebHostEnvironment env)
    {
        _authService = authService;
        _env = env;
    }

    // POST: api/setup/admin
    [HttpPost("admin")]
    public async Task<IActionResult> CrearPrimerAdmin([FromBody] SetupAdminDto dto)
    {
        // En producción este endpoint no existe
        if (!_env.IsDevelopment())
            return NotFound();

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            if (await _authService.HayAdminsAsync())
                return Conflict(new
                {
                    error = "Ya existe al menos un administrador. Este endpoint está deshabilitado."
                });

            await _authService.CrearAdminAsync(
                dto.Nombre, dto.Apellido, dto.Email, dto.Password, dto.Role);

            return Ok(new
            {
                mensaje = $"Administrador '{dto.Email}' creado correctamente. " +
                          "Este endpoint ya no puede volver a usarse."
            });
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Error al crear el administrador inicial." });
        }
    }
}
