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
using Instituto.MinimalAPI.Endpoints;

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
// ENDPOINTS MAPPING
// ═══════════════════════════════════════════════════════════════════════

app.MapAuthEndpoints();
app.MapSetupEndpoints();
app.MapAdminEndpoints();
app.MapAlumnoEndpoints();
app.MapCarreraEndpoints();
app.MapProfesorEndpoints();
app.MapFormularioEndpoints();
app.MapListadoEndpoints();
app.MapHealthEndpoints();
app.MapDebugEndpoints();

// Run
app.Run();

// Make Program accessible for integration tests
public partial class Program { }