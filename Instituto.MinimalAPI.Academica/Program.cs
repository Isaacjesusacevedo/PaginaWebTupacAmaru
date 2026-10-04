using System.Text;
using Instituto.AD.Data;
using Instituto.AD.Interfaces;
using Instituto.AD.Repositories;
using Instituto.BR.Interfaces;
using Instituto.BR.Services;
using Instituto.MinimalAPI.Academica.Endpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ── JWT ──────────────────────────────────────────────────────────────────────
// La clave nunca se commitea: "dotnet user-secrets set Jwt:Key ..." o la variable
// de entorno Jwt__Key. Si falta o es débil, la API no arranca.
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Length < 32)
{
    throw new InvalidOperationException(
        "Jwt:Key no está configurado o tiene menos de 32 caracteres. " +
        "Configuralo con 'dotnet user-secrets set \"Jwt:Key\" \"...\"' o la variable de entorno Jwt__Key.");
}

var jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException("Jwt:Issuer no está configurado.");
var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException("Jwt:Audience no está configurado.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminPolicy", policy => policy.RequireRole("Admin", "SuperAdmin"));

// ── CORS ───────────────────────────────────────────────────────────────────────
// En desarrollo: AllowAnyOrigin para facilitar testing con frontend local.
// En producción: usar Cors:AllowedOrigins desde configuración.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AcademicaCors", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        }
        else
        {
            var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                ?? Array.Empty<string>();
            policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod();
        }
    });
});

// ── JSON Options ──────────────────────────────────────────────────────────────
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
});

// ── EF Core ───────────────────────────────────────────────────────────────────
var connectionString = builder.Configuration.GetConnectionString("SqlServer")
    ?? throw new InvalidOperationException("ConnectionStrings:SqlServer no configurado.");

builder.Services.AddDbContext<InstitutoDbContext>(options => options.UseSqlServer(connectionString));

// ── Repositories (AD) ─────────────────────────────────────────────────────────
builder.Services.AddScoped<IAlumnoRepository, AlumnoRepository>();
builder.Services.AddScoped<ICarreraRepository, CarreraRepository>();
builder.Services.AddScoped<IListadoRepository, ListadoRepository>();
builder.Services.AddScoped<IInfAcademicaRepository, InfAcademicaRepository>();
builder.Services.AddScoped<IInfAcademicaEstRepository, InfAcademicaEstRepository>();

// ── Services (BR) ─────────────────────────────────────────────────────────────
builder.Services.AddScoped<IAlumnoService, AlumnoService>();
builder.Services.AddScoped<ICarreraService, CarreraService>();
builder.Services.AddScoped<IListadoService, ListadoService>();
builder.Services.AddScoped<IInfAcademicaService, InfAcademicaService>();
builder.Services.AddScoped<IInfAcademicaEstService, InfAcademicaEstService>();
builder.Services.AddScoped<IInscripcionService, InscripcionService>();

// ── Swagger/OpenAPI ────────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Instituto API - Información Académica",
        Version = "v1",
        Description = "Listado, información académica e inscripción - Instituto Superior Docente Túpac Amaru"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingrese el token JWT: Bearer {token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// ── Handler global de errores: logea el detalle, responde un mensaje genérico ─
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        var logger = context.RequestServices.GetRequiredService<ILoggerFactory>()
            .CreateLogger("GlobalExceptionHandler");
        logger.LogError(exception, "Error no controlado en {Path}", context.Request.Path);

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new
        {
            isSuccess = false,
            message = "Ocurrió un error interno del servidor."
        });
    });
});

app.UseRouting();
app.UseCors("AcademicaCors");
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHealthEndpoint();
app.MapListadoEndpoint();
app.MapInfAcademicaListEndpoint();
app.MapInfAcademicaGetByIdEndpoint();
app.MapInfAcademicaCreateEndpoint();
app.MapInfAcademicaUpdateEndpoint();
app.MapInfAcademicaDeleteEndpoint();
app.MapInfAcademicaAlumnosListEndpoint();
app.MapInfAcademicaAlumnosCreateEndpoint();
app.MapInfAcademicaAlumnosUpdateEndpoint();
app.MapInfAcademicaAlumnosDeleteEndpoint();
app.MapInscripcionEndpoint();

app.Run();

public partial class Program { }
