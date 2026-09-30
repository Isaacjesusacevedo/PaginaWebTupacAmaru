using System.Text.Json;
using Instituto.BR.Interfaces;
using Instituto.BR.Services;
using Instituto.AD.Interfaces;
using Instituto.AD.Repositories;
using Instituto.AD;

var builder = WebApplication.CreateBuilder(args);

// ── CORS para el front-end Vue ───────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("VueCors", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

// ── Controllers + JSON options ──────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
        options.JsonSerializerOptions.WriteIndented = false;
    });

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
    options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
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
app.MapControllers();

// Health check endpoint
app.MapGet("/", () => "Backend corriendo correctamente!");

app.Run();

// Make Program accessible for integration tests
public partial class Program { }