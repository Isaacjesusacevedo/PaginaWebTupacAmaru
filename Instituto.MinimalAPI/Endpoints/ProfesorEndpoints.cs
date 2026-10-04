using Instituto.AD.Models;
using Instituto.BR.Interfaces;

namespace Instituto.MinimalAPI.Endpoints;

public static class ProfesorEndpoints
{
    public static void MapProfesorEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/profesores").WithTags("Profesores");

        group.MapGet("/", async (IProfesorService service) =>
            Results.Ok(new { isSuccess = true, message = "Operación exitosa", data = await service.GetAllAsync() }))
            .WithName("GetAllProfesores");

        group.MapGet("/{id:int}", async (int id, IProfesorService service) =>
        {
            var profesor = await service.GetByIdAsync(id);
            return profesor is null
                ? Results.NotFound(new { isSuccess = false, message = $"Profesor con Id {id} no fue encontrado." })
                : Results.Ok(new { isSuccess = true, message = "Operación exitosa", data = profesor });
        }).WithName("GetProfesorById");

        group.MapPost("/", async (Profesor profesor, IProfesorService service) =>
        {
            var result = await service.CreateAsync(profesor);
            return !result.Success
                ? Results.BadRequest(new { isSuccess = false, message = result.Message })
                : Results.CreatedAtRoute("GetProfesorById", new { id = result.Data!.Id },
                    new { isSuccess = true, message = "Operación exitosa", data = result.Data });
        }).WithName("CreateProfesor").Accepts<Profesor>("application/json");

        group.MapPut("/{id:int}", async (int id, Profesor profesor, IProfesorService service) =>
        {
            var result = await service.UpdateAsync(id, profesor);
            return !result.Success
                ? Results.NotFound(new { isSuccess = false, message = result.Message })
                : Results.Ok(new { isSuccess = true, message = "Operación exitosa", data = result.Data });
        }).WithName("UpdateProfesor");

        group.MapDelete("/{id:int}", async (int id, IProfesorService service) =>
        {
            var result = await service.DeleteAsync(id);
            return !result.Success ? Results.NotFound(new { isSuccess = false, message = result.Message }) : Results.NoContent();
        }).WithName("DeleteProfesor");
    }
}