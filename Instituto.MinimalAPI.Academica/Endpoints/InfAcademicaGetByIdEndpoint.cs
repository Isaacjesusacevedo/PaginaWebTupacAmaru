using Instituto.BR.Interfaces;

namespace Instituto.MinimalAPI.Academica.Endpoints;

public static class InfAcademicaGetByIdEndpoint
{
    public static void MapInfAcademicaGetByIdEndpoint(this WebApplication app)
    {
        app.MapGet("/api/inf-academica/{id:int}", async (int id, IInfAcademicaService service) =>
        {
            var tipo = await service.GetByIdAsync(id);
            return tipo is null
                ? ApiResults.Fail($"El tipo de información académica con Id {id} no existe.", StatusCodes.Status404NotFound)
                : ApiResults.Ok(tipo);
        })
        .WithName("GetInfAcademicaById")
        .WithTags("InfAcademica")
        .WithSummary("Obtiene un tipo de información académica por Id.")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .RequireAuthorization("AdminPolicy");
    }
}
