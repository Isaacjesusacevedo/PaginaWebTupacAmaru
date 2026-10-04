using System.Text.Json;
using Instituto.BR.Interfaces;
using Instituto.BR.Services;
using Instituto.AD.Interfaces;
using Instituto.AD.Repositories;
using Instituto.AD;
using Instituto.AD.Models;
using Instituto.MinimalAPI.Models;
using Instituto.BR.DTOs;
using Microsoft.EntityFrameworkCore;
using Instituto.AD.Data;

var builder = WebApplication.CreateBuilder(args);

// ═══════════════════════════════════════════════════════════════════════
// CONFIGURATION
// ═══════════════════════════════════════════════════════════════════════

var envConn = Environment.GetEnvironmentVariable("CONNECTION_STRING_SQLSERVER");

Console.WriteLine("═══════════════════════════════════════════════");
Console.WriteLine("[CONFIG] ENV VAR 'CONNECTION_STRING_SQLSERVER' presente: " + (!string.IsNullOrEmpty(envConn)));
Console.WriteLine("[CONFIG] ASPNETCORE_ENVIRONMENT: " + Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));

if (!string.IsNullOrWhiteSpace(envConn))
{
    Console.WriteLine("[CONFIG] Longitud: " + envConn.Length);
    Console.WriteLine("[CONFIG] Primeros 50 chars: " + envConn.Substring(0, Math.Min(50, envConn.Length)));
    builder.Configuration["ConnectionStrings:SqlServer"] = envConn;
    Console.WriteLine("[CONFIG] ✅ Usando connection string de ENV VAR");
}
else
{
    Console.WriteLine("[CONFIG] ❌ ENV VAR vacía, usando appsettings.json");
}

var connectionString = builder.Configuration.GetConnectionString("SqlServer")
    ?? throw new InvalidOperationException("ConnectionStrings:SqlServer no configurado.");

Console.WriteLine("[CONFIG] Connection string FINAL (primeros 60 chars): " +
    connectionString.Substring(0, Math.Min(60, connectionString.Length)));
Console.WriteLine("═══════════════════════════════════════════════");

// ── CORS ────────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("VueCors", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

// ── JSON Options ────────────────────────────────────────────────────────────────
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
    options.SerializerOptions.WriteIndented = false;
});

// ── EF Core ─────────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<InstitutoDbContext>(options =>
    options.UseSqlServer(connectionString));

// ── Repositories (AD Layer) ────────────────────────────────────────────────────
builder.Services.AddScoped<ICarreraRepository, CarreraRepository>();
builder.Services.AddScoped<IAlumnoRepository, AlumnoRepository>();
builder.Services.AddScoped<IAdministradorRepository, AdministradorRepository>();
builder.Services.AddScoped<IProfesorRepository, ProfesorRepository>();
builder.Services.AddScoped<IFormularioRepository, FormularioRepository>();
builder.Services.AddScoped<IListadoRepository, ListadoRepository>();

// ── Services (BR Layer) ────────────────────────────────────────────────────────
builder.Services.AddScoped<ICarreraService, CarreraService>();
builder.Services.AddScoped<IAlumnoService, AlumnoService>();
builder.Services.AddScoped<IAdministradorService, AdministradorService>();
builder.Services.AddScoped<IProfesorService, ProfesorService>();
builder.Services.AddScoped<IFormularioService, FormularioService>();
builder.Services.AddScoped<IListadoService, ListadoService>();

// ── Swagger/OpenAPI ────────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Instituto API",
        Version = "v1",
        Description = "API del Sistema de Gestión Institucional - Instituto Superior Docente Túpac Amaru"
    });
});

// ── Puerto dinámico (Render) ───────────────────────────────────────────────────
var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// ═══════════════════════════════════════════════════════════════════════
// BUILD & MIDDLEWARE PIPELINE
// ═══════════════════════════════════════════════════════════════════════

var app = builder.Build();

// ── Global Exception Handler ───────────────────────────────────────────────────
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

// ── Middleware Pipeline ────────────────────────────────────────────────────────
app.UseRouting();
app.UseCors("VueCors");

// ── Swagger UI (solo Development) ──────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ═══════════════════════════════════════════════════════════════════════
// ENDPOINTS
// ═══════════════════════════════════════════════════════════════════════

// ── Health check ────────────────────────────────────────────────────────────────
app.MapGet("/", () => "Backend corriendo correctamente!");

