using Instituto.AD.Interfaces;
using Instituto.AD.Models;
using Instituto.AD.Data;
using Microsoft.EntityFrameworkCore;

namespace Instituto.AD.Repositories;

public class ListadoRepository : IListadoRepository
{
    private readonly InstitutoDbContext _context;

    public ListadoRepository(InstitutoDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<List<ListadoItem>> GetListadoAsync()
    {
        return await _context.Database
            .SqlQueryRaw<ListadoItem>("EXEC sp_Listado_GetAll")
            .ToListAsync();
    }
}