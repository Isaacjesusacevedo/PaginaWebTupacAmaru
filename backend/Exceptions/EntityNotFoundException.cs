namespace Backend.Exceptions;

public class EntityNotFoundException : Exception
{
    public EntityNotFoundException(string message) : base(message) { }

    public EntityNotFoundException(string entityName, int id)
        : base($"{entityName} con Id {id} no fue encontrado/a.") { }
}
