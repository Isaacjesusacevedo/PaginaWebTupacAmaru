using Instituto.AD.Models;
using Instituto.BR.Interfaces;

namespace Instituto.MinimalAPI.Endpoints;

public static class FormularioEndpoints
{
    public static void MapFormularioEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/formularios").WithTags("Formularios");

        group.MapGet("/", async (IFormularioService service) =>
            Results.Ok(new { isSuccess = true, message = "Operación exitosa", data = await service.GetAllAsync() }))
            .WithName("GetAllFormularios");

        group.MapGet("/{id:int}", async (int id, IFormularioService service) =>
        {
            var formulario = await service.GetByIdAsync(id);
            return formulario is null
                ? Results.NotFound(new { isSuccess = false, message = $"Formulario con Id {id} no fue encontrado." })
                : Results.Ok(new { isSuccess = true, message = "Operación exitosa", data = formulario });
        }).WithName("GetFormularioById");

        group.MapPost("/", async (Formulario formulario, IFormularioService service) =>
        {
            var result = await service.CreateAsync(formulario);
            return !result.Success
                ? Results.BadRequest(new { isSuccess = false, message = result.Message })
                : Results.CreatedAtRoute("GetFormularioById", new { id = result.Data!.Id },
                    new { isSuccess = true, message = "Operación exitosa", data = result.Data });
        }).WithName("CreateFormulario").Accepts<Formulario>("application/json");

        group.MapPut("/{id:int}", async (int id, Formulario formulario, IFormularioService service) =>
        {
            var result = await service.UpdateAsync(id, formulario);
            return !result.Success
                ? Results.NotFound(new { isSuccess = false, message = result.Message })
                : Results.Ok(new { isSuccess = true, message = "Operación exitosa", data = result.Data });
        }).WithName("UpdateFormulario");

        group.MapDelete("/{id:int}", async (int id, IFormularioService service) =>
        {
            var result = await service.DeleteAsync(id);
            return !result.Success ? Results.NotFound(new { isSuccess = false, message = result.Message }) : Results.NoContent();
        }).WithName("DeleteFormulario");
    }
}