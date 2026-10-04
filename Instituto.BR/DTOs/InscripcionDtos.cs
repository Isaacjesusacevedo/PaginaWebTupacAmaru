namespace Instituto.BR.DTOs;

public class InformacionAcademicaItemDto
{
    public string Tipo { get; set; } = string.Empty;
    public DateTime FechaEmision { get; set; }
    public string? TituloSecundario { get; set; }
    public string? Institucion { get; set; }
    public string? EstadoTitulo { get; set; }
}

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
    public List<InformacionAcademicaItemDto> InformacionAcademica { get; set; } = new();
}

public class InscripcionResultDto
{
    public int AlumnoId { get; set; }
}
