using Backend.Models;
using Backend.Services;

var builder = WebApplication.CreateBuilder(args);

// 🔹 CORS para Vue
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

// 🔹 Controllers + JSON options (IMPORTANTE)
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

// 🔹 Servicios JSON
builder.Services.AddSingleton<ICrudJsonService<Carrera>>(
    new CrudJsonService<Carrera>("Data/Carrera.json")
);
builder.Services.AddSingleton<ICrudJsonService<Alumno>>(
    new CrudJsonService<Alumno>("Data/Alumno.json")
);
builder.Services.AddSingleton<ICrudJsonService<Administrador>>(
    new CrudJsonService<Administrador>("Data/Administrador.json")
);

var app = builder.Build();

// 🔹 Orden CORRECTO del middleware
app.UseCors("VueCors");

app.UseRouting();

app.MapControllers();

// 🔹 Test rápido
app.MapGet("/", () => "Backend corriendo correctamente!");

app.Run();
