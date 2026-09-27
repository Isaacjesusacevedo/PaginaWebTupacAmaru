using Instituto.AD.Models;

namespace Instituto.AD.Interfaces;

public interface ICarreraRepository
{
    List<Carrera> GetAll();
    Carrera? GetById(int id);
    Carrera Create(Carrera entity);
    void Update(int id, Carrera entity);
    void Delete(int id);
    bool Exists(int id);
}

public interface IAlumnoRepository
{
    List<Alumno> GetAll();
    Alumno? GetById(int id);
    Alumno Create(Alumno entity);
    void Update(int id, Alumno entity);
    void Delete(int id);
    bool Exists(int id);
    bool ExistsByDNI(int dni);
    bool ExistsByEmail(string email);
    bool ExistsByCarreraId(int carreraId);
}

public interface IAdministradorRepository
{
    List<Administrador> GetAll();
    Administrador? GetById(int id);
    Administrador? GetByEmail(string email);
    Administrador Create(Administrador entity);
    void Update(int id, Administrador entity);
    void Delete(int id); // Soft delete
    bool Exists(int id);
    bool ExistsByEmail(string email);
    int Count();
    void UpdatePasswordHash(int id, string newHash);
}

public interface IProfesorRepository
{
    List<Profesor> GetAll();
    Profesor? GetById(int id);
    Profesor Create(Profesor entity);
    void Update(int id, Profesor entity);
    void Delete(int id);
    bool Exists(int id);
    bool ExistsByEmail(string email);
}

public interface IFormularioRepository
{
    List<Formulario> GetAll();
    Formulario? GetById(int id);
    Formulario Create(Formulario entity);
    void Update(int id, Formulario entity);
    void Delete(int id);
    bool Exists(int id);
}

public interface IListadoRepository
{
    List<ListadoItem> GetListado();
}