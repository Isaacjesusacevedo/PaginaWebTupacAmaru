namespace Backend.Services;

public interface ICrudJsonService<T> where T : class
{
    List<T> GetAll();

    /// <summary>Retorna la entidad o lanza EntityNotFoundException si no existe.</summary>
    T GetById(int id);

    /// <summary>Asigna el Id de forma autoincremental y persiste la entidad. Retorna la entidad con el Id asignado.</summary>
    T Create(T entity);

    void Update(int id, T entity);
    void Delete(int id);
}
