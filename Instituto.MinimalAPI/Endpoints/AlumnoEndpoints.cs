using Instituto.AD.Models;
using Instituto.BR.Interfaces;

namespace Instituto.MinimalAPI.Endpoints;

public static class AlumnoEndpoints
{
    public static void MapAlumnoEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/alumnos").WithTags("Alumnos");

        group.MapGet("/", async (IAlumnoService service) =>
            Results.Ok(new { isSuccess = true, message = "Operación exitosa", data = await service.GetAllAsync() }))
            .WithName("GetAllAlumnos");

        group.MapGet("/{id:int}", async (int id, IAlumnoService service) =>
        {
            var alumno = await service.GetByIdAsync(id);
            return alumno is null
                ? Results.NotFound(new { isSuccess = false, message = $"Alumno con Id {id} no fue encontrado." })
                : Results.Ok(new { isSuccess = true, message = "Operación exitosa", data = alumno });
        }).WithName("GetAlumnoById");

        group.MapPost("/", async (Alumno alumno, IAlumnoService service) =>
        {
            var result = await service.CreateAsync(alumno);
            return !result.Success
                ? Results.BadRequest(new { isSuccess = false, message = result.Message })
                : Results.CreatedAtRoute("GetAlumnoById", new { id = result.Data!.Id },
                    new { isSuccess = true, message = "Operación exitosa", data = result.Data });
        }).WithName("CreateAlumno").Accepts<Alumno>("application/json");

        group.MapPut("/{id:int}", async (int id, Alumno alumno, IAlumnoService service) =>
        {
            var result = await service.UpdateAsync(id, alumno);
            return !result.Success
                ? Results.NotFound(new { isSuccess = false, message = result.Message })
                : Results.Ok(new { isSuccess = true, message = "Operación exitosa", data = result.Data });
        }).WithName("UpdateAlumno");

        group.MapDelete("/{id:int}", async (int id, IAlumnoService service) =>
        {
            var result = await service.DeleteAsync(id);
            return !result.Success ? Results.NotFound(new { isSuccess = false, message = result.Message }) : Results.NoContent();
        }).WithName("DeleteAlumno");
    }
}