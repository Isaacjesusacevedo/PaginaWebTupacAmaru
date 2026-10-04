namespace Instituto.BR.DTOs;

public class InscripcionDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Dni { get; set; }
    public DateTime FechaNacimiento { get; set; }
    public string? Direccion { get; set; }
    public string? Nacionalidad { get; set; }
    public string? Telefono { get; set; }
    public string? Turno { get; set; }
    public int CarreraId { get; set; }

    // Info académica (dentro de la misma inscripción)
    public DateTime? FechaEgreso { get; set; }
    public string? TituloSecundario { get; set; }
    public bool PoseeTitulo { get; set; }
    public bool TituloEnTramite { get; set; }
    public bool ConsMaterias { get; set; }
    public bool ConsAlumnoRegular { get; set; }
}

public class InscripcionResultDto
{
    public int AlumnoId { get; set; }
}