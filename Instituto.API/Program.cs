using System.Text.Json;
using Instituto.BR.Interfaces;
using Instituto.BR.Services;
using Instituto.AD.Interfaces;
using Instituto.AD.Repositories;
using Instituto.AD;
using Instituto.AD.Models;
using Instituto.API.Models;
using Instituto.BR.DTOs;

var builder = WebApplication.CreateBuilder(args);

// ── CORS para el front-end Vue ───────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("VueCors", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

// ── JSON Options ──────────────────────────────────────────────────────────────
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
    options.SerializerOptions.WriteIndented = false;
});

// ── Repositories (AD Layer) ─────────────────────────────────────────────────
var connectionString = builder.Configuration.GetConnectionString("SqlServer")
    ?? throw new InvalidOperationException("ConnectionStrings:SqlServer no configurado.");

builder.Services.AddScoped<ICarreraRepository>(_ => new CarreraRepository(connectionString));
builder.Services.AddScoped<IAlumnoRepository>(_ => new AlumnoRepository(connectionString));
builder.Services.AddScoped<IAdministradorRepository>(_ => new AdministradorRepository(connectionString));
builder.Services.AddScoped<IProfesorRepository>(_ => new ProfesorRepository(connectionString));
builder.Services.AddScoped<IFormularioRepository>(_ => new FormularioRepository(connectionString));
builder.Services.AddScoped<IListadoRepository>(_ => new ListadoRepository(connectionString));

// ── Services (BR Layer) ─────────────────────────────────────────────────────
builder.Services.AddScoped<ICarreraService, CarreraService>();
builder.Services.AddScoped<IAlumnoService, AlumnoService>();
builder.Services.AddScoped<IAdministradorService, AdministradorService>();
builder.Services.AddScoped<IProfesorService, ProfesorService>();
builder.Services.AddScoped<IFormularioService, FormularioService>();
builder.Services.AddScoped<IListadoService, ListadoService>();

// ── Build ────────────────────────────────────────────────────────────────────
var app = builder.Build();

// ── Global Exception Handler ────────────────────────────────────────────────
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        var body = JsonSerializer.Serialize(new { error = "Ocurrió un error interno del servidor." });
        await context.Response.WriteAsync(body);
    });
});

// ── Middleware Pipeline ─────────────────────────────────────────────────────
app.UseRouting();
app.UseCors("VueCors");

// ── Health check ────────────────────────────────────────────────────────────
app.MapGet("/", () => "Backend corriendo correctamente!");

// ============================================================================
// MINIMAL API ENDPOINTS
// ============================================================================

// ============================================================================
// AUTH ENDPOINTS
// ============================================================================
var authGroup = app.MapGroup("/api/auth").WithTags("Auth");

authGroup.MapPost("/login", async (LoginRequest dto, IAdministradorService authService) =>
{
    if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
        return Results.BadRequest(new { isSuccess = false, message = "Email y contraseña requeridos" });

    var admin = await Task.FromResult(authService.Login(dto.Email, dto.Password));

    if (admin is null)
        return Results.Unauthorized();

    var sessionToken = Guid.NewGuid().ToString("N");
    var expira = DateTime.UtcNow.AddHours(8);

    return Results.Ok(new
    {
        isSuccess = true,
        message = "Operación exitosa",
        data = new
        {
            Token = sessionToken,
            ExpiraEn = expira,
            Admin = admin
        }
    });
}).WithName("Login").AllowAnonymous();

authGroup.MapPost("/verify-password", async (VerifyPasswordRequest dto, IAdministradorService authService) =>
{
    if (string.IsNullOrWhiteSpace(dto.Email))
        return Results.BadRequest(new { isSuccess = false, message = "Email requerido para verificar contraseña" });

    var admin = await Task.FromResult(authService.Login(dto.Email, dto.Password));

    if (admin is null)
        return Results.Unauthorized();

    return Results.Ok(new { isSuccess = true, message = "Contraseña verificada correctamente" });
}).WithName("VerifyPassword");

