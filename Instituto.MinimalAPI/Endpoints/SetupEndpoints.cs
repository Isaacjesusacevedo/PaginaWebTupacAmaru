using Instituto.BR.DTOs;
using Instituto.BR.Interfaces;

namespace Instituto.MinimalAPI.Endpoints;

public static class SetupEndpoints
{
    public static void MapSetupEndpoints(this WebApplication app)
    {
        app.MapPost("/api/setup/admin", async (SetupAdminDto dto, IAdministradorService authService) =>
        {
            if (!app.Environment.IsDevelopment())
                return Results.NotFound();

            var hayAdmins = await authService.HayAdminsAsync();
            if (hayAdmins)
                return Results.Conflict(new { isSuccess = false, message = "Ya existe al menos un administrador. Este endpoint está deshabilitado." });

            var admin = await authService.CrearPrimerAdminAsync(dto);

            if (admin is null)
                return Results.Json(new { isSuccess = false, message = "Error al crear el administrador inicial." }, statusCode: 500);

            return Results.Ok(new { isSuccess = true, message = $"Administrador '{admin.Email}' creado correctamente. Este endpoint ya no puede volver a usarse." });
        }).WithName("CrearPrimerAdmin").AllowAnonymous();

        app.MapGet("/api/setup/status", async (IAdministradorService authService) =>
        {
            var hayAdmins = await authService.HayAdminsAsync();
            return Results.Ok(new { isSuccess = true, message = "Operación exitosa", data = new { hasAdmin = hayAdmins } });
        }).WithName("SetupStatus").WithTags("Setup");
    }
}