app.MapGet("/health", async (InstitutoDbContext db) =>
{
    try
    {
        await db.Database.CanConnectAsync();
        return Results.Ok(new { status = "OK", timestamp = DateTime.UtcNow, database = "connected" });
    }
    catch (Exception ex)
    {
        return Results.Ok(new
        {
            status = "error",
            timestamp = DateTime.UtcNow,
            database = "error",
            detalle = ex.Message
        });
    }
}).WithName("HealthCheck").WithTags("Health");

// ── Debug endpoints (temporales - quitar en producción final) ──────────────────
app.MapGet("/debug-config", () =>
{
    var envConnDebug = Environment.GetEnvironmentVariable("CONNECTION_STRING_SQLSERVER");
    var configConn = builder.Configuration.GetConnectionString("SqlServer");
    var envAsp = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
    var envPort = Environment.GetEnvironmentVariable("PORT");

    return Results.Ok(new
    {
        aspnetcore_environment = envAsp,
        port = envPort,
        envVarPresente = !string.IsNullOrEmpty(envConnDebug),
        envVarLargo = envConnDebug?.Length ?? 0,
        envVarPrimeros60 = envConnDebug?.Substring(0, Math.Min(60, envConnDebug?.Length ?? 0)),
        configConnPrimeros60 = configConn?.Substring(0, Math.Min(60, configConn?.Length ?? 0)),
        todasLasVars = Environment.GetEnvironmentVariables()
            .Keys.Cast<string>()
            .Where(k => k.ToUpper().Contains("CONNECTION")
                     || k.ToUpper().Contains("SQL")
                     || k.ToUpper().Contains("ASPNETCORE"))
            .OrderBy(k => k)
            .ToArray()
    });
});

app.MapGet("/debug-db", async (InstitutoDbContext db) =>
{
    try
    {
        var canConnect = await db.Database.CanConnectAsync();
        return Results.Ok(new { canConnect, timestamp = DateTime.UtcNow });
    }
    catch (Exception ex)
    {
        return Results.Ok(new
        {
            canConnect = false,
            error = ex.Message,
            innerError = ex.InnerException?.Message,
            timestamp = DateTime.UtcNow
        });
    }
});

app.MapGet("/debug-login", async (IAdministradorService authService, IAdministradorRepository repo) =>
{
    var email = "admin@tupac.edu.ar";
    var password = "Tupac123";
    var resultado = new Dictionary<string, object?>();

    try
    {
        var adminEncontrado = await authService.LoginAsync(email, password);
        resultado["loginExitoso"] = adminEncontrado != null;

        var adminRaw = await repo.GetByEmailAsync(email);

        if (adminRaw != null)
        {
            resultado["adminEncontrado"] = true;
            resultado["email"] = adminRaw.Email;
            resultado["activo"] = adminRaw.Activo;
            resultado["hashLargo"] = adminRaw.PasswordHash?.Length ?? 0;
            resultado["hashPrimeros40"] = adminRaw.PasswordHash?.Substring(0, Math.Min(40, adminRaw.PasswordHash.Length));
            resultado["hashUltimos20"] = adminRaw.PasswordHash != null && adminRaw.PasswordHash.Length > 20
                ? adminRaw.PasswordHash.Substring(adminRaw.PasswordHash.Length - 20)
                : null;
            resultado["hashTieneEspacios"] = adminRaw.PasswordHash?.Contains(" ") ?? false;

            try
            {
                var verify = BCrypt.Net.BCrypt.Verify(password, adminRaw.PasswordHash);
                resultado["bcryptVerify"] = verify;
            }
            catch (Exception ex)
            {
                resultado["bcryptVerifyError"] = ex.Message;
            }
        }
        else
        {
            resultado["adminEncontrado"] = false;
        }
    }
    catch (Exception ex)
    {
        resultado["exception"] = ex.Message;
        resultado["innerException"] = ex.InnerException?.Message;
    }

    return Results.Ok(resultado);
});

// ── Auth Endpoints ─────────────────────────────────────────────────────────────
var authGroup = app.MapGroup("/api/auth").WithTags("Auth");

