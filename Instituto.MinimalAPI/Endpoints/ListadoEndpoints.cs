using Instituto.BR.Interfaces;

namespace Instituto.MinimalAPI.Endpoints;

public static class ListadoEndpoints
{
    public static void MapListadoEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/listado").WithTags("Listados");

        group.MapGet("/", async (IListadoService service) =>
            Results.Ok(new { isSuccess = true, message = "Operación exitosa", data = await service.GetListadoAsync() }))
            .WithName("GetListado");
    }
}