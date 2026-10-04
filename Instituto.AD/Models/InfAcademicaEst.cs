using System;

namespace Instituto.AD.Models;

public class InfAcademicaEst
{
    public int Id { get; set; }
    public int InfAcademicaId { get; set; }
    public int AlumnoId { get; set; }
    public DateTime FechaEmision { get; set; }
    public string? TituloSecundario { get; set; }
    public string? Institucion { get; set; }
    public string EstadoTitulo { get; set; } = string.Empty;
}
