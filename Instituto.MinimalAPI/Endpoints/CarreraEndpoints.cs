using Instituto.AD.Models;
using Instituto.BR.Interfaces;

namespace Instituto.MinimalAPI.Endpoints;

public static class CarreraEndpoints
{
    public static void MapCarreraEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/carreras").WithTags("Carreras");

        group.MapGet("/", async (ICarreraService service) =>
            Results.Ok(new { isSuccess = true, message = "Operación exitosa", data = await service.GetAllAsync() }))
            .WithName("GetAllCarreras");

        group.MapGet("/{id:int}", async (int id, ICarreraService service) =>
        {
            var carrera = await service.GetByIdAsync(id);
            return carrera is null
                ? Results.NotFound(new { isSuccess = false, message = $"Carrera con Id {id} no fue encontrada." })
                : Results.Ok(new { isSuccess = true, message = "Operación exitosa", data = carrera });
        }).WithName("GetCarreraById");

        group.MapPost("/", async (Carrera carrera, ICarreraService service) =>
        {
            var result = await service.CreateAsync(carrera);
            return !result.Success
                ? Results.BadRequest(new { isSuccess = false, message = result.Message })
                : Results.CreatedAtRoute("GetCarreraById", new { id = result.Data!.Id },
                    new { isSuccess = true, message = "Operación exitosa", data = result.Data });
        }).WithName("CreateCarrera").Accepts<Carrera>("application/json");

        group.MapPut("/{id:int}", async (int id, Carrera carrera, ICarreraService service) =>
        {
            var result = await service.UpdateAsync(id, carrera);
            return !result.Success
                ? Results.NotFound(new { isSuccess = false, message = result.Message })
                : Results.Ok(new { isSuccess = true, message = "Operación exitosa", data = result.Data });
        }).WithName("UpdateCarrera");

        group.MapDelete("/{id:int}", async (int id, ICarreraService service) =>
        {
            var result = await service.DeleteAsync(id);
            return !result.Success ? Results.NotFound(new { isSuccess = false, message = result.Message }) : Results.NoContent();
        }).WithName("DeleteCarrera");
    }
}