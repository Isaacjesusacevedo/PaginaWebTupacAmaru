using System.Collections.Generic;

namespace Backend.Services;

public interface ICrudJsonService<T> where T : class
{
    List<T> GetAll();
    T? GetById(int id);
    void Create(T entity);
    void Update(int id, T entity);
    void Delete(int id);
}
