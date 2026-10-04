using System;

namespace Instituto.AD.Models;

public class ListadoItem
{
    public int AlumnoId { get; set; }
    public string? NombreCompleto { get; set; }
    public int DNI { get; set; }
    public string? Email { get; set; }
    public string? Carrera { get; set; }
    public string? Turno { get; set; }
    public DateTime FechaNacimiento { get; set; }
    public DateTime? FechaEgreso { get; set; }
    public string? TituloSecundario { get; set; }
    public bool PoseeTitulo { get; set; }
    public bool TituloEnTramite { get; set; }
    public bool ConsMaterias { get; set; }
    public bool ConsAlumnoRegular { get; set; }
}