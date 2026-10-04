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
            "EXEC Inf_Academica_Est_GetById @ID_Inf_Academica_Est",
            new SqlParameter("@ID_Inf_Academica_Est", id));
        return items.FirstOrDefault();
    }

    public async Task<InfAcademicaEst?> GetByAlumnoIdAsync(int alumnoId)
    {
        var items = await _context.QueryEntityAsync<InfAcademicaEst>(
            "EXEC Inf_Academica_Est_GetByAlumno @ID_Est",
            new SqlParameter("@ID_Est", alumnoId));
        return items.FirstOrDefault();
    }

    public async Task<int> CreateAsync(InfAcademicaEst entity)
    {
        return await _context.ScalarInsertAsync(
            "EXEC Inf_Academica_Est_Insert @ID_Est, @Fecha_Egreso, @Titulo_Secundario, " +
            "@Posee_Titulo, @Titulo_En_Tramite, @Cons_Materias, @Cons_Alumno_Regular",
            new SqlParameter("@ID_Est", entity.AlumnoId),
            new SqlParameter("@Fecha_Egreso", entity.FechaEgreso ?? (object)DBNull.Value),
            new SqlParameter("@Titulo_Secundario", entity.TituloSecundario ?? (object)DBNull.Value),
            new SqlParameter("@Posee_Titulo", entity.PoseeTitulo),
            new SqlParameter("@Titulo_En_Tramite", entity.TituloEnTramite),
            new SqlParameter("@Cons_Materias", entity.ConsMaterias),
            new SqlParameter("@Cons_Alumno_Regular", entity.ConsAlumnoRegular));
    }

    public async Task UpdateAsync(InfAcademicaEst entity)
    {
        await _context.NonQueryAsync(
            "EXEC Inf_Academica_Est_Update @ID_Inf_Academica_Est, @Fecha_Egreso, " +
            "@Titulo_Secundario, @Posee_Titulo, @Titulo_En_Tramite, @Cons_Materias, @Cons_Alumno_Regular",
            new SqlParameter("@ID_Inf_Academica_Est", entity.Id),
            new SqlParameter("@Fecha_Egreso", entity.FechaEgreso ?? (object)DBNull.Value),
            new SqlParameter("@Titulo_Secundario", entity.TituloSecundario ?? (object)DBNull.Value),
            new SqlParameter("@Posee_Titulo", entity.PoseeTitulo),
            new SqlParameter("@Titulo_En_Tramite", entity.TituloEnTramite),
            new SqlParameter("@Cons_Materias", entity.ConsMaterias),
            new SqlParameter("@Cons_Alumno_Regular", entity.ConsAlumnoRegular));
    }

    public async Task<bool> ExistsAsync(int id)
        => await _context.ExistsAsync(
            "EXEC Inf_Academica_Est_Exists @ID_Inf_Academica_Est",
            new SqlParameter("@ID_Inf_Academica_Est", id));

    public async Task<bool> ExistsByAlumnoIdAsync(int alumnoId)
    => await _context.ExistsAsync(
        "EXEC Inf_Academica_Est_ExistsByAlumno @ID_Est",
        new SqlParameter("@ID_Est", alumnoId));

    public async Task DeleteAsync(int id)
        => await _context.NonQueryAsync(
            "EXEC Inf_Academica_Est_Delete @ID_Inf_Academica_Est",
            new SqlParameter("@ID_Inf_Academica_Est", id));
}