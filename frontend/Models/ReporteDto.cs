using System.Text.Json.Serialization;

namespace FrontEndBlazor.Models;

/// <summary>
/// Request para filtros de reportes
/// </summary>
public class FiltrosReporteRequest
{
    [JsonPropertyName("fecha_inicio")]
    public DateOnly FechaInicio { get; set; }
    
    [JsonPropertyName("fecha_fin")]
    public DateOnly FechaFin { get; set; }
    
    [JsonPropertyName("proveedor_id")]
    public string? ProveedorId { get; set; }
    
    [JsonPropertyName("medicamento_id")]
    public string? MedicamentoId { get; set; }
}

/// <summary>
/// Datos de un proveedor en la comparación de precios
/// </summary>
public class ComparacionProveedorPrecio
{
    [JsonPropertyName("proveedor_id")]
    public string ProveedorId { get; set; } = string.Empty;
    
    [JsonPropertyName("proveedor_nombre")]
    public string ProveedorNombre { get; set; } = string.Empty;
    
    [JsonPropertyName("proveedor_nit")]
    public string ProveedorNit { get; set; } = string.Empty;
    
    [JsonPropertyName("total_unidades_compradas")]
    public int TotalUnidadesCompradas { get; set; }
    
    [JsonPropertyName("total_dinero_invertido")]
    public decimal TotalDineroInvertido { get; set; }
    
    [JsonPropertyName("precio_promedio")]
    public decimal PrecioPromedio { get; set; }
    
    [JsonPropertyName("numero_ordenes")]
    public int NumeroOrdenes { get; set; }
}

/// <summary>
/// Comparación de precios por medicamento
/// </summary>
public class ComparacionPorMedicamento
{
    [JsonPropertyName("medicamento_id")]
    public string MedicamentoId { get; set; } = string.Empty;
    
    [JsonPropertyName("medicamento_nombre")]
    public string MedicamentoNombre { get; set; } = string.Empty;
    
    [JsonPropertyName("medicamento_fabricante")]
    public string MedicamentoFabricante { get; set; } = string.Empty;
    
    [JsonPropertyName("medicamento_presentacion")]
    public string MedicamentoPresentacion { get; set; } = string.Empty;
    
    [JsonPropertyName("proveedores")]
    public List<ComparacionProveedorPrecio> Proveedores { get; set; } = new();
    
    [JsonPropertyName("mejor_precio_proveedor_id")]
    public string? MejorPrecioProveedorId { get; set; }
    
    [JsonPropertyName("mejor_precio")]
    public decimal? MejorPrecio { get; set; }
}

/// <summary>
/// Response de comparación de precios
/// </summary>
public class ComparacionPreciosResponse
{
    [JsonPropertyName("fecha_inicio")]
    public DateOnly FechaInicio { get; set; }
    
    [JsonPropertyName("fecha_fin")]
    public DateOnly FechaFin { get; set; }
    
    [JsonPropertyName("total_medicamentos")]
    public int TotalMedicamentos { get; set; }
    
    [JsonPropertyName("total_proveedores")]
    public int TotalProveedores { get; set; }
    
    [JsonPropertyName("comparaciones")]
    public List<ComparacionPorMedicamento> Comparaciones { get; set; } = new();
    
    [JsonPropertyName("mensaje")]
    public string? Mensaje { get; set; }
}

/// <summary>
/// Detalle de compras por medicamento y proveedor
/// </summary>
public class ReporteCompraDetalle
{
    [JsonPropertyName("medicamento_id")]
    public string MedicamentoId { get; set; } = string.Empty;
    
    [JsonPropertyName("medicamento_nombre")]
    public string MedicamentoNombre { get; set; } = string.Empty;
    
    [JsonPropertyName("medicamento_fabricante")]
    public string MedicamentoFabricante { get; set; } = string.Empty;
    
    [JsonPropertyName("medicamento_presentacion")]
    public string MedicamentoPresentacion { get; set; } = string.Empty;
    
    [JsonPropertyName("proveedor_id")]
    public string ProveedorId { get; set; } = string.Empty;
    
    [JsonPropertyName("proveedor_nombre")]
    public string ProveedorNombre { get; set; } = string.Empty;
    
    [JsonPropertyName("proveedor_nit")]
    public string ProveedorNit { get; set; } = string.Empty;
    
    [JsonPropertyName("total_unidades_compradas")]
    public int TotalUnidadesCompradas { get; set; }
    
    [JsonPropertyName("total_dinero_invertido")]
    public decimal TotalDineroInvertido { get; set; }
    
    [JsonPropertyName("numero_ordenes")]
    public int NumeroOrdenes { get; set; }
    
    [JsonPropertyName("precio_promedio")]
    public decimal PrecioPromedio { get; set; }
}

/// <summary>
/// Totales consolidados por proveedor
/// </summary>
public class ReporteTotalesPorProveedor
{
    [JsonPropertyName("proveedor_id")]
    public string ProveedorId { get; set; } = string.Empty;
    
    [JsonPropertyName("proveedor_nombre")]
    public string ProveedorNombre { get; set; } = string.Empty;
    
    [JsonPropertyName("proveedor_nit")]
    public string ProveedorNit { get; set; } = string.Empty;
    
    [JsonPropertyName("total_ordenes")]
    public int TotalOrdenes { get; set; }
    
    [JsonPropertyName("total_items")]
    public int TotalItems { get; set; }
    
    [JsonPropertyName("total_invertido")]
    public decimal TotalInvertido { get; set; }
}

/// <summary>
/// Response del reporte de compras
/// </summary>
public class ReporteComprasResponse
{
    [JsonPropertyName("fecha_inicio")]
    public DateOnly FechaInicio { get; set; }
    
    [JsonPropertyName("fecha_fin")]
    public DateOnly FechaFin { get; set; }
    
    [JsonPropertyName("total_ordenes")]
    public int TotalOrdenes { get; set; }
    
    [JsonPropertyName("total_proveedores")]
    public int TotalProveedores { get; set; }
    
    [JsonPropertyName("total_medicamentos")]
    public int TotalMedicamentos { get; set; }
    
    [JsonPropertyName("gran_total_invertido")]
    public decimal GranTotalInvertido { get; set; }
    
    [JsonPropertyName("detalles")]
    public List<ReporteCompraDetalle> Detalles { get; set; } = new();
    
    [JsonPropertyName("totales_por_proveedor")]
    public List<ReporteTotalesPorProveedor> TotalesPorProveedor { get; set; } = new();
    
    [JsonPropertyName("mensaje")]
    public string? Mensaje { get; set; }
}
