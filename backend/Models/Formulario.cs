using System.ComponentModel.DataAnnotations;

namespace Backend.Models;

public class Formulario
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(200, ErrorMessage = "El nombre no puede superar los 200 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El estado es obligatorio.")]
    [StringLength(20, ErrorMessage = "El estado no puede superar los 20 caracteres.")]
    public string Estado { get; set; } = "Borrador"; // Borrador, Abierto, Cerrado

    [Required(ErrorMessage = "La fecha de apertura es obligatoria.")]
    public DateTime FechaApertura { get; set; }

    [Required(ErrorMessage = "La fecha de cierre es obligatoria.")]
    public DateTime FechaCierre { get; set; }

    [StringLength(1000, ErrorMessage = "La descripción no puede superar los 1000 caracteres.")]
    public string? Descripcion { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.Now;
}