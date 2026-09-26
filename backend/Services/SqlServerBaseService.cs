using System.Data;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Backend.Exceptions;

namespace Backend.Services;

public abstract class SqlServerBaseService<T> where T : class
{
    protected readonly string _connectionString;
    protected readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    protected SqlServerBaseService(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("SqlServer")
            ?? throw new InvalidOperationException("SqlServer connection string no configurada.");
    }

    protected SqlConnection OpenConnection()
    {
        var conn = new SqlConnection(_connectionString);
        conn.Open();
        return conn;
    }

    protected List<T> ExecuteQuery(string sql, Action<SqlCommand>? addParams = null)
    {
        try
        {
            using var conn = OpenConnection();
            using var cmd = new SqlCommand(sql, conn);
            addParams?.Invoke(cmd);

            var result = new List<T>();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var item = MapReaderToEntity(reader);
                if (item is not null) result.Add(item);
            }
            return result;
        }
        catch (Exception ex) when (ex is not PersistenceException)
        {
            throw new PersistenceException(
                $"[{typeof(T).Name}] SQL error: {ex.GetType().Name} — {ex.Message}", ex);
        }
    }

    protected T ExecuteQuerySingle(string entityName, int id, string sql,
        Action<SqlCommand>? addParams = null)
    {
        var list = ExecuteQuery(sql, addParams);
        return list.FirstOrDefault()
            ?? throw new EntityNotFoundException(entityName, id);
    }

    protected int ExecuteScalar(string sql, Action<SqlCommand> addParams)
    {
        using var conn = OpenConnection();
        using var cmd = new SqlCommand(sql, conn);
        addParams(cmd);
        var result = cmd.ExecuteScalar();
        return result != null ? Convert.ToInt32(result) : 0;
    }

    protected int ExecuteNonQuery(string sql, Action<SqlCommand> addParams)
    {
        using var conn = OpenConnection();
        using var cmd = new SqlCommand(sql, conn);
        addParams(cmd);
        return cmd.ExecuteNonQuery();
    }

    protected abstract T MapReaderToEntity(SqlDataReader reader);
}