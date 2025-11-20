using System.ComponentModel.DataAnnotations;

namespace FrontEndBlazor.Models;

/// <summary>
/// DTO para crear una venta
/// </summary>
public class VentaCreateDto
{
    [Required(ErrorMessage = "Debe agregar al menos un medicamento")]
    [MinLength(1, ErrorMessage = "Debe agregar al menos un medicamento")]
    public List<DetalleVentaCreateDto> Detalles { get; set; } = new();
    
    [StringLength(200, ErrorMessage = "El cliente no puede exceder 200 caracteres")]
    public string? Cliente { get; set; }
    
    [StringLength(500, ErrorMessage = "Las notas no pueden exceder 500 caracteres")]
    public string? Notas { get; set; }
}

/// <summary>
/// DTO para detalle de venta al crear
/// </summary>
public class DetalleVentaCreateDto
{
    [Required(ErrorMessage = "El medicamento es requerido")]
    public string MedicamentoId { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "La cantidad es requerida")]
    [Range(1, 1000, ErrorMessage = "La cantidad debe ser entre 1 y 1000")]
    public int Cantidad { get; set; }
    
    [Range(0.01, 999999.99, ErrorMessage = "El precio debe ser mayor a 0")]
    public double? PrecioUnitario { get; set; }
}

/// <summary>
/// DTO completo de venta (lectura desde API)
/// </summary>
public class VentaDto
{
    public string Id { get; set; } = string.Empty;
    public string UsuarioId { get; set; } = string.Empty;
    public string UsuarioNombre { get; set; } = string.Empty;
    public double Total { get; set; }
    public DateTime Fecha { get; set; }
    public string? Cliente { get; set; }
    public string? Notas { get; set; }
    public List<DetalleVentaDto> Detalles { get; set; } = new();
    
    // Propiedades computadas para UI
    public string FechaFormateada => Fecha.ToString("dd/MM/yyyy HH:mm");
    public string TotalFormateado => Total.ToString("C2");
}

/// <summary>
/// DTO para detalle de venta (lectura)
/// </summary>
public class DetalleVentaDto
{
    public string Id { get; set; } = string.Empty;
    public string MedicamentoId { get; set; } = string.Empty;
    public string MedicamentoNombre { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public double PrecioUnitario { get; set; }
    public double Subtotal { get; set; }
    
    // Propiedades computadas para UI
    public string SubtotalFormateado => Subtotal.ToString("C2");
    public string PrecioUnitarioFormateado => PrecioUnitario.ToString("C2");
}

/// <summary>
/// DTO para reporte de ventas por período
/// </summary>
public class ReporteVentaPeriodoDto
{
    [Required(ErrorMessage = "La fecha de inicio es requerida")]
    public DateTime FechaInicio { get; set; } = DateTime.Today.AddDays(-30);
    
    [Required(ErrorMessage = "La fecha de fin es requerida")]
    public DateTime FechaFin { get; set; } = DateTime.Today;
}

/// <summary>
/// DTO para respuesta de reporte completo
/// </summary>
public class ReporteCompletoDto
{
    public string Periodo { get; set; } = string.Empty;
    public double TotalVentas { get; set; }
    public int CantidadVentas { get; set; }
    public double VentasPromedio { get; set; }
    public List<ProductoMasVendidoDto> ProductosMasVendidos { get; set; } = new();
    public List<VentasPorDiaDto> VentasPorDia { get; set; } = new();
    
    // Propiedades computadas para UI
    public string TotalVentasFormateado => TotalVentas.ToString("C2");
    public string VentasPromedioFormateado => VentasPromedio.ToString("C2");
}

/// <summary>
/// DTO para producto más vendido en reportes
/// </summary>
public class ProductoMasVendidoDto
{
    public string MedicamentoId { get; set; } = string.Empty;
    public string MedicamentoNombre { get; set; } = string.Empty;
    public int CantidadVendida { get; set; }
    public double TotalVentas { get; set; }
    
    // Propiedades computadas para UI
    public string TotalVentasFormateado => TotalVentas.ToString("C2");
}

/// <summary>
/// DTO para ventas por día en reportes
/// </summary>
public class VentasPorDiaDto
{
    public string Fecha { get; set; } = string.Empty;
    public double TotalVentas { get; set; }
    public int CantidadVentas { get; set; }
    
    // Propiedades computadas para UI
    public string TotalVentasFormateado => TotalVentas.ToString("C2");
    public DateTime FechaDateTime => DateTime.Parse(Fecha);
    public string FechaCorta => FechaDateTime.ToString("dd/MM");
}

/// <summary>
/// DTO para proyección de demanda
/// </summary>
public class ProyeccionDemandaDto
{
    public string PeriodoAnalizado { get; set; } = string.Empty;
    public int DiasProyeccion { get; set; }
    public List<ProyeccionProductoDto> Proyecciones { get; set; } = new();
}

/// <summary>
/// DTO para proyección por producto
/// </summary>
public class ProyeccionProductoDto
{
    public string MedicamentoId { get; set; } = string.Empty;
    public string MedicamentoNombre { get; set; } = string.Empty;
    public int VentasUltimos90Dias { get; set; }
    public double PromedioDiario { get; set; }
    public int DemandaProyectada30Dias { get; set; }
    public int NivelReordenRecomendado { get; set; }
}