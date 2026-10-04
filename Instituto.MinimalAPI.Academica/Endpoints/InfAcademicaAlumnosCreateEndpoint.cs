using Instituto.AD.Models;
using Instituto.BR.Interfaces;

namespace Instituto.MinimalAPI.Academica.Endpoints;

public static class InfAcademicaAlumnosCreateEndpoint
{
    public static void MapInfAcademicaAlumnosCreateEndpoint(this WebApplication app)
    {
        app.MapPost("/api/inf-academica-alumnos", async (InfAcademicaEst entity, IInfAcademicaEstService service) =>
        {
            var resultado = await service.CreateAsync(entity);
            return resultado.Success
                ? ApiResults.Created(resultado.Data, resultado.Message ?? "Operación exitosa")
                : ApiResults.Fail(resultado.Message ?? "No se pudo crear la información académica.");
        })
        .WithName("CreateInfAcademicaAlumno")
        .WithTags("InfAcademicaAlumnos")
        .WithSummary("Crea un registro de información académica para un alumno existente.")
        .Produces(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .RequireAuthorization("AdminPolicy");
    }
}
