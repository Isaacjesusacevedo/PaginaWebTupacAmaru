using Instituto.AD.Models;
using Instituto.BR.DTOs;

namespace Instituto.BR.Interfaces;

public interface ICarreraService
{
    List<Carrera> GetAll();
    Carrera? GetById(int id);
    ServiceResult<Carrera> Create(Carrera carrera);
    ServiceResult<Carrera> Update(int id, Carrera carrera);
    ServiceResult Delete(int id);
}

public interface IAlumnoService
{
    List<Alumno> GetAll();
    Alumno? GetById(int id);
    ServiceResult<Alumno> Create(Alumno alumno);
    ServiceResult<Alumno> Update(int id, Alumno alumno);
    ServiceResult Delete(int id);
}

public interface IAdministradorService
{
    List<Administrador> GetAll();
    Administrador? GetById(int id);
    ServiceResult<Administrador> Create(Administrador admin, string password);
    ServiceResult<Administrador> Update(int id, Administrador admin);
    ServiceResult ChangePassword(int id, string passwordActual, string nuevaPassword);
    ServiceResult Delete(int id);
    AdminResult? Login(string email, string password);
    bool HayAdmins();
    Task<AdminResult?> CrearPrimerAdminAsync(SetupAdminDto dto);
    ServiceResult ChangePasswordByEmail(string email, string passwordActual, string nuevaPassword);
}

public interface IProfesorService
{
    List<Profesor> GetAll();
    Profesor? GetById(int id);
    ServiceResult<Profesor> Create(Profesor profesor);
    ServiceResult<Profesor> Update(int id, Profesor profesor);
    ServiceResult Delete(int id);
}

public interface IFormularioService
{
    List<Formulario> GetAll();
    Formulario? GetById(int id);
    ServiceResult<Formulario> Create(Formulario formulario);
    ServiceResult<Formulario> Update(int id, Formulario formulario);
    ServiceResult Delete(int id);
}

public interface IListadoService
{
    List<AlumnoListadoDto> GetListado();
}