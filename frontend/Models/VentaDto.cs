using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FrontEndBlazor.Models;

/// <summary>
///son los enum de la venta
/// </summary>
public enum EstadoVenta
{
    PENDIENTE,
    CONFIRMADA,
    CANCELADA
}

public enum MetodoPago
{
    EFECTIVO,
    TARJETA,
    TRANSFERENCIA,
    OTRO
}

public enum MetodoDescuento
{
    FIFO,  //first In, First Out
    FEFO   //first Expired, First Out
}

//el backend espera valores "30", "60", "90" como strings
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PeriodoProyeccion
{
    [JsonPropertyName("30")]
    DIAS_30,
    
    [JsonPropertyName("60")]
    DIAS_60,
    
    [JsonPropertyName("90")]
    DIAS_90
}

/// <summary>
///dto completo
/// </summary>
public class VentaDto
{
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("numero_venta")]
    public string NumeroVenta { get; set; } = string.Empty;
    
    [JsonPropertyName("fecha_venta")]
    public DateTime FechaVenta { get; set; }
    
    public EstadoVenta Estado { get; set; }
    
    [JsonPropertyName("metodo_pago")]
    public MetodoPago? MetodoPago { get; set; }
    
    public decimal Total { get; set; }
    
    [JsonPropertyName("cliente_nombre")]
    public string? ClienteNombre { get; set; }
    
    [JsonPropertyName("cliente_documento")]
    public string? ClienteDocumento { get; set; }
    
    public string? Observaciones { get; set; }
    
    [JsonPropertyName("created_by")]
    public string? CreatedBy { get; set; }
    
    [JsonPropertyName("created_at")]
    public DateTime? CreatedAt { get; set; }
    
    [JsonPropertyName("confirmada_at")]
    public DateTime? ConfirmadaAt { get; set; }
    
    [JsonPropertyName("cancelada_at")]
    public DateTime? CanceladaAt { get; set; }
    
    public List<DetalleVentaDto> Detalles { get; set; } = new();
    
    //propiedades para la UI
    public string EstadoBadgeClass => Estado switch
    {
        EstadoVenta.PENDIENTE => "badge bg-warning text-dark",
        EstadoVenta.CONFIRMADA => "badge bg-success",
        EstadoVenta.CANCELADA => "badge bg-danger",
        _ => "badge bg-secondary"
    };
    
    public string MetodoPagoBadgeClass => MetodoPago switch
    {
        Models.MetodoPago.EFECTIVO => "badge bg-success",
        Models.MetodoPago.TARJETA => "badge bg-info",
        Models.MetodoPago.TRANSFERENCIA => "badge bg-primary",
        _ => "badge bg-secondary"
    };
}

/// <summary>
///dto detalles para la venta
/// </summary>
public class DetalleVentaDto
{
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("venta_id")]
    public string VentaId { get; set; } = string.Empty;
    
    [JsonPropertyName("medicamento_id")]
    public string MedicamentoId { get; set; } = string.Empty;
    
    public int Cantidad { get; set; }
    
    [JsonPropertyName("precio_unitario")]
    public decimal PrecioUnitario { get; set; }
    
    public decimal Subtotal { get; set; }
    
    public string? Lote { get; set; }
    
    //info del medicamento
    [JsonPropertyName("medicamento_nombre")]
    public string? MedicamentoNombre { get; set; }
    
    [JsonPropertyName("medicamento_fabricante")]
    public string? MedicamentoFabricante { get; set; }
    
    [JsonPropertyName("medicamento_presentacion")]
    public string? MedicamentoPresentacion { get; set; }
}

/// <summary>
///dto para crear detalle de venta
/// </summary>
public class DetalleVentaCreateDto
{
    [Required(ErrorMessage = "El medicamento es obligatorio")]
    [JsonPropertyName("medicamento_id")]
    public string MedicamentoId { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "La cantidad es obligatoria")]
    [Range(1, 10000, ErrorMessage = "La cantidad debe ser entre 1 y 10,000")]
    public int Cantidad { get; set; }
    
    [JsonPropertyName("precio_unitario")]
    public decimal? PrecioUnitario { get; set; }
}

/// <summary>
///dto para crear venta
/// </summary>
public class VentaCreateDto
{
    [Required(ErrorMessage = "Debe agregar al menos un producto")]
    [MinLength(1, ErrorMessage = "Debe agregar al menos un producto")]
    public List<DetalleVentaCreateDto> Detalles { get; set; } = new();
    
