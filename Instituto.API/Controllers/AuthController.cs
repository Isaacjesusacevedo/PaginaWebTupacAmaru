using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Instituto.BR.DTOs;
using Instituto.BR.Interfaces;
using Instituto.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Instituto.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAdministradorService _authService;
    private readonly IConfiguration _config;

    public AuthController(IAdministradorService authService, IConfiguration config)
    {
        _authService = authService;
        _config = config;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var admin = _authService.Login(dto.Email, dto.Password);

        if (admin is null)
            return Unauthorized(ApiResponse<string>.Error("Email o contraseña incorrectos."));

        var token = GenerarToken(admin);
        var expira = DateTime.UtcNow.AddHours(8);

        return Ok(ApiResponse<LoginResponse>.Success(new LoginResponse
        {
            Token = token,
            ExpiraEn = expira,
            Admin = admin
        }));
    }

    [HttpPost("verify-password")]
    [Authorize]
    public IActionResult VerifyPassword([FromBody] VerifyPasswordRequest dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var email = User.FindFirst(ClaimTypes.Email)?.Value 
            ?? User.FindFirst(JwtRegisteredClaimNames.Email)?.Value;

        if (string.IsNullOrEmpty(email))
            return Unauthorized(ApiResponse<string>.Error("Token inválido"));

        var admin = _authService.Login(email, dto.Password);

        if (admin is null)
            return Unauthorized(ApiResponse<string>.Error("Contraseña incorrecta"));

        return Ok(ApiResponse<string>.Success("Contraseña verificada correctamente"));
    }

    private string GenerarToken(AdminResult admin)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, admin.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, admin.Email),
            new Claim(ClaimTypes.Name, $"{admin.Nombre} {admin.Apellido}"),
            new Claim(ClaimTypes.Role, admin.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}