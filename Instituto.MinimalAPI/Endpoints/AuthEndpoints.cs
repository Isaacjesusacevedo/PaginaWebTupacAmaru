using Instituto.BR.Interfaces;
using Instituto.MinimalAPI.Models;

namespace Instituto.MinimalAPI.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/login", async (LoginRequest dto, IAdministradorService authService) =>
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                return Results.BadRequest(new { isSuccess = false, message = "Email y contraseña requeridos" });

            var admin = await authService.LoginAsync(dto.Email, dto.Password);

            if (admin is null)
                return Results.Unauthorized();

            var sessionToken = Guid.NewGuid().ToString("N");
            var expira = DateTime.UtcNow.AddHours(8);

            return Results.Ok(new
            {
                isSuccess = true,
                message = "Operación exitosa",
                data = new
                {
                    Token = sessionToken,
                    ExpiraEn = expira,
                    Admin = admin
                }
            });
        }).WithName("Login").AllowAnonymous();

        group.MapPost("/verify-password", async (VerifyPasswordRequest dto, IAdministradorService authService) =>
        {
            if (string.IsNullOrWhiteSpace(dto.Email))
                return Results.BadRequest(new { isSuccess = false, message = "Email requerido para verificar contraseña" });

            var admin = await authService.LoginAsync(dto.Email, dto.Password);

            if (admin is null)
                return Results.Unauthorized();

            return Results.Ok(new { isSuccess = true, message = "Contraseña verificada correctamente" });
        }).WithName("VerifyPassword");
    }
}