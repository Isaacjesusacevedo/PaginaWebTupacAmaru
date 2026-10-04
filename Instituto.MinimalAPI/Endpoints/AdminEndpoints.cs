using Instituto.AD.Models;
using Instituto.BR.DTOs;
using Instituto.BR.Interfaces;
using Instituto.MinimalAPI.Models;

namespace Instituto.MinimalAPI.Endpoints;

public static class AdminEndpoints
{
    public static void MapAdminEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/administradores").WithTags("Administradores");

        group.MapGet("/", async (IAdministradorService service) =>
            Results.Ok(new { isSuccess = true, message = "Operación exitosa", data = await service.GetAllAsync() }))
            .WithName("GetAllAdministradores");

        group.MapGet("/{id:int}", async (int id, IAdministradorService service) =>
        {
            var admin = await service.GetByIdAsync(id);
            return admin is null
                ? Results.NotFound(new { isSuccess = false, message = $"Administrador con Id {id} no fue encontrado." })
                : Results.Ok(new { isSuccess = true, message = "Operación exitosa", data = admin });
        }).WithName("GetAdministradorById");

        group.MapPost("/with-password", async (AdminCreateDto dto, IAdministradorService service) =>
        {
            var admin = new Administrador
            {
                Nombre = dto.Nombre,
                Apellido = dto.Apellido,
                Email = dto.Email,
                Role = dto.Role
            };

            var result = await service.CreateAsync(admin, dto.Password);
            return !result.Success
                ? Results.BadRequest(new { isSuccess = false, message = result.Message })
                : Results.CreatedAtRoute("GetAdministradorById", new { id = result.Data!.Id },
                    new { isSuccess = true, message = "Operación exitosa", data = result.Data });
        }).WithName("CreateAdministradorWithPassword").Accepts<AdminCreateDto>("application/json");

        group.MapPut("/{id:int}", async (int id, Administrador admin, IAdministradorService service) =>
        {
            var result = await service.UpdateAsync(id, admin);
            return !result.Success
                ? Results.NotFound(new { isSuccess = false, message = result.Message })
                : Results.Ok(new { isSuccess = true, message = "Operación exitosa", data = result.Data });
        }).WithName("UpdateAdministrador");

        group.MapPut("/{id:int}/password", async (int id, ChangePasswordRequest dto, IAdministradorService service) =>
        {
            var result = await service.ChangePasswordAsync(id, dto.PasswordActual, dto.NuevaPassword);
            return !result.Success ? Results.Unauthorized() : Results.Ok(new { isSuccess = true, message = result.Message });
        }).WithName("ChangeAdministradorPassword");

        group.MapDelete("/{id:int}", async (int id, IAdministradorService service) =>
        {
            var result = await service.DeleteAsync(id);
            return !result.Success ? Results.NotFound(new { isSuccess = false, message = result.Message }) : Results.NoContent();
        }).WithName("DeleteAdministrador");
    }
}