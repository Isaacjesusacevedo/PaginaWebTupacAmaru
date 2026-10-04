using Instituto.AD.Data;
using Instituto.AD.Interfaces;
using Instituto.AD.Models;
using Microsoft.Data.SqlClient;

namespace Instituto.AD.Repositories;

public class InfAcademicaEstRepository : IInfAcademicaEstRepository
{
    private readonly InstitutoDbContext _context;

    public InfAcademicaEstRepository(InstitutoDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<List<InfAcademicaEst>> GetAllAsync()
        => await _context.QueryEntityAsync<InfAcademicaEst>("EXEC Inf_Academica_Est_List");

    public async Task<InfAcademicaEst?> GetByIdAsync(int id)
    {
        var items = await _context.QueryEntityAsync<InfAcademicaEst>(
            "EXEC Inf_Academica_Est_GetById @ID_Inf_Academica_Est", new SqlParameter("@ID_Inf_Academica_Est", id));
        return items.FirstOrDefault();
    }

    public async Task<List<InfAcademicaEst>> GetByAlumnoAsync(int alumnoId)
        => await _context.QueryEntityAsync<InfAcademicaEst>(
            "EXEC Inf_Academica_Est_GetByAlumno @ID_Est", new SqlParameter("@ID_Est", alumnoId));

    public async Task<int> CreateAsync(InfAcademicaEst entity)
    {
        return await _context.ScalarInsertAsync(
            "EXEC Inf_Academica_Est_Insert @ID_Inf_Aca, @ID_Est, @Fecha_Emision, @Titulo_Secundario, @Institucion, @Estado_Titulo",
            new SqlParameter("@ID_Inf_Aca", entity.InfAcademicaId),
            new SqlParameter("@ID_Est", entity.AlumnoId),
            new SqlParameter("@Fecha_Emision", entity.FechaEmision),
            new SqlParameter("@Titulo_Secundario", entity.TituloSecundario ?? (object)DBNull.Value),
            new SqlParameter("@Institucion", entity.Institucion ?? (object)DBNull.Value),
            new SqlParameter("@Estado_Titulo", entity.EstadoTitulo));
    }

    public async Task UpdateAsync(InfAcademicaEst entity)
    {
        await _context.NonQueryAsync(
            "EXEC Inf_Academica_Est_Update @ID_Inf_Academica_Est, @ID_Inf_Aca, @Fecha_Emision, @Titulo_Secundario, @Institucion, @Estado_Titulo",
            new SqlParameter("@ID_Inf_Academica_Est", entity.Id),
            new SqlParameter("@ID_Inf_Aca", entity.InfAcademicaId),
            new SqlParameter("@Fecha_Emision", entity.FechaEmision),
            new SqlParameter("@Titulo_Secundario", entity.TituloSecundario ?? (object)DBNull.Value),
            new SqlParameter("@Institucion", entity.Institucion ?? (object)DBNull.Value),
            new SqlParameter("@Estado_Titulo", entity.EstadoTitulo));
    }

    public async Task DeleteAsync(int id)
        => await _context.NonQueryAsync(
            "EXEC Inf_Academica_Est_Delete @ID_Inf_Academica_Est", new SqlParameter("@ID_Inf_Academica_Est", id));

    public async Task<bool> ExistsAsync(int id)
        => await _context.ExistsAsync(
            "EXEC Inf_Academica_Est_Exists @ID_Inf_Academica_Est", new SqlParameter("@ID_Inf_Academica_Est", id));

    public async Task<bool> ExistsByCombinationAsync(int infAcademicaId, int alumnoId, int? excludeId = null)
        => await _context.ExistsAsync(
            "EXEC Inf_Academica_Est_ExistsByCombination @ID_Inf_Aca, @ID_Est, @ExcludeId",
            new SqlParameter("@ID_Inf_Aca", infAcademicaId),
            new SqlParameter("@ID_Est", alumnoId),
            new SqlParameter("@ExcludeId", excludeId ?? (object)DBNull.Value));

    public async Task<bool> ExistsByInfAcademicaAsync(int infAcademicaId)
        => await _context.ExistsAsync(
            "EXEC Inf_Academica_Est_ExistsByInfAca @ID_Inf_Aca", new SqlParameter("@ID_Inf_Aca", infAcademicaId));
}
