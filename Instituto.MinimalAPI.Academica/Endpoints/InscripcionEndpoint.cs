using Instituto.BR.DTOs;
using Instituto.BR.Interfaces;

namespace Instituto.MinimalAPI.Academica.Endpoints;

public static class InscripcionEndpoint
{
    public static void MapInscripcionEndpoint(this WebApplication app)
    {
        app.MapPost("/api/inscripcion", async (InscripcionDto dto, IInscripcionService service) =>
        {
            var resultado = await service.InscribirAsync(dto);
            return resultado.Success
                ? ApiResults.Created(resultado.Data, resultado.Message ?? "Operación exitosa")
                : ApiResults.Fail(resultado.Message ?? "No se pudo completar la inscripción.");
        })
        .WithName("Inscribir")
        .WithTags("Inscripcion")
        .WithSummary("Inscribe un nuevo alumno junto con su información académica.")
        .Produces(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .AllowAnonymous();
    }
}