// ============================================================================
// SETUP ENDPOINT
// ============================================================================
app.MapPost("/api/setup/admin", async (SetupAdminDto dto, IServiceProvider services) =>
{
    if (!app.Environment.IsDevelopment())
        return Results.NotFound();

    var authService = services.GetRequiredService<IAdministradorService>();

    if (authService.HayAdmins())
        return Results.Conflict(new { isSuccess = false, message = "Ya existe al menos un administrador. Este endpoint está deshabilitado." });

    var admin = await authService.CrearPrimerAdminAsync(dto);

    if (admin is null)
        return Results.Json(new { isSuccess = false, message = "Error al crear el administrador inicial." }, statusCode: 500);

    return Results.Ok(new { isSuccess = true, message = $"Administrador '{admin.Email}' creado correctamente. Este endpoint ya no puede volver a usarse." });
}).WithName("CrearPrimerAdmin").AllowAnonymous();

// ============================================================================
// ADMINISTRADORES
// ============================================================================
var adminGroup = app.MapGroup("/api/administradores").WithTags("Administradores");

adminGroup.MapGet("/", async (IAdministradorService service) =>
{
    var admins = await Task.FromResult(service.GetAll());
    return Results.Ok(new { isSuccess = true, message = "Operación exitosa", data = admins });
}).WithName("GetAllAdministradores");

adminGroup.MapGet("/{id:int}", async (int id, IAdministradorService service) =>
{
    var admin = await Task.FromResult(service.GetById(id));
    return admin is null
        ? Results.NotFound(new { isSuccess = false, message = $"Administrador con Id {id} no fue encontrado." })
        : Results.Ok(new { isSuccess = true, message = "Operación exitosa", data = admin });
}).WithName("GetAdministradorById");

adminGroup.MapPost("/with-password", async (AdminCreateDto dto, IAdministradorService service) =>
{
    var admin = new Administrador
    {
        Nombre = dto.Nombre,
        Apellido = dto.Apellido,
        Email = dto.Email,
        Role = dto.Role
    };

    var result = await Task.FromResult(service.Create(admin, dto.Password));
    if (!result.Success)
        return Results.BadRequest(new { isSuccess = false, message = result.Message });

    return Results.CreatedAtRoute("GetAdministradorById", new { id = result.Data!.Id }, new { isSuccess = true, message = "Operación exitosa", data = result.Data });
}).WithName("CreateAdministradorWithPassword").Accepts<AdminCreateDto>("application/json");

adminGroup.MapPut("/{id:int}", async (int id, Administrador admin, IAdministradorService service) =>
{
    var result = await Task.FromResult(service.Update(id, admin));
    if (!result.Success)
        return Results.NotFound(new { isSuccess = false, message = result.Message });

    return Results.Ok(new { isSuccess = true, message = "Operación exitosa", data = result.Data });
}).WithName("UpdateAdministrador");

adminGroup.MapPut("/{id:int}/password", async (int id, ChangePasswordRequest dto, IAdministradorService service) =>
{
    var result = await Task.FromResult(service.ChangePassword(id, dto.PasswordActual, dto.NuevaPassword));
    if (!result.Success)
        return Results.Unauthorized();

    return Results.Ok(new { isSuccess = true, message = result.Message });
}).WithName("ChangeAdministradorPassword");

adminGroup.MapDelete("/{id:int}", async (int id, IAdministradorService service) =>
{
    var result = await Task.FromResult(service.Delete(id));
    if (!result.Success)
        return Results.NotFound(new { isSuccess = false, message = result.Message });

    return Results.NoContent();
}).WithName("DeleteAdministrador");

// ============================================================================
// ALUMNOS
// ============================================================================
var alumnoGroup = app.MapGroup("/api/alumnos").WithTags("Alumnos");

alumnoGroup.MapGet("/", async (IAlumnoService service) =>
{
    var alumnos = await Task.FromResult(service.GetAll());
    return Results.Ok(new { isSuccess = true, message = "Operación exitosa", data = alumnos });
}).WithName("GetAllAlumnos");

