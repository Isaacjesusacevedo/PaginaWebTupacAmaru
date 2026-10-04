using Instituto.AD.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Instituto.AD;

// Helper reutilizable para invocar Stored Procedures desde la AD vía EF Core.
// Las entidades se materializan siempre con ToListAsync (nunca FirstOrDefaultAsync
// u otro operador LINQ sobre el FromSqlRaw) y se filtran en memoria.
public static class SpInvoker
{
    public static async Task<List<T>> QueryEntityAsync<T>(this InstitutoDbContext context, string sql, params SqlParameter[] parameters)
        where T : class
        => await context.Set<T>().FromSqlRaw(sql, parameters).ToListAsync();

    public static async Task<bool> ExistsAsync(this InstitutoDbContext context, string sql, params SqlParameter[] parameters)
    {
        var rows = await context.Database.SqlQueryRaw<int>(sql, parameters).ToListAsync();
        return rows.Count > 0 && rows[0] == 1;
    }

    public static async Task<int> ScalarInsertAsync(this InstitutoDbContext context, string sql, params SqlParameter[] parameters)
    {
        await context.Database.OpenConnectionAsync();
        try
        {
            using var command = context.Database.GetDbConnection().CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddRange(parameters);
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }
        finally
        {
            await context.Database.CloseConnectionAsync();
        }
    }

    public static async Task NonQueryAsync(this InstitutoDbContext context, string sql, params SqlParameter[] parameters)
        => await context.Database.ExecuteSqlRawAsync(sql, parameters);
}
