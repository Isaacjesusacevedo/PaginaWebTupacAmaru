using System;

namespace Instituto.AD.Models;

public class InfAcademica
{
    public int Id { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public string Estado { get; set; } = string.Empty;
}
