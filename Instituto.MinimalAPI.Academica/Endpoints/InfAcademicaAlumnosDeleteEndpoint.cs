using Instituto.BR.Interfaces;

namespace Instituto.MinimalAPI.Academica.Endpoints;

public static class InfAcademicaAlumnosDeleteEndpoint
{
    public static void MapInfAcademicaAlumnosDeleteEndpoint(this WebApplication app)
    {
        app.MapDelete("/api/inf-academica-alumnos/{id:int}", async (int id, IInfAcademicaEstService service) =>
        {
            var resultado = await service.DeleteAsync(id);
            return resultado.Success
                ? Results.NoContent()
                : ApiResults.Fail(resultado.Message ?? "No se pudo eliminar la información académica.");
        })
        .WithName("DeleteInfAcademicaAlumno")
        .WithTags("InfAcademicaAlumnos")
        .WithSummary("Elimina un registro de información académica de un alumno.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .RequireAuthorization("AdminPolicy");
    }
}
