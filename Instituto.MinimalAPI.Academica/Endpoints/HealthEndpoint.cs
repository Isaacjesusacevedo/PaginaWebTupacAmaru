using Instituto.AD.Data;

namespace Instituto.MinimalAPI.Academica.Endpoints;

public static class HealthEndpoint
{
    public static void MapHealthEndpoint(this WebApplication app)
    {
        app.MapGet("/health", async (InstitutoDbContext db) =>
        {
            try
            {
                var conectado = await db.Database.CanConnectAsync();
                return Results.Ok(new
                {
                    status = conectado ? "OK" : "error",
                    timestamp = DateTime.UtcNow,
                    database = conectado ? "connected" : "error"
                });
            }
            catch
            {
                return Results.Ok(new { status = "error", timestamp = DateTime.UtcNow, database = "error" });
            }
        })
        .WithName("HealthCheck")
        .WithTags("Health")
        .WithSummary("Verifica que la API y la base de datos estén disponibles.")
        .Produces(StatusCodes.Status200OK)
        .AllowAnonymous();
    }
}
