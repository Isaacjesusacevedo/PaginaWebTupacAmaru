using Instituto.AD.Models;
using Instituto.BR.Interfaces;

namespace Instituto.MinimalAPI.Academica.Endpoints;

public static class InfAcademicaUpdateEndpoint
{
    public static void MapInfAcademicaUpdateEndpoint(this WebApplication app)
    {
        app.MapPut("/api/inf-academica/{id:int}", async (int id, InfAcademica entity, IInfAcademicaService service) =>
        {
            var resultado = await service.UpdateAsync(id, entity);
            return resultado.Success
                ? ApiResults.Ok(resultado.Data, resultado.Message ?? "Operación exitosa")
                : ApiResults.Fail(resultado.Message ?? "No se pudo actualizar el tipo de información académica.");
        })
        .WithName("UpdateInfAcademica")
        .WithTags("InfAcademica")
        .WithSummary("Actualiza un tipo de información académica del catálogo.")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .RequireAuthorization("AdminPolicy");
    }
}
