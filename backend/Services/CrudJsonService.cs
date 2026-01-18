using System.Text.Json;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace Backend.Services;

public class CrudJsonService<T> : ICrudJsonService<T> where T : class
{
    private readonly string _path;

    public CrudJsonService(string path)
    {
        _path = path;
    }

    private List<T> ReadFile()
    {
        if (!File.Exists(_path))
            return new List<T>();

        var json = File.ReadAllText(_path);
        return JsonSerializer.Deserialize<List<T>>(json) ?? new();
    }

    private void WriteFile(List<T> data)
    {
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_path, json);
    }

    public List<T> GetAll() => ReadFile();

    public T? GetById(int id)
    {
        var list = ReadFile();
        return list.FirstOrDefault(e =>
            (int)e!.GetType().GetProperty("Id")!.GetValue(e)! == id
        );
    }

    public void Create(T entity)
    {
        var list = ReadFile();
        list.Add(entity);
        WriteFile(list);
    }

    public void Update(int id, T entity)
    {
        var list = ReadFile();
        var index = list.FindIndex(e =>
            (int)e!.GetType().GetProperty("Id")!.GetValue(e)! == id
        );

        if (index != -1)
        {
            list[index] = entity;
            WriteFile(list);
        }
    }

    public void Delete(int id)
    {
        var list = ReadFile();
        list.RemoveAll(e =>
            (int)e!.GetType().GetProperty("Id")!.GetValue(e)! == id
        );
        WriteFile(list);
    }
}
