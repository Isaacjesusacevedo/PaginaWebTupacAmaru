using Instituto.AD.Data;

namespace Instituto.MinimalAPI.Endpoints;

public static class HealthEndpoints
{
    public static void MapHealthEndpoints(this WebApplication app)
    {
        app.MapGet("/", () => "Backend corriendo correctamente!");

        app.MapGet("/health", async (InstitutoDbContext db) =>
        {
            try
            {
                await db.Database.CanConnectAsync();
                return Results.Ok(new { status = "OK", timestamp = DateTime.UtcNow, database = "connected" });
            }
            catch (Exception ex)
            {
                return Results.Ok(new
                {
                    status = "error",
                    timestamp = DateTime.UtcNow,
                    database = "error",
                    detalle = ex.Message
                });
            }
        }).WithName("HealthCheck").WithTags("Health");

        app.MapGet("/api/stats", async (
            Instituto.BR.Interfaces.ICarreraService carreraService,
            Instituto.BR.Interfaces.IAlumnoService alumnoService,
            Instituto.BR.Interfaces.IAdministradorService adminService,
            Instituto.BR.Interfaces.IProfesorService profesorService,
            Instituto.BR.Interfaces.IFormularioService formularioService) =>
        {
            return Results.Ok(new
            {
                isSuccess = true,
                message = "Operación exitosa",
                data = new
                {
                    totalCarreras = (await carreraService.GetAllAsync()).Count,
                    totalAlumnos = (await alumnoService.GetAllAsync()).Count,
                    totalAdmins = (await adminService.GetAllAsync()).Count,
                    totalProfesores = (await profesorService.GetAllAsync()).Count,
                    totalFormularios = (await formularioService.GetAllAsync()).Count
                }
            });
        }).WithName("GetStats").WithTags("Stats");
    }
}