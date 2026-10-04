using Instituto.BR.Interfaces;

namespace Instituto.MinimalAPI.Academica.Endpoints;

public static class ListadoEndpoint
{
    public static void MapListadoEndpoint(this WebApplication app)
    {
        app.MapGet("/api/listado", async (IListadoService service) =>
        {
            var listado = await service.GetListadoAsync();
            return ApiResults.Ok(listado);
        })
        .WithName("GetListado")
        .WithTags("Listado")
        .WithSummary("Lista los alumnos con su información académica, carrera y edad.")
        .Produces(StatusCodes.Status200OK)
        .AllowAnonymous();
    }
}
