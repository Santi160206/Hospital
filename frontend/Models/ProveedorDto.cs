using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FrontEndBlazor.Models;

/// <summary>
/// DTO completo de proveedor (lectura desde API)
/// </summary>
public class ProveedorDto
{
    public string Id { get; set; } = string.Empty;
    
    public string Nit { get; set; } = string.Empty;
    
    public string Nombre { get; set; } = string.Empty;
    
    public string? Telefono { get; set; }
    
    public string? Email { get; set; }
    
    public string? Direccion { get; set; }
    
    public string Estado { get; set; } = "ACTIVO";
    
    public string? CreatedBy { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public string? UpdatedBy { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    // Propiedades computadas para UI
    public bool EstaActivo => Estado == "ACTIVO";
    
    public string EstadoBadgeClass => EstaActivo ? "badge bg-success" : "badge bg-secondary";
}

/// <summary>
/// DTO para crear proveedor
/// </summary>
public class ProveedorCreateDto
{
    [Required(ErrorMessage = "El NIT es obligatorio")]
    [StringLength(50, MinimumLength = 5, ErrorMessage = "El NIT debe tener entre 5 y 50 caracteres")]
    [RegularExpression(@"^\d+(-\d)?$", ErrorMessage = "El NIT debe contener solo números y opcionalmente un guión con dígito verificador (ej: 123456789-0)")]
    public string Nit { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "El nombre debe tener entre 1 y 200 caracteres")]
    public string Nombre { get; set; } = string.Empty;
    
    [StringLength(50, ErrorMessage = "El teléfono no puede exceder 50 caracteres")]
    [Phone(ErrorMessage = "El teléfono debe tener un formato válido")]
    public string? Telefono { get; set; }
    
    [EmailAddress(ErrorMessage = "El email debe tener un formato válido")]
    [StringLength(200, ErrorMessage = "El email no puede exceder 200 caracteres")]
    public string? Email { get; set; }
    
    [StringLength(500, ErrorMessage = "La dirección no puede exceder 500 caracteres")]
    public string? Direccion { get; set; }
}

/// <summary>
/// DTO para actualizar proveedor (todos los campos opcionales excepto NIT e ID)
/// </summary>
public class ProveedorUpdateDto
{
    [StringLength(200, MinimumLength = 1, ErrorMessage = "El nombre debe tener entre 1 y 200 caracteres")]
    public string? Nombre { get; set; }
    
    [StringLength(50, ErrorMessage = "El teléfono no puede exceder 50 caracteres")]
    public string? Telefono { get; set; }
    
    [EmailAddress(ErrorMessage = "El email debe tener un formato válido")]
    [StringLength(200, ErrorMessage = "El email no puede exceder 200 caracteres")]
    public string? Email { get; set; }
    
    [StringLength(500, ErrorMessage = "La dirección no puede exceder 500 caracteres")]
    public string? Direccion { get; set; }
    
    public string? Estado { get; set; }
}

/// <summary>
/// DTO reducido para búsquedas/autocompletar
/// </summary>
public class ProveedorShortDto
{
    public string Id { get; set; } = string.Empty;
    public string Nit { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Estado { get; set; } = "ACTIVO";
}

/// <summary>
/// Response de estadísticas
/// </summary>
public class ProveedorStatsDto
{
    public int Total { get; set; }
    public int Activos { get; set; }
    public int Inactivos { get; set; }
}