authGroup.MapPost("/login", async (LoginRequest dto, IAdministradorService authService) =>
{
    if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
        return Results.BadRequest(new { isSuccess = false, message = "Email y contraseña requeridos" });

    var admin = await authService.LoginAsync(dto.Email, dto.Password);

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

    var admin = await authService.LoginAsync(dto.Email, dto.Password);

    if (admin is null)
        return Results.Unauthorized();

    return Results.Ok(new { isSuccess = true, message = "Contraseña verificada correctamente" });
}).WithName("VerifyPassword");

// ── Setup Endpoint ─────────────────────────────────────────────────────────────
app.MapPost("/api/setup/admin", async (SetupAdminDto dto, IAdministradorService authService) =>
{
    if (!app.Environment.IsDevelopment())
        return Results.NotFound();

    var hayAdmins = await authService.HayAdminsAsync();
    if (hayAdmins)
        return Results.Conflict(new { isSuccess = false, message = "Ya existe al menos un administrador. Este endpoint está deshabilitado." });

    var admin = await authService.CrearPrimerAdminAsync(dto);

    if (admin is null)
        return Results.Json(new { isSuccess = false, message = "Error al crear el administrador inicial." }, statusCode: 500);

    return Results.Ok(new { isSuccess = true, message = $"Administrador '{admin.Email}' creado correctamente. Este endpoint ya no puede volver a usarse." });
}).WithName("CrearPrimerAdmin").AllowAnonymous();

app.MapGet("/api/setup/status", async (IAdministradorService authService) =>
{
    var hayAdmins = await authService.HayAdminsAsync();
    return Results.Ok(new { isSuccess = true, message = "Operación exitosa", data = new { hasAdmin = hayAdmins } });
}).WithName("SetupStatus").WithTags("Setup");

// ── CRUD Endpoints ─────────────────────────────────────────────────────────────
MapCrudEndpoints(app);

// Run
app.Run();

// ═══════════════════════════════════════════════════════════════════════
// HELPER: CRUD MAPPING
// ═══════════════════════════════════════════════════════════════════════

