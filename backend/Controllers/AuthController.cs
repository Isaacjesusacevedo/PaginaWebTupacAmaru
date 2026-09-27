using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Backend.Models;
using Backend.Services;

namespace Backend.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAdminAuthService _authService;
    private readonly IConfiguration _config;

    public AuthController(IAdminAuthService authService, IConfiguration config)
    {
        _authService = authService;
        _config = config;
    }

    // POST: api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var admin = await _authService.LoginAsync(dto.Email, dto.Password);

            if (admin is null)
                return Unauthorized(new { error = "Email o contraseña incorrectos." });

            var token = GenerarToken(admin);
            var expira = DateTime.UtcNow.AddHours(8);

            return Ok(new
            {
                token,
                expiraEn = expira,
                admin = new
                {
                    admin.Id,
                    admin.Nombre,
                    admin.Apellido,
                    admin.Email,
                    admin.Role
                }
            });
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Error interno al intentar iniciar sesión." });
        }
    }

    // POST: api/auth/verify-password
    [HttpPost("verify-password")]
    [Authorize]
    public async Task<IActionResult> VerifyPassword([FromBody] VerifyPasswordDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            // Obtener el email del usuario autenticado desde el token JWT
            var email = User.FindFirst(ClaimTypes.Email)?.Value ?? User.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
            
            if (string.IsNullOrEmpty(email))
                return Unauthorized(new { error = "Token inválido" });

            var admin = await _authService.LoginAsync(email, dto.Password);

            if (admin is null)
                return Unauthorized(new { error = "Contraseña incorrecta" });

            return Ok(new { mensaje = "Contraseña verificada correctamente" });
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Error interno al verificar contraseña" });
        }
    }

    // ── Generación del JWT ───────────────────────────────────────────────────────

    private string GenerarToken(AdminResult admin)
    {
        var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,   admin.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, admin.Email),
            new Claim(ClaimTypes.Name,               $"{admin.Nombre} {admin.Apellido}"),
            new Claim(ClaimTypes.Role,               admin.Role),
            new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer:            _config["Jwt:Issuer"],
            audience:          _config["Jwt:Audience"],
            claims:            claims,
            expires:           DateTime.UtcNow.AddHours(8),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