alumnoGroup.MapGet("/{id:int}", async (int id, IAlumnoService service) =>
{
    var alumno = await Task.FromResult(service.GetById(id));
    return alumno is null
        ? Results.NotFound(new { isSuccess = false, message = $"Alumno con Id {id} no fue encontrado." })
        : Results.Ok(new { isSuccess = true, message = "Operación exitosa", data = alumno });
}).WithName("GetAlumnoById");

alumnoGroup.MapPost("/", async (Alumno alumno, IAlumnoService service) =>
{
    var result = await Task.FromResult(service.Create(alumno));
    if (!result.Success)
        return Results.BadRequest(new { isSuccess = false, message = result.Message });

    return Results.CreatedAtRoute("GetAlumnoById", new { id = result.Data!.Id }, new { isSuccess = true, message = "Operación exitosa", data = result.Data });
}).WithName("CreateAlumno").Accepts<Alumno>("application/json");

alumnoGroup.MapPut("/{id:int}", async (int id, Alumno alumno, IAlumnoService service) =>
{
    var result = await Task.FromResult(service.Update(id, alumno));
    if (!result.Success)
        return Results.NotFound(new { isSuccess = false, message = result.Message });

    return Results.Ok(new { isSuccess = true, message = "Operación exitosa", data = result.Data });
}).WithName("UpdateAlumno");

alumnoGroup.MapDelete("/{id:int}", async (int id, IAlumnoService service) =>
{
    var result = await Task.FromResult(service.Delete(id));
    if (!result.Success)
        return Results.NotFound(new { isSuccess = false, message = result.Message });

    return Results.NoContent();
}).WithName("DeleteAlumno");

// ============================================================================
// CARRERAS
// ============================================================================
var carreraGroup = app.MapGroup("/api/carreras").WithTags("Carreras");

carreraGroup.MapGet("/", async (ICarreraService service) =>
{
    var carreras = await Task.FromResult(service.GetAll());
    return Results.Ok(new { isSuccess = true, message = "Operación exitosa", data = carreras });
}).WithName("GetAllCarreras");

carreraGroup.MapGet("/{id:int}", async (int id, ICarreraService service) =>
{
    var carrera = await Task.FromResult(service.GetById(id));
    return carrera is null
        ? Results.NotFound(new { isSuccess = false, message = $"Carrera con Id {id} no fue encontrada." })
        : Results.Ok(new { isSuccess = true, message = "Operación exitosa", data = carrera });
}).WithName("GetCarreraById");

carreraGroup.MapPost("/", async (Carrera carrera, ICarreraService service) =>
{
    var result = await Task.FromResult(service.Create(carrera));
    if (!result.Success)
        return Results.BadRequest(new { isSuccess = false, message = result.Message });

    return Results.CreatedAtRoute("GetCarreraById", new { id = result.Data!.Id }, new { isSuccess = true, message = "Operación exitosa", data = result.Data });
}).WithName("CreateCarrera").Accepts<Carrera>("application/json");

carreraGroup.MapPut("/{id:int}", async (int id, Carrera carrera, ICarreraService service) =>
{
    var result = await Task.FromResult(service.Update(id, carrera));
    if (!result.Success)
        return Results.NotFound(new { isSuccess = false, message = result.Message });

    return Results.Ok(new { isSuccess = true, message = "Operación exitosa", data = result.Data });
}).WithName("UpdateCarrera");

carreraGroup.MapDelete("/{id:int}", async (int id, ICarreraService service) =>
{
    var result = await Task.FromResult(service.Delete(id));
    if (!result.Success)
        return Results.NotFound(new { isSuccess = false, message = result.Message });

    return Results.NoContent();
}).WithName("DeleteCarrera");

// ============================================================================
// PROFESORES
// ============================================================================
var profesorGroup = app.MapGroup("/api/profesores").WithTags("Profesores");

profesorGroup.MapGet("/", async (IProfesorService service) =>
{
    var profesores = await Task.FromResult(service.GetAll());
    return Results.Ok(new { isSuccess = true, message = "Operación exitosa", data = profesores });
}).WithName("GetAllProfesores");