static void MapCrudEndpoints(WebApplication app)
{
    // Administradores
    var adminGroup = app.MapGroup("/api/administradores").WithTags("Administradores");
    adminGroup.MapGet("/", async (IAdministradorService s) => Results.Ok(new { isSuccess = true, message = "Operación exitosa", data = await s.GetAllAsync() })).WithName("GetAllAdministradores");
    adminGroup.MapGet("/{id:int}", async (int id, IAdministradorService s) =>
    {
        var a = await s.GetByIdAsync(id);
        return a is null ? Results.NotFound(new { isSuccess = false, message = $"Administrador con Id {id} no fue encontrado." }) : Results.Ok(new { isSuccess = true, message = "Operación exitosa", data = a });
    }).WithName("GetAdministradorById");
    adminGroup.MapPost("/with-password", async (AdminCreateDto dto, IAdministradorService s) =>
    {
        var admin = new Administrador { Nombre = dto.Nombre, Apellido = dto.Apellido, Email = dto.Email, Role = dto.Role };
        var r = await s.CreateAsync(admin, dto.Password);
        return !r.Success ? Results.BadRequest(new { isSuccess = false, message = r.Message }) : Results.CreatedAtRoute("GetAdministradorById", new { id = r.Data!.Id }, new { isSuccess = true, message = "Operación exitosa", data = r.Data });
    }).WithName("CreateAdministradorWithPassword").Accepts<AdminCreateDto>("application/json");
    adminGroup.MapPut("/{id:int}", async (int id, Administrador a, IAdministradorService s) =>
    {
        var r = await s.UpdateAsync(id, a);
        return !r.Success ? Results.NotFound(new { isSuccess = false, message = r.Message }) : Results.Ok(new { isSuccess = true, message = "Operación exitosa", data = r.Data });
    }).WithName("UpdateAdministrador");
    adminGroup.MapPut("/{id:int}/password", async (int id, ChangePasswordRequest dto, IAdministradorService s) =>
    {
        var r = await s.ChangePasswordAsync(id, dto.PasswordActual, dto.NuevaPassword);
        return !r.Success ? Results.Unauthorized() : Results.Ok(new { isSuccess = true, message = r.Message });
    }).WithName("ChangeAdministradorPassword");
    adminGroup.MapDelete("/{id:int}", async (int id, IAdministradorService s) =>
    {
        var r = await s.DeleteAsync(id);
        return !r.Success ? Results.NotFound(new { isSuccess = false, message = r.Message }) : Results.NoContent();
    }).WithName("DeleteAdministrador");

    // Alumnos
    var alumnoGroup = app.MapGroup("/api/alumnos").WithTags("Alumnos");
    alumnoGroup.MapGet("/", async (IAlumnoService s) => Results.Ok(new { isSuccess = true, message = "Operación exitosa", data = await s.GetAllAsync() })).WithName("GetAllAlumnos");
    alumnoGroup.MapGet("/{id:int}", async (int id, IAlumnoService s) =>
    {
        var a = await s.GetByIdAsync(id);
        return a is null ? Results.NotFound(new { isSuccess = false, message = $"Alumno con Id {id} no fue encontrado." }) : Results.Ok(new { isSuccess = true, message = "Operación exitosa", data = a });
    }).WithName("GetAlumnoById");
    alumnoGroup.MapPost("/", async (Alumno a, IAlumnoService s) =>
    {
        var r = await s.CreateAsync(a);
        return !r.Success ? Results.BadRequest(new { isSuccess = false, message = r.Message }) : Results.CreatedAtRoute("GetAlumnoById", new { id = r.Data!.Id }, new { isSuccess = true, message = "Operación exitosa", data = r.Data });
    }).WithName("CreateAlumno").Accepts<Alumno>("application/json");
    alumnoGroup.MapPut("/{id:int}", async (int id, Alumno a, IAlumnoService s) =>
    {
        var r = await s.UpdateAsync(id, a);
        return !r.Success ? Results.NotFound(new { isSuccess = false, message = r.Message }) : Results.Ok(new { isSuccess = true, message = "Operación exitosa", data = r.Data });
    }).WithName("UpdateAlumno");
    alumnoGroup.MapDelete("/{id:int}", async (int id, IAlumnoService s) =>
    {
        var r = await s.DeleteAsync(id);
        return !r.Success ? Results.NotFound(new { isSuccess = false, message = r.Message }) : Results.NoContent();
    }).WithName("DeleteAlumno");

    // Carreras
    var carreraGroup = app.MapGroup("/api/carreras").WithTags("Carreras");
    carreraGroup.MapGet("/", async (ICarreraService s) => Results.Ok(new { isSuccess = true, message = "Operación exitosa", data = await s.GetAllAsync() })).WithName("GetAllCarreras");
    carreraGroup.MapGet("/{id:int}", async (int id, ICarreraService s) =>
    {
        var c = await s.GetByIdAsync(id);
        return c is null ? Results.NotFound(new { isSuccess = false, message = $"Carrera con Id {id} no fue encontrada." }) : Results.Ok(new { isSuccess = true, message = "Operación exitosa", data = c });
    }).WithName("GetCarreraById");
    carreraGroup.MapPost("/", async (Carrera c, ICarreraService s) =>
    {
        var r = await s.CreateAsync(c);
        return !r.Success ? Results.BadRequest(new { isSuccess = false, message = r.Message }) : Results.CreatedAtRoute("GetCarreraById", new { id = r.Data!.Id }, new { isSuccess = true, message = "Operación exitosa", data = r.Data });
    }).WithName("CreateCarrera").Accepts<Carrera>("application/json");
    carreraGroup.MapPut("/{id:int}", async (int id, Carrera c, ICarreraService s) =>
    {
        var r = await s.UpdateAsync(id, c);
        return !r.Success ? Results.NotFound(new { isSuccess = false, message = r.Message }) : Results.Ok(new { isSuccess = true, message = "Operación exitosa", data = r.Data });
    }).WithName("UpdateCarrera");
    carreraGroup.MapDelete("/{id:int}", async (int id, ICarreraService s) =>
    {
        var r = await s.DeleteAsync(id);
        return !r.Success ? Results.NotFound(new { isSuccess = false, message = r.Message }) : Results.NoContent();
    }).WithName("DeleteCarrera");

    // Profesores
    var profesorGroup = app.MapGroup("/api/profesores").WithTags("Profesores");
    profesorGroup.MapGet("/", async (IProfesorService s) => Results.Ok(new { isSuccess = true, message = "Operación exitosa", data = await s.GetAllAsync() })).WithName("GetAllProfesores");
    profesorGroup.MapGet("/{id:int}", async (int id, IProfesorService s) =>
    {
        var p = await s.GetByIdAsync(id);
        return p is null ? Results.NotFound(new { isSuccess = false, message = $"Profesor con Id {id} no fue encontrado." }) : Results.Ok(new { isSuccess = true, message = "Operación exitosa", data = p });
    }).WithName("GetProfesorById");
    profesorGroup.MapPost("/", async (Profesor p, IProfesorService s) =>
    {
        var r = await s.CreateAsync(p);
        return !r.Success ? Results.BadRequest(new { isSuccess = false, message = r.Message }) : Results.CreatedAtRoute("GetProfesorById", new { id = r.Data!.Id }, new { isSuccess = true, message = "Operación exitosa", data = r.Data });
    }).WithName("CreateProfesor").Accepts<Profesor>("application/json");
    profesorGroup.MapPut("/{id:int}", async (int id, Profesor p, IProfesorService s) =>
    {
        var r = await s.UpdateAsync(id, p);
        return !r.Success ? Results.NotFound(new { isSuccess = false, message = r.Message }) : Results.Ok(new { isSuccess = true, message = "Operación exitosa", data = r.Data });
    }).WithName("UpdateProfesor");
    profesorGroup.MapDelete("/{id:int}", async (int id, IProfesorService s) =>
    {
        var r = await s.DeleteAsync(id);
        return !r.Success ? Results.NotFound(new { isSuccess = false, message = r.Message }) : Results.NoContent();
    }).WithName("DeleteProfesor");

    // Formularios
    var formularioGroup = app.MapGroup("/api/formularios").WithTags("Formularios");
    formularioGroup.MapGet("/", async (IFormularioService s) => Results.Ok(new { isSuccess = true, message = "Operación exitosa", data = await s.GetAllAsync() })).WithName("GetAllFormularios");
    formularioGroup.MapGet("/{id:int}", async (int id, IFormularioService s) =>
    {
        var f = await s.GetByIdAsync(id);
        return f is null ? Results.NotFound(new { isSuccess = false, message = $"Formulario con Id {id} no fue encontrado." }) : Results.Ok(new { isSuccess = true, message = "Operación exitosa", data = f });
    }).WithName("GetFormularioById");
    formularioGroup.MapPost("/", async (Formulario f, IFormularioService s) =>
    {
        var r = await s.CreateAsync(f);
        return !r.Success ? Results.BadRequest(new { isSuccess = false, message = r.Message }) : Results.CreatedAtRoute("GetFormularioById", new { id = r.Data!.Id }, new { isSuccess = true, message = "Operación exitosa", data = r.Data });
    }).WithName("CreateFormulario").Accepts<Formulario>("application/json");
    formularioGroup.MapPut("/{id:int}", async (int id, Formulario f, IFormularioService s) =>
    {
        var r = await s.UpdateAsync(id, f);
        return !r.Success ? Results.NotFound(new { isSuccess = false, message = r.Message }) : Results.Ok(new { isSuccess = true, message = "Operación exitosa", data = r.Data });
    }).WithName("UpdateFormulario");
    formularioGroup.MapDelete("/{id:int}", async (int id, IFormularioService s) =>
    {
        var r = await s.DeleteAsync(id);
        return !r.Success ? Results.NotFound(new { isSuccess = false, message = r.Message }) : Results.NoContent();
    }).WithName("DeleteFormulario");

    // Listados
    var listadoGroup = app.MapGroup("/api/listado").WithTags("Listados");
    listadoGroup.MapGet("/", async (IListadoService s) => Results.Ok(new { isSuccess = true, message = "Operación exitosa", data = await s.GetListadoAsync() })).WithName("GetListado");

    // Stats
    app.MapGet("/api/stats", async (
        ICarreraService carreraService,
        IAlumnoService alumnoService,
        IAdministradorService adminService,
        IProfesorService profesorService,
        IFormularioService formularioService) =>
    {
        return Results.Ok(new
        {
            isSuccess = true,
            message = "Operación exitosa",
            data = new
            {
                totalCarreras = (await carreraService.GetAllAsync()).Count,
                totalAlumnos = (await alumnoService.GetAllAsync()).Count,
                totalAdmins = (await adminService.GetAllAsync()).Count,
                totalProfesores = (await profesorService.GetAllAsync()).Count,
                totalFormularios = (await formularioService.GetAllAsync()).Count
            }
        });
    }).WithName("GetStats").WithTags("Stats");
}

// Make Program accessible for integration tests
public partial class Program { }