using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FrontEndBlazor.Models;

/// <summary>
/// DTO completo de orden de compra (lectura desde API)
/// HU-4.02: Post-Orden
/// </summary>
public class OrdenCompraDto
{
    public string Id { get; set; } = string.Empty;
    
    public string NumeroOrden { get; set; } = string.Empty;
    
    public string ProveedorId { get; set; } = string.Empty;
    
    public string? ProveedorNombre { get; set; }
    
    public string? ProveedorNit { get; set; }
    
    public DateTime FechaCreacion { get; set; }
    
    public DateTime FechaPrevistaEntrega { get; set; }
    
    public DateTime? FechaEnvio { get; set; }
    
    public DateTime? FechaRecepcion { get; set; }
    
    public string Estado { get; set; } = "PENDIENTE";
    
    public string? Observaciones { get; set; }
    
    public decimal TotalEstimado { get; set; }
    
    public string? CreatedBy { get; set; }
    
    public string? RecibidoBy { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    public List<DetalleOrdenDto> Detalles { get; set; } = new();
    
    public int? DiasHastaEntrega { get; set; }
    
    public bool EstaRetrasada { get; set; }
    
    // Propiedades computadas para UI
    public string EstadoBadgeClass => Estado switch
    {
        "PENDIENTE" => "badge bg-secondary",
        "ENVIADA" => "badge bg-info",
        "RECIBIDA" => "badge bg-success",
        "RETRASADA" => "badge bg-danger",
        _ => "badge bg-secondary"
    };
    
    public string EstadoIcon => Estado switch
    {
        "PENDIENTE" => "bi-clock-history",
        "ENVIADA" => "bi-box-seam",
        "RECIBIDA" => "bi-check-circle-fill",
        "RETRASADA" => "bi-exclamation-triangle-fill",
        _ => "bi-question-circle"
    };
    
    public bool PuedeEditar => Estado == "PENDIENTE";
    
    public bool PuedeEnviar => Estado == "PENDIENTE";
    
    public bool PuedeRecibir => Estado == "ENVIADA" || Estado == "RETRASADA";
}

/// <summary>
/// DTO de detalle de orden (item/producto) - LECTURA desde API
/// Incluye LoteEsperado y FechaVencimientoEsperada auto-llenados por el backend
/// </summary>
public class DetalleOrdenDto
{
    public string Id { get; set; } = string.Empty;
    
    public string OrdenCompraId { get; set; } = string.Empty;
    
    public string MedicamentoId { get; set; } = string.Empty;
    
    public string? MedicamentoNombre { get; set; }
    
    public string? MedicamentoFabricante { get; set; }
    
    public string? MedicamentoPresentacion { get; set; }
    
    public int CantidadSolicitada { get; set; }
    
    public int CantidadRecibida { get; set; }
    
    public decimal PrecioUnitario { get; set; }
    
    public decimal Subtotal { get; set; }
    
    // ✅ Estos campos se AUTO-LLENAN desde el medicamento en el backend
    public string? LoteEsperado { get; set; }
    
    public DateTime? FechaVencimientoEsperada { get; set; }
    
    // Propiedades computadas
    public bool TieneDiferencia => CantidadRecibida != CantidadSolicitada && CantidadRecibida > 0;
    
    public int Diferencia => CantidadRecibida - CantidadSolicitada;
}

/// <summary>
/// DTO para crear orden de compra
/// </summary>
public class OrdenCompraCreateDto
{
    [Required(ErrorMessage = "El proveedor es obligatorio")]
    public string ProveedorId { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "La fecha prevista de entrega es obligatoria")]
    public DateTime FechaPrevistaEntrega { get; set; } = DateTime.Today.AddDays(7);
    
    [StringLength(1000, ErrorMessage = "Las observaciones no pueden exceder 1000 caracteres")]
    public string? Observaciones { get; set; }
    
    [Required(ErrorMessage = "Debe agregar al menos un producto")]
    [MinLength(1, ErrorMessage = "Debe agregar al menos un producto")]
    public List<DetalleOrdenCreateDto> Detalles { get; set; } = new();
}

