using System.Text.Json;
using System.Reflection;
using Backend.Exceptions;

namespace Backend.Services;

public class CrudJsonService<T> : ICrudJsonService<T> where T : class
{
    private readonly string _path;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public CrudJsonService(string path)
    {
        _path = path;
        EnsureFileExists();
    }

    // ── Helpers privados ────────────────────────────────────────────────────────

    private void EnsureFileExists()
    {
        try
        {
            var dir = Path.GetDirectoryName(_path);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            if (!File.Exists(_path))
                File.WriteAllText(_path, "[]");
        }
        catch (IOException ex)
        {
            throw new PersistenceException($"No se pudo inicializar el archivo '{_path}'.", ex);
        }
    }

    private List<T> ReadFile()
    {
        try
        {
            var json = File.ReadAllText(_path);
            return JsonSerializer.Deserialize<List<T>>(json, _jsonOptions) ?? new List<T>();
        }
        catch (IOException ex)
        {
            throw new PersistenceException($"Error al leer el archivo '{_path}'.", ex);
        }
        catch (JsonException ex)
        {
            throw new PersistenceException($"El archivo '{_path}' contiene JSON inválido.", ex);
        }
        catch (Exception ex) when (ex is not PersistenceException)
        {
            throw new PersistenceException($"Error inesperado al leer '{_path}'.", ex);
        }
    }

    private void WriteFile(List<T> data)
    {
        try
        {
            var json = JsonSerializer.Serialize(data, _jsonOptions);
            File.WriteAllText(_path, json);
        }
        catch (IOException ex)
        {
            throw new PersistenceException($"Error al escribir en el archivo '{_path}'.", ex);
        }
        catch (Exception ex) when (ex is not PersistenceException)
        {
            throw new PersistenceException($"Error inesperado al escribir en '{_path}'.", ex);
        }
    }

    private static int GetId(T entity)
    {
        var prop = entity.GetType().GetProperty("Id")
            ?? throw new PersistenceException(
                $"El tipo '{typeof(T).Name}' no expone una propiedad 'Id'.");

        return (int)prop.GetValue(entity)!;
    }

    private static void SetId(T entity, int value)
    {
        var prop = entity.GetType().GetProperty("Id")
            ?? throw new PersistenceException(
                $"El tipo '{typeof(T).Name}' no expone una propiedad 'Id'.");

        prop.SetValue(entity, value);
    }

    // ── Operaciones públicas ─────────────────────────────────────────────────────

    public List<T> GetAll()
    {
        _semaphore.Wait();
        try
        {
            return ReadFile();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public T GetById(int id)
    {
        _semaphore.Wait();
        try
        {
            var entity = ReadFile().FirstOrDefault(e => GetId(e) == id);
            if (entity is null)
                throw new EntityNotFoundException(typeof(T).Name, id);

            return entity;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// Asigna el Id como max(existentes) + 1 dentro del semáforo,
    /// evitando la race condition entre GetAll() y Create() en el controlador.
    /// </summary>
    public T Create(T entity)
    {
        _semaphore.Wait();
        try
        {
            var list = ReadFile();
            int nextId = list.Count == 0 ? 1 : list.Max(GetId) + 1;
            SetId(entity, nextId);
            list.Add(entity);
            WriteFile(list);
            return entity;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public void Update(int id, T entity)
    {
        _semaphore.Wait();
        try
        {
            var list = ReadFile();
            var index = list.FindIndex(e => GetId(e) == id);
            if (index == -1)
                throw new EntityNotFoundException(typeof(T).Name, id);

            SetId(entity, id); // garantiza consistencia aunque el body no incluya Id
            list[index] = entity;
            WriteFile(list);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public void Delete(int id)
    {
        _semaphore.Wait();
        try
        {
            var list = ReadFile();
            var removed = list.RemoveAll(e => GetId(e) == id);
            if (removed == 0)
                throw new EntityNotFoundException(typeof(T).Name, id);

            WriteFile(list);
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
