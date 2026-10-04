namespace Instituto.BR;

public static class InfAcademicaConstants
{
    public const string TipoTitulo = "Título";
    public const string TipoTituloEnTramite = "Título en trámite";
    public const string TipoConstanciaMateriasAdeudadas = "Constancia de materias adeudadas";
    public const string TipoConstanciaAlumnoRegular = "Constancia de alumno regular";

    public const string EstadoTramite = "TRAMITE";
    public const string EstadoMano = "MANO";
    public const string EstadoPausa = "PAUSA";

    public static readonly string[] EstadosValidos = { EstadoTramite, EstadoMano, EstadoPausa };

    public const string CatalogoHabilitado = "HABILITADO";
    public const string CatalogoDeshabilitado = "DESHABILITADO";
}