/// <summary>
/// DTO para crear detalle de orden (item/producto)
/// ⚠️ IMPORTANTE: NO incluye LoteEsperado ni FechaVencimientoEsperada
/// El backend los auto-llena desde el medicamento automáticamente
/// </summary>
public class DetalleOrdenCreateDto
{
    [Required(ErrorMessage = "El medicamento es obligatorio")]
    public string MedicamentoId { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "La cantidad es obligatoria")]
    [Range(1, 100000, ErrorMessage = "La cantidad debe ser mayor a 0")]
    public int CantidadSolicitada { get; set; }
    
    [Required(ErrorMessage = "El precio unitario es obligatorio")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
    public decimal PrecioUnitario { get; set; }
    
    // ❌ ELIMINADO: LoteEsperado (auto-llenado por backend desde medicamento)
    // ❌ ELIMINADO: FechaVencimientoEsperada (auto-llenado por backend desde medicamento)
    
    // Para mostrar en UI (no se envía al backend)
    [JsonIgnore]
    public string? MedicamentoNombre { get; set; }
    
    [JsonIgnore]
    public string? MedicamentoFabricante { get; set; }
    
    [JsonIgnore]
    public string? MedicamentoPresentacion { get; set; }
    
    [JsonIgnore]
    public decimal Subtotal => CantidadSolicitada * PrecioUnitario;
}

/// <summary>
/// DTO para actualizar orden (solo en estado PENDIENTE)
/// </summary>
public class OrdenCompraUpdateDto
{
    public string? ProveedorId { get; set; }
    
    public DateTime? FechaPrevistaEntrega { get; set; }
    
    [StringLength(1000, ErrorMessage = "Las observaciones no pueden exceder 1000 caracteres")]
    public string? Observaciones { get; set; }
}

/// <summary>
/// DTO para marcar orden como enviada
/// </summary>
public class MarcarEnviadaDto
{
    public DateTime? FechaEnvio { get; set; }
    
    [StringLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres")]
    public string? Observaciones { get; set; }
}

/// <summary>
/// DTO para recibir orden
/// </summary>
public class RecepcionOrdenDto
{
    [Required]
    [MinLength(1, ErrorMessage = "Debe especificar al menos un item recibido")]
    public List<RecepcionItemDto> Items { get; set; } = new();
    
    public DateTime? FechaRecepcion { get; set; }
    
    [StringLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres")]
    public string? Observaciones { get; set; }
    
    public bool ActualizarInventario { get; set; } = true;
}

/// <summary>
/// DTO para item recibido
/// </summary>
public class RecepcionItemDto
{
    [Required]
    public string DetalleId { get; set; } = string.Empty;
    
    [Range(0, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor o igual a 0")]
    public int CantidadRecibida { get; set; }
}

/// <summary>
/// Response de recepción
/// </summary>
public class RecepcionOrdenResponse
{
    public string OrdenId { get; set; } = string.Empty;
    
    public string NumeroOrden { get; set; } = string.Empty;
    
    public string Estado { get; set; } = string.Empty;
    
    public int ItemsRecibidos { get; set; }
    
    public List<DiferenciaItemDto> ItemsConDiferencias { get; set; } = new();
    
    public bool InventarioActualizado { get; set; }
    
    public string Mensaje { get; set; } = string.Empty;
}

/// <summary>
/// Diferencia en cantidad recibida
/// </summary>
public class DiferenciaItemDto
{
    public string DetalleId { get; set; } = string.Empty;
    
    public string Medicamento { get; set; } = string.Empty;
    
    public int Solicitada { get; set; }
    
    public int Recibida { get; set; }
    
    public int Diferencia { get; set; }
}

/// <summary>
/// DTO resumido para listas
/// </summary>
public class OrdenCompraShortDto
{
    public string Id { get; set; } = string.Empty;
    
    public string NumeroOrden { get; set; } = string.Empty;
    
    public string ProveedorNombre { get; set; } = string.Empty;
    
    public DateTime FechaPrevistaEntrega { get; set; }
    
    public string Estado { get; set; } = "PENDIENTE";
    
    public decimal TotalEstimado { get; set; }
}

/// <summary>
/// Estadísticas de órdenes
/// </summary>
public class OrdenCompraStatsDto
{
    public int Total { get; set; }
    
    public int Pendientes { get; set; }
    
    public int Enviadas { get; set; }
    
    public int Recibidas { get; set; }
    
    public int Retrasadas { get; set; }
}
