using System;
using Instituto.BR.DTOs;
using Instituto.BR.Interfaces;
using Instituto.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace Instituto.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAdministradorService _authService;

    public AuthController(IAdministradorService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var admin = _authService.Login(dto.Email, dto.Password);

        if (admin is null)
            return Unauthorized(ApiResponse<string>.Error("Email o contraseña incorrectos."));

        // Generar token de sesión simple (no JWT)
        var sessionToken = Guid.NewGuid().ToString("N");
        var expira = DateTime.UtcNow.AddHours(8);

        return Ok(ApiResponse<LoginResponse>.Success(new LoginResponse
        {
            Token = sessionToken,
            ExpiraEn = expira,
            Admin = admin
        }));
    }

    [HttpPost("verify-password")]
    public IActionResult VerifyPassword([FromBody] VerifyPasswordRequest dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (string.IsNullOrEmpty(dto.Email))
            return BadRequest(ApiResponse<string>.Error("Email requerido para verificar contraseña"));

        var admin = _authService.Login(dto.Email, dto.Password);

        if (admin is null)
            return Unauthorized(ApiResponse<string>.Error("Contraseña incorrecta"));

        return Ok(ApiResponse<string>.Success("Contraseña verificada correctamente"));
    }
}