    [JsonPropertyName("metodo_pago")]
    public MetodoPago? MetodoPago { get; set; }
    
    [JsonPropertyName("cliente_nombre")]
    [StringLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres")]
    public string? ClienteNombre { get; set; }
    
    [JsonPropertyName("cliente_documento")]
    [StringLength(50, ErrorMessage = "El documento no puede exceder 50 caracteres")]
    public string? ClienteDocumento { get; set; }
    
    [StringLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres")]
    public string? Observaciones { get; set; }
    
    [JsonPropertyName("metodo_descuento")]
    public MetodoDescuento MetodoDescuento { get; set; } = MetodoDescuento.FEFO;
    
    [JsonPropertyName("confirmar_pago")]
    public bool ConfirmarPago { get; set; } = false;
}

/// <summary>
///dto para confirmar pago
/// </summary>
public class VentaConfirmarPagoDto
{
    [Required(ErrorMessage = "El método de pago es obligatorio")]
    [JsonPropertyName("metodo_pago")]
    public MetodoPago MetodoPago { get; set; }
    
    [JsonPropertyName("metodo_descuento")]
    public MetodoDescuento MetodoDescuento { get; set; } = MetodoDescuento.FEFO;
}

/// <summary>
///desglose de descuento
/// </summary>
public class DesgloseLoteDto
{
    [JsonPropertyName("medicamento_id")]
    public string MedicamentoId { get; set; } = string.Empty;
    
    public string Lote { get; set; } = string.Empty;
    
    [JsonPropertyName("cantidad_descontada")]
    public int CantidadDescontada { get; set; }
    
    [JsonPropertyName("stock_anterior")]
    public int StockAnterior { get; set; }
    
    [JsonPropertyName("stock_nuevo")]
    public int StockNuevo { get; set; }
    
    [JsonPropertyName("fecha_vencimiento")]
    public DateTime? FechaVencimiento { get; set; }
}

/// <summary>
/// confirmación de pago
/// </summary>
public class VentaConfirmarResponse
{
    public VentaDto Venta { get; set; } = new();
    
    [JsonPropertyName("desglose_descuento")]
    public List<DesgloseLoteDto> DesgloseDescuento { get; set; } = new();
    
    public string Mensaje { get; set; } = string.Empty;
}

/// <summary>
///filtros para el reporte de ventas
/// </summary>
public class FiltrosReporteVentas
{
    [Required(ErrorMessage = "La fecha de inicio es obligatoria")]
    [JsonPropertyName("fecha_inicio")]
    public DateTime FechaInicio { get; set; } = DateTime.Today.AddMonths(-1);
    
    [Required(ErrorMessage = "La fecha de fin es obligatoria")]
    [JsonPropertyName("fecha_fin")]
    public DateTime FechaFin { get; set; } = DateTime.Today;
    
    [JsonPropertyName("medicamento_id")]
    public string? MedicamentoId { get; set; }
    
    public EstadoVenta? Estado { get; set; }
}

/// <summary>
///ventas consolidadas por medicamento
/// </summary>
public class VentaPorMedicamento
{
    [JsonPropertyName("medicamento_id")]
    public string MedicamentoId { get; set; } = string.Empty;
    
    [JsonPropertyName("medicamento_nombre")]
    public string MedicamentoNombre { get; set; } = string.Empty;
    
    [JsonPropertyName("medicamento_fabricante")]
    public string MedicamentoFabricante { get; set; } = string.Empty;
    
    [JsonPropertyName("medicamento_presentacion")]
    public string MedicamentoPresentacion { get; set; } = string.Empty;
    
    [JsonPropertyName("total_unidades")]
    public int TotalUnidades { get; set; }
    
    [JsonPropertyName("total_ingresos")]
    public decimal TotalIngresos { get; set; }
    
    [JsonPropertyName("numero_ventas")]
    public int NumeroVentas { get; set; }
    
    [JsonPropertyName("precio_promedio")]
    public decimal PrecioPromedio { get; set; }
}

/// <summary>
///response del reporte de ventas
/// </summary>
public class ReporteVentasResponse
{
    [JsonPropertyName("fecha_inicio")]
    public DateTime FechaInicio { get; set; }
    
    [JsonPropertyName("fecha_fin")]
    public DateTime FechaFin { get; set; }
    
    [JsonPropertyName("total_ventas")]
    public int TotalVentas { get; set; }
    
    [JsonPropertyName("total_medicamentos")]
    public int TotalMedicamentos { get; set; }
    
    [JsonPropertyName("gran_total_ingresos")]
    public decimal GranTotalIngresos { get; set; }
    
    [JsonPropertyName("ventas_por_medicamento")]
    public List<VentaPorMedicamento> VentasPorMedicamento { get; set; } = new();
    
    public string? Mensaje { get; set; }
}

/// <summary>
///filtros para proyección de ventas
/// </summary>
public class FiltrosProyeccionVentas
{
    [JsonPropertyName("periodo_dias")]
    public string PeriodoDias { get; set; } = "90"; // Valor directo como string: "30", "60", "90"
    
    [JsonPropertyName("medicamento_id")]
    public string? MedicamentoId { get; set; }
    
    [JsonPropertyName("meses_historico")]
    [Range(6, 24, ErrorMessage = "El historial debe ser entre 6 y 24 meses")]
    public int MesesHistorico { get; set; } = 12;
}

/// <summary>
///proyección de demanda para un medicamento
/// </summary>
public class ProyeccionMedicamento
{
    [JsonPropertyName("medicamento_id")]
    public string MedicamentoId { get; set; } = string.Empty;
    
    [JsonPropertyName("medicamento_nombre")]
    public string MedicamentoNombre { get; set; } = string.Empty;
    
    [JsonPropertyName("medicamento_fabricante")]
    public string MedicamentoFabricante { get; set; } = string.Empty;
    
    [JsonPropertyName("medicamento_presentacion")]
    public string MedicamentoPresentacion { get; set; } = string.Empty;
    
    [JsonPropertyName("promedio_mensual")]
    public decimal PromedioMensual { get; set; }
    
    [JsonPropertyName("total_historico")]
    public int TotalHistorico { get; set; }
    
    [JsonPropertyName("meses_con_datos")]
    public int MesesConDatos { get; set; }
    
    [JsonPropertyName("demanda_proyectada")]
    public decimal DemandaProyectada { get; set; }
    
    [JsonPropertyName("stock_actual")]
    public int StockActual { get; set; }
    
    [JsonPropertyName("stock_recomendado")]
    public int StockRecomendado { get; set; }
    
    [JsonPropertyName("requiere_reposicion")]
    public bool RequiereReposicion { get; set; }
    
    public string Tendencia { get; set; } = string.Empty;
    
    public string Confianza { get; set; } = string.Empty;
    
    //propiedades computadas para UI
    public string TendenciaIcon => Tendencia.ToUpper() switch
    {
        "CRECIENTE" => "📈",
        "DECRECIENTE" => "📉",
        "ESTABLE" => "➡️",
        _ => "❓"
    };
    
    public string TendenciaBadgeClass => Tendencia.ToUpper() switch
    {
        "CRECIENTE" => "badge bg-success",
        "DECRECIENTE" => "badge bg-danger",
        "ESTABLE" => "badge bg-info",
        _ => "badge bg-secondary"
    };
    
    public string ConfianzaBadgeClass => Confianza.ToUpper() switch
    {
        "ALTA" => "badge bg-success",
        "MEDIA" => "badge bg-warning text-dark",
        "BAJA" => "badge bg-danger",
        _ => "badge bg-secondary"
    };
}

/// <summary>
///response de proyección de ventas
/// </summary>
public class ProyeccionVentasResponse
{
    [JsonPropertyName("fecha_corte")]
    public DateTime FechaCorte { get; set; }
    
    [JsonPropertyName("periodo_proyeccion_dias")]
    public int PeriodoProyeccionDias { get; set; }
    
    [JsonPropertyName("meses_historial")]
    public int MesesHistorial { get; set; }
    
    public List<ProyeccionMedicamento> Proyecciones { get; set; } = new();
    
    public string? Mensaje { get; set; }
    
    public List<string> Advertencias { get; set; } = new();
}

/// <summary>
///estadísticas generales de ventas
/// </summary>
public class EstadisticasVentas
{
    [JsonPropertyName("total_ventas_confirmadas")]
    public int TotalVentasConfirmadas { get; set; }
    
    [JsonPropertyName("total_ventas_pendientes")]
    public int TotalVentasPendientes { get; set; }
    
    [JsonPropertyName("total_ingresos")]
    public decimal TotalIngresos { get; set; }
    
    [JsonPropertyName("medicamento_mas_vendido")]
    public string? MedicamentoMasVendido { get; set; }
    
    [JsonPropertyName("promedio_venta")]
    public decimal PromedioVenta { get; set; }
    
    [JsonPropertyName("periodo_analizado")]
    public string PeriodoAnalizado { get; set; } = string.Empty;
}