profesorGroup.MapGet("/{id:int}", async (int id, IProfesorService service) =>
{
    var profesor = await Task.FromResult(service.GetById(id));
    return profesor is null
        ? Results.NotFound(new { isSuccess = false, message = $"Profesor con Id {id} no fue encontrado." })
        : Results.Ok(new { isSuccess = true, message = "Operación exitosa", data = profesor });
}).WithName("GetProfesorById");

profesorGroup.MapPost("/", async (Profesor profesor, IProfesorService service) =>
{
    var result = await Task.FromResult(service.Create(profesor));
    if (!result.Success)
        return Results.BadRequest(new { isSuccess = false, message = result.Message });

    return Results.CreatedAtRoute("GetProfesorById", new { id = result.Data!.Id }, new { isSuccess = true, message = "Operación exitosa", data = result.Data });
}).WithName("CreateProfesor").Accepts<Profesor>("application/json");

profesorGroup.MapPut("/{id:int}", async (int id, Profesor profesor, IProfesorService service) =>
{
    var result = await Task.FromResult(service.Update(id, profesor));
    if (!result.Success)
        return Results.NotFound(new { isSuccess = false, message = result.Message });

    return Results.Ok(new { isSuccess = true, message = "Operación exitosa", data = result.Data });
}).WithName("UpdateProfesor");

profesorGroup.MapDelete("/{id:int}", async (int id, IProfesorService service) =>
{
    var result = await Task.FromResult(service.Delete(id));
    if (!result.Success)
        return Results.NotFound(new { isSuccess = false, message = result.Message });

    return Results.NoContent();
}).WithName("DeleteProfesor");

// ============================================================================
// FORMULARIOS
// ============================================================================
var formularioGroup = app.MapGroup("/api/formularios").WithTags("Formularios");

formularioGroup.MapGet("/", async (IFormularioService service) =>
{
    var formularios = await Task.FromResult(service.GetAll());
    return Results.Ok(new { isSuccess = true, message = "Operación exitosa", data = formularios });
}).WithName("GetAllFormularios");

formularioGroup.MapGet("/{id:int}", async (int id, IFormularioService service) =>
{
    var formulario = await Task.FromResult(service.GetById(id));
    return formulario is null
        ? Results.NotFound(new { isSuccess = false, message = $"Formulario con Id {id} no fue encontrado." })
        : Results.Ok(new { isSuccess = true, message = "Operación exitosa", data = formulario });
}).WithName("GetFormularioById");

formularioGroup.MapPost("/", async (Formulario formulario, IFormularioService service) =>
{
    var result = await Task.FromResult(service.Create(formulario));
    if (!result.Success)
        return Results.BadRequest(new { isSuccess = false, message = result.Message });

    return Results.CreatedAtRoute("GetFormularioById", new { id = result.Data!.Id }, new { isSuccess = true, message = "Operación exitosa", data = result.Data });
}).WithName("CreateFormulario").Accepts<Formulario>("application/json");

formularioGroup.MapPut("/{id:int}", async (int id, Formulario formulario, IFormularioService service) =>
{
    var result = await Task.FromResult(service.Update(id, formulario));
    if (!result.Success)
        return Results.NotFound(new { isSuccess = false, message = result.Message });

    return Results.Ok(new { isSuccess = true, message = "Operación exitosa", data = result.Data });
}).WithName("UpdateFormulario");

formularioGroup.MapDelete("/{id:int}", async (int id, IFormularioService service) =>
{
    var result = await Task.FromResult(service.Delete(id));
    if (!result.Success)
        return Results.NotFound(new { isSuccess = false, message = result.Message });

    return Results.NoContent();
}).WithName("DeleteFormulario");

// ============================================================================
// LISTADOS
// ============================================================================
var listadoGroup = app.MapGroup("/api/listado").WithTags("Listados");

listadoGroup.MapGet("/", async (IListadoService service) =>
{
    var listado = await Task.FromResult(service.GetListado());
    return Results.Ok(new { isSuccess = true, message = "Operación exitosa", data = listado });
}).WithName("GetListado");

// Run
app.Run();

// Make Program accessible for integration tests
public partial class Program { }