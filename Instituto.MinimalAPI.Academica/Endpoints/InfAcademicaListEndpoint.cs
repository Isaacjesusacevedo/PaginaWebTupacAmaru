using Instituto.BR.Interfaces;

namespace Instituto.MinimalAPI.Academica.Endpoints;

public static class InfAcademicaListEndpoint
{
    public static void MapInfAcademicaListEndpoint(this WebApplication app)
    {
        app.MapGet("/api/inf-academica", async (IInfAcademicaService service) =>
        {
            var tipos = await service.GetAllAsync();
            return ApiResults.Ok(tipos);
        })
        .WithName("GetInfAcademica")
        .WithTags("InfAcademica")
        .WithSummary("Lista el catálogo de tipos de información académica.")
        .Produces(StatusCodes.Status200OK)
        .RequireAuthorization("AdminPolicy");
    }
}
