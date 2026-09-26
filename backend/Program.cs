using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Backend.Models;
using Backend.Services;

var builder = WebApplication.CreateBuilder(args);

// ── CORS para el front-end Vue ───────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("VueCors", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// ── JWT Authentication ───────────────────────────────────────────────────────
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key no configurado en appsettings.json.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer           = true,
            ValidIssuer              = builder.Configuration["Jwt:Issuer"],
            ValidateAudience         = true,
            ValidAudience            = builder.Configuration["Jwt:Audience"],
            ValidateLifetime         = true,
            ClockSkew                = TimeSpan.FromMinutes(5)   // margen de gracia 5 min
        };

        // Devolver JSON en respuestas 401/403 en lugar del HTML por defecto
        options.Events = new JwtBearerEvents
        {
            OnChallenge = ctx =>
            {
                ctx.HandleResponse();
                ctx.Response.StatusCode  = 401;
                ctx.Response.ContentType = "application/json";
                return ctx.Response.WriteAsync(
                    JsonSerializer.Serialize(new { error = "No autenticado. Iniciá sesión." }));
            },
            OnForbidden = ctx =>
            {
                ctx.Response.StatusCode  = 403;
                ctx.Response.ContentType = "application/json";
                return ctx.Response.WriteAsync(
                    JsonSerializer.Serialize(new { error = "No tenés permiso para realizar esta acción." }));
            }
        };
    });

builder.Services.AddAuthorization();

// ── Controllers + opciones JSON ──────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
        options.JsonSerializerOptions.WriteIndented = false;
    });

// Configurar HttpClient/Json para UTF-8 en todo el pipeline
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
});

// ── Servicios de persistencia — SQL Server ────────────────────────────────────
builder.Services.AddScoped<ICrudJsonService<Carrera>,       CarreraSqlServerService>();
builder.Services.AddScoped<ICrudJsonService<Alumno>,        AlumnoSqlServerService>();
builder.Services.AddScoped<ICrudJsonService<Administrador>, AdministradorSqlServerService>();
builder.Services.AddScoped<ICrudJsonService<Profesor>,      ProfesorSqlServerService>();
builder.Services.AddScoped<ICrudJsonService<Formulario>,    FormularioSqlServerService>();

// ── Servicio de autenticación con Supabase ───────────────────────────────────
builder.Services.AddScoped<IAdminAuthService, AdminAuthService>();

// ── Build ────────────────────────────────────────────────────────────────────
var app = builder.Build();

// ── Middleware global de errores no capturados ───────────────────────────────
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode  = 500;
        context.Response.ContentType = "application/json";
        var body = JsonSerializer.Serialize(new { error = "Ocurrió un error interno del servidor." });
        await context.Response.WriteAsync(body);
    });
});

// ── Pipeline de middleware ───────────────────────────────────────────────────
app.UseRouting();
app.UseCors("VueCors");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Endpoint de salud rápida
app.MapGet("/", () => "Backend corriendo correctamente!");

app.Run();
