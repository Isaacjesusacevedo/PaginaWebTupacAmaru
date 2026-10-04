using Instituto.BR.Interfaces;

namespace Instituto.MinimalAPI.Academica.Endpoints;

public static class InfAcademicaAlumnosListEndpoint
{
    public static void MapInfAcademicaAlumnosListEndpoint(this WebApplication app)
    {
        app.MapGet("/api/inf-academica-alumnos", async (int? alumnoId, IInfAcademicaEstService service) =>
        {
            var registros = alumnoId.HasValue
                ? await service.GetByAlumnoAsync(alumnoId.Value)
                : await service.GetAllAsync();
            return ApiResults.Ok(registros);
        })
        .WithName("GetInfAcademicaAlumnos")
        .WithTags("InfAcademicaAlumnos")
        .WithSummary("Lista la información académica de los alumnos, opcionalmente filtrada por alumnoId.")
        .Produces(StatusCodes.Status200OK)
        .RequireAuthorization("AdminPolicy");
    }
}
