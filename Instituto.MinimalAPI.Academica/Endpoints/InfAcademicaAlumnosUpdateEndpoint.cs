using Instituto.AD.Models;
using Instituto.BR.Interfaces;

namespace Instituto.MinimalAPI.Academica.Endpoints;

public static class InfAcademicaAlumnosUpdateEndpoint
{
    public static void MapInfAcademicaAlumnosUpdateEndpoint(this WebApplication app)
    {
        app.MapPut("/api/inf-academica-alumnos/{id:int}", async (int id, InfAcademicaEst entity, IInfAcademicaEstService service) =>
        {
            var resultado = await service.UpdateAsync(id, entity);
            return resultado.Success
                ? ApiResults.Ok(resultado.Data, resultado.Message ?? "Operación exitosa")
                : ApiResults.Fail(resultado.Message ?? "No se pudo actualizar la información académica.");
        })
        .WithName("UpdateInfAcademicaAlumno")
        .WithTags("InfAcademicaAlumnos")
        .WithSummary("Actualiza un registro de información académica. El alumno asociado no cambia.")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .RequireAuthorization("AdminPolicy");
    }
}
