using System;

namespace Instituto.AD.Models;

public class InfAcademicaEst
{
    public int Id { get; set; }
    public int AlumnoId { get; set; }
    public DateTime? FechaEgreso { get; set; }
    public string? TituloSecundario { get; set; }
    public bool PoseeTitulo { get; set; }
    public bool TituloEnTramite { get; set; }
    public bool ConsMaterias { get; set; }
    public bool ConsAlumnoRegular { get; set; }
}