using System.ComponentModel.DataAnnotations;

namespace FrontEndBlazor.Models;

/// <summary>
/// DTO completo de movimiento (lectura desde API)
/// </summary>
public class MovimientoDto
{
    public string Id { get; set; } = string.Empty;
    
    public string MedicamentoId { get; set; } = string.Empty;
    
    public string Tipo { get; set; } = string.Empty;
    
    public int Cantidad { get; set; }
    
    public string? UsuarioId { get; set; }
    
    public string? Motivo { get; set; }
    
    public DateTime Fecha { get; set; }
    
    // Propiedades computadas para UI
    public string TipoBadgeClass => Tipo == "ENTRADA" ? "badge bg-success" : "badge bg-danger";
    
    public string TipoIcono => Tipo == "ENTRADA" ? "➕" : "➖";
}

/// <summary>
/// DTO para crear movimiento
/// </summary>
public class MovimientoCreateDto
{
    [Required(ErrorMessage = "El tipo de movimiento es obligatorio")]
    [RegularExpression("^(ENTRADA|SALIDA)$", ErrorMessage = "El tipo debe ser ENTRADA o SALIDA")]
    public string Tipo { get; set; } = "ENTRADA";
    
    [Required(ErrorMessage = "La cantidad es obligatoria")]
    [Range(1, 1000000, ErrorMessage = "La cantidad debe ser entre 1 y 1,000,000")]
    public int Cantidad { get; set; }
    
    [StringLength(200, ErrorMessage = "El motivo no puede exceder 200 caracteres")]
    public string? Motivo { get; set; }
}
