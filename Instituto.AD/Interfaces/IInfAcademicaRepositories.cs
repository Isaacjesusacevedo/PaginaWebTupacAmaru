using Instituto.AD.Models;

namespace Instituto.AD.Interfaces;

public interface IInfAcademicaRepository
{
    Task<List<InfAcademica>> GetAllAsync();
    Task<InfAcademica?> GetByIdAsync(int id);
    Task<int> CreateAsync(InfAcademica entity);
    Task UpdateAsync(InfAcademica entity);
    Task DeleteAsync(int id);
}

public interface IInfAcademicaEstRepository
{
    Task<List<InfAcademicaEst>> GetAllAsync();
    Task<InfAcademicaEst?> GetByIdAsync(int id);
    Task<List<InfAcademicaEst>> GetByAlumnoAsync(int alumnoId);
    Task<int> CreateAsync(InfAcademicaEst entity);
    Task UpdateAsync(InfAcademicaEst entity);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> ExistsByCombinationAsync(int infAcademicaId, int alumnoId, int? excludeId = null);
    Task<bool> ExistsByInfAcademicaAsync(int infAcademicaId);
}
