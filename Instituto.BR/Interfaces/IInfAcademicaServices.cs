using Instituto.AD.Models;
using Instituto.BR.DTOs;

namespace Instituto.BR.Interfaces;

public interface IInfAcademicaService
{
    Task<List<InfAcademica>> GetAllAsync();
    Task<InfAcademica?> GetByIdAsync(int id);
    Task<ServiceResult<InfAcademica>> CreateAsync(InfAcademica entity);
    Task<ServiceResult<InfAcademica>> UpdateAsync(int id, InfAcademica entity);
    Task<ServiceResult> DeleteAsync(int id);
}

public interface IInfAcademicaEstService
{
    Task<List<InfAcademicaEst>> GetAllAsync();
    Task<InfAcademicaEst?> GetByIdAsync(int id);
    Task<List<InfAcademicaEst>> GetByAlumnoAsync(int alumnoId);
    Task<ServiceResult<InfAcademicaEst>> CreateAsync(InfAcademicaEst entity);
    Task<ServiceResult<InfAcademicaEst>> UpdateAsync(int id, InfAcademicaEst entity);
    Task<ServiceResult> DeleteAsync(int id);
}

public interface IInscripcionService
{
    Task<ServiceResult<InscripcionResultDto>> InscribirAsync(InscripcionDto dto);
}
