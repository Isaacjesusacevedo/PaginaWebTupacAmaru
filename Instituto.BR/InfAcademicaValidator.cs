namespace Instituto.BR;

// Reglas de validación compartidas por InfAcademicaEstService e InscripcionService,
// para no duplicar las mismas condiciones en los dos lugares donde se guarda un registro académico.
public static class InfAcademicaValidator
{
    public static string ResolverEstadoPorDefecto(string? estado, string tipoDescripcion)
    {
        if (!string.IsNullOrWhiteSpace(estado))
            return estado;

        return tipoDescripcion == InfAcademicaConstants.TipoTituloEnTramite
            ? InfAcademicaConstants.EstadoTramite
            : InfAcademicaConstants.EstadoMano;
    }

    public static string? ValidarEstado(string estado)
        => InfAcademicaConstants.EstadosValidos.Contains(estado)
            ? null
            : $"El estado debe ser uno de: {string.Join(", ", InfAcademicaConstants.EstadosValidos)}.";

    public static string? ValidarFechaEmision(DateTime fechaEmision, string estado)
    {
        if (fechaEmision == default)
            return "La fecha de emisión es obligatoria.";

        // En TRAMITE la fecha es estimada, por eso puede ser futura.
        if (estado != InfAcademicaConstants.EstadoTramite && fechaEmision.Date > DateTime.Today)
            return "La fecha de emisión no puede ser futura.";

        return null;
    }

    public static (string? Error, string? TituloSecundario, string? Institucion) ResolverCamposPorTipo(
        string tipoDescripcion, string? tituloSecundario, string? institucion)
    {
        switch (tipoDescripcion)
        {
            case InfAcademicaConstants.TipoTitulo:
                if (string.IsNullOrWhiteSpace(tituloSecundario))
                    return ("El título secundario es obligatorio para el tipo 'Título'.", null, null);
                if (tituloSecundario.Length > 100)
                    return ("El título secundario no puede superar los 100 caracteres.", null, null);
                return (null, tituloSecundario, null);

            case InfAcademicaConstants.TipoTituloEnTramite:
                if (string.IsNullOrWhiteSpace(institucion))
                    return ("La institución es obligatoria para el tipo 'Título en trámite'.", null, null);
                if (institucion.Length > 150)
                    return ("La institución no puede superar los 150 caracteres.", null, null);
                return (null, null, institucion);

            default:
                return (null, null, null);
        }
    }
}
