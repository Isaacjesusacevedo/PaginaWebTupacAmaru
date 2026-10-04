using Instituto.AD.Models;
using Instituto.BR.Interfaces;

namespace Instituto.MinimalAPI.Academica.Endpoints;

public static class InfAcademicaCreateEndpoint
{
    public static void MapInfAcademicaCreateEndpoint(this WebApplication app)
    {
        app.MapPost("/api/inf-academica", async (InfAcademica entity, IInfAcademicaService service) =>
        {
            var resultado = await service.CreateAsync(entity);
            return resultado.Success
                ? ApiResults.Created(resultado.Data, resultado.Message ?? "Operación exitosa")
                : ApiResults.Fail(resultado.Message ?? "No se pudo crear el tipo de información académica.");
        })
        .WithName("CreateInfAcademica")
        .WithTags("InfAcademica")
        .WithSummary("Crea un tipo de información académica en el catálogo.")
        .Produces(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .RequireAuthorization("AdminPolicy");
    }
}
