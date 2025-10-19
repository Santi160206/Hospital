using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FrontEndBlazor.Models;

/// <summary>
/// DTO completo de medicamento (lectura desde API)
/// </summary>
public class MedicamentoDto
{
    public string Id { get; set; } = string.Empty;
    
    public string Nombre { get; set; } = string.Empty;
    
    public string Fabricante { get; set; } = string.Empty;
    
    public string Presentacion { get; set; } = string.Empty;
    
    public string Lote { get; set; } = string.Empty;
    
    public DateTime FechaVencimiento { get; set; }
    
    public int Stock { get; set; }
    
    public int MinimoStock { get; set; }
    
    public double Precio { get; set; }
    
    public string? PrincipioActivo { get; set; }
    
    public string Estado { get; set; } = "ACTIVO";
    
    public bool IsDeleted { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    // Propiedades computadas para UI
    public bool EstaVencido => FechaVencimiento < DateTime.Today;
    
    public bool ProximoAVencer => FechaVencimiento <= DateTime.Today.AddDays(30) && !EstaVencido;
    
    public bool StockBajo => Stock <= MinimoStock;
    
    public string EstadoBadgeClass => Estado == "ACTIVO" ? "badge bg-success" : "badge bg-danger";
    
    public string VencimientoBadgeClass => EstaVencido ? "badge bg-danger" : 
                                           ProximoAVencer ? "badge bg-warning" : 
                                           "badge bg-success";
}

/// <summary>
/// DTO para crear medicamento
/// </summary>
public class MedicamentoCreateDto
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 200 caracteres")]
    public string Nombre { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "El fabricante es obligatorio")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "El fabricante debe tener entre 2 y 200 caracteres")]
    public string Fabricante { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "La presentación es obligatoria")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "La presentación debe tener entre 2 y 200 caracteres")]
    public string Presentacion { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "El lote es obligatorio")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "El lote debe tener entre 1 y 100 caracteres")]
    public string Lote { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "La fecha de vencimiento es obligatoria")]
    public DateTime? FechaVencimiento { get; set; }
    
    [Required(ErrorMessage = "El stock inicial es obligatorio")]
    [Range(0, 1000000, ErrorMessage = "El stock debe ser entre 0 y 1,000,000")]
    public int Stock { get; set; }
    
    [Range(0, 1000000, ErrorMessage = "El stock mínimo debe ser entre 0 y 1,000,000")]
    public int MinimoStock { get; set; } = 0;
    
    [Range(0, 999999.99, ErrorMessage = "El precio debe ser entre 0 y 999,999.99")]
    public double Precio { get; set; } = 0;
    
    [StringLength(300, ErrorMessage = "El principio activo no puede exceder 300 caracteres")]
    public string? PrincipioActivo { get; set; }
}

/// <summary>
/// DTO para actualizar medicamento (todos los campos opcionales)
/// </summary>
public class MedicamentoUpdateDto
{
    [StringLength(200, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 200 caracteres")]
    public string? Nombre { get; set; }
    
    [StringLength(200, MinimumLength = 2, ErrorMessage = "El fabricante debe tener entre 2 y 200 caracteres")]
    public string? Fabricante { get; set; }
    
    [StringLength(200, MinimumLength = 2, ErrorMessage = "La presentación debe tener entre 2 y 200 caracteres")]
    public string? Presentacion { get; set; }
    
    [StringLength(100, MinimumLength = 1, ErrorMessage = "El lote debe tener entre 1 y 100 caracteres")]
    public string? Lote { get; set; }
    
    public DateTime? FechaVencimiento { get; set; }
    
    [Range(0, 1000000, ErrorMessage = "El stock debe ser entre 0 y 1,000,000")]
    public int? Stock { get; set; }
    
    [Range(0, 1000000, ErrorMessage = "El stock mínimo debe ser entre 0 y 1,000,000")]
    public int? MinimoStock { get; set; }
    
    [Range(0, 999999.99, ErrorMessage = "El precio debe ser entre 0 y 999,999.99")]
    public double? Precio { get; set; }
    
    [StringLength(300, ErrorMessage = "El principio activo no puede exceder 300 caracteres")]
    public string? PrincipioActivo { get; set; }
}

/// <summary>
/// DTO reducido para búsquedas/autocompletar
/// </summary>
public class MedicamentoShortDto
{
    public string Id { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Presentacion { get; set; } = string.Empty;
    public string Fabricante { get; set; } = string.Empty;
    public string Lote { get; set; } = string.Empty;
}
