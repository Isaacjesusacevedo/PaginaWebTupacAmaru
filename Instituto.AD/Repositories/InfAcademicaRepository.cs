using Instituto.AD.Data;
using Instituto.AD.Interfaces;
using Instituto.AD.Models;
using Microsoft.Data.SqlClient;

namespace Instituto.AD.Repositories;

public class InfAcademicaRepository : IInfAcademicaRepository
{
    private readonly InstitutoDbContext _context;

    public InfAcademicaRepository(InstitutoDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<List<InfAcademica>> GetAllAsync()
        => await _context.QueryEntityAsync<InfAcademica>("EXEC Inf_Academica_List");

    public async Task<InfAcademica?> GetByIdAsync(int id)
    {
        var items = await _context.QueryEntityAsync<InfAcademica>(
            "EXEC Inf_Academica_GetById @ID_Inf_Aca", new SqlParameter("@ID_Inf_Aca", id));
        return items.FirstOrDefault();
    }

    public async Task<int> CreateAsync(InfAcademica entity)
    {
        return await _context.ScalarInsertAsync(
            "EXEC Inf_Academica_Insert @Inf_Aca_Descripcion, @Inf_Aca_Fecha, @Inf_Aca_Estado",
            new SqlParameter("@Inf_Aca_Descripcion", entity.Descripcion),
            new SqlParameter("@Inf_Aca_Fecha", entity.Fecha),
            new SqlParameter("@Inf_Aca_Estado", entity.Estado));
    }

    public async Task UpdateAsync(InfAcademica entity)
    {
        await _context.NonQueryAsync(
            "EXEC Inf_Academica_Update @ID_Inf_Aca, @Inf_Aca_Descripcion, @Inf_Aca_Fecha, @Inf_Aca_Estado",
            new SqlParameter("@ID_Inf_Aca", entity.Id),
            new SqlParameter("@Inf_Aca_Descripcion", entity.Descripcion),
            new SqlParameter("@Inf_Aca_Fecha", entity.Fecha),
            new SqlParameter("@Inf_Aca_Estado", entity.Estado));
    }

    public async Task DeleteAsync(int id)
        => await _context.NonQueryAsync("EXEC Inf_Academica_Delete @ID_Inf_Aca", new SqlParameter("@ID_Inf_Aca", id));
}
