using Instituto.BR.Interfaces;

namespace Instituto.MinimalAPI.Academica.Endpoints;

public static class InfAcademicaDeleteEndpoint
{
    public static void MapInfAcademicaDeleteEndpoint(this WebApplication app)
    {
        app.MapDelete("/api/inf-academica/{id:int}", async (int id, IInfAcademicaService service) =>
        {
            var resultado = await service.DeleteAsync(id);
            return resultado.Success
                ? Results.NoContent()
                : ApiResults.Fail(resultado.Message ?? "No se pudo eliminar el tipo de información académica.");
        })
        .WithName("DeleteInfAcademica")
        .WithTags("InfAcademica")
        .WithSummary("Elimina un tipo de información académica del catálogo.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .RequireAuthorization("AdminPolicy");
    }
}
