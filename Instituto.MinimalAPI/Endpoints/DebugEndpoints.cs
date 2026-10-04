using Instituto.AD.Data;
using Instituto.AD.Interfaces;
using Instituto.BR.Interfaces;

namespace Instituto.MinimalAPI.Endpoints;

public static class DebugEndpoints
{
    public static void MapDebugEndpoints(this WebApplication app)
    {
        app.MapGet("/debug-config", () =>
        {
            var envConnDebug = Environment.GetEnvironmentVariable("CONNECTION_STRING_SQLSERVER");
            var configConn = app.Configuration.GetConnectionString("SqlServer");
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
    }
}