using System.Text.Json.Serialization;

namespace FrontEndBlazor.Models;

/// <summary>
/// Enums para alertas (deben coincidir con el backend)
/// </summary>
public enum TipoAlerta
{
    STOCK_MINIMO,
    STOCK_CRITICO,
    STOCK_AGOTADO,
    VENCIMIENTO_PROXIMO,
    VENCIMIENTO_INMEDIATO,
    VENCIDO,
    ORDEN_RETRASADA
}

public enum EstadoAlerta
{
    ACTIVA,
    PENDIENTE_REPOSICION,
    RESUELTA
}

public enum PrioridadAlerta
{
    BAJA,
    MEDIA,
    ALTA,
    CRITICA
}

/// <summary>
/// DTO de alerta con información del medicamento
/// </summary>
public class AlertaDto
{
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("medicamento_id")]
    public string MedicamentoId { get; set; } = string.Empty;
    
    public TipoAlerta Tipo { get; set; }
    
    public EstadoAlerta Estado { get; set; }
    
    public PrioridadAlerta Prioridad { get; set; }
    
    public string Mensaje { get; set; } = string.Empty;
    
    // Datos específicos de stock
    [JsonPropertyName("stock_actual")]
    public int? StockActual { get; set; }
    
    [JsonPropertyName("stock_minimo")]
    public int? StockMinimo { get; set; }
    
    // Datos específicos de vencimiento
    [JsonPropertyName("fecha_vencimiento")]
    public DateTime? FechaVencimiento { get; set; }
    
    [JsonPropertyName("dias_restantes")]
    public int? DiasRestantes { get; set; }
    
    public string? Lote { get; set; }
    
    // Información del medicamento
    [JsonPropertyName("medicamento_nombre")]
    public string MedicamentoNombre { get; set; } = string.Empty;
    
    [JsonPropertyName("medicamento_fabricante")]
    public string MedicamentoFabricante { get; set; } = string.Empty;
    
    [JsonPropertyName("medicamento_presentacion")]
    public string MedicamentoPresentacion { get; set; } = string.Empty;
    
    [JsonPropertyName("medicamento_lote")]
    public string MedicamentoLote { get; set; } = string.Empty;
    
    // Metadatos adicionales (JSON genérico para información extra)
    public Dictionary<string, object>? Metadatos { get; set; }
    
    // Auditoría
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
    
    [JsonPropertyName("updated_at")]
    public DateTime? UpdatedAt { get; set; }
    
    [JsonPropertyName("resuelta_at")]
    public DateTime? ResueltaAt { get; set; }
    
    [JsonPropertyName("resuelta_by")]
    public string? ResueltaBy { get; set; }
    
    public bool Notificada { get; set; }
    
    [JsonPropertyName("notificada_at")]
    public DateTime? NotificadaAt { get; set; }
    
    // Propiedades computadas para UI
    public string TipoIcon => Tipo switch
    {
        TipoAlerta.STOCK_AGOTADO => "📦",
        TipoAlerta.STOCK_CRITICO => "⚠️",
        TipoAlerta.STOCK_MINIMO => "📉",
        TipoAlerta.VENCIDO => "❌",
        TipoAlerta.VENCIMIENTO_INMEDIATO => "⏰",
        TipoAlerta.VENCIMIENTO_PROXIMO => "📅",
        TipoAlerta.ORDEN_RETRASADA => "🚚",
        _ => "🔔"
    };
    
    public string PrioridadBadgeClass => Prioridad switch
    {
        PrioridadAlerta.CRITICA => "badge bg-danger",
        PrioridadAlerta.ALTA => "badge bg-warning text-dark",
        PrioridadAlerta.MEDIA => "badge bg-info",
        PrioridadAlerta.BAJA => "badge bg-secondary",
        _ => "badge bg-secondary"
    };
    
    public string EstadoBadgeClass => Estado switch
    {
        EstadoAlerta.ACTIVA => "badge bg-danger",
        EstadoAlerta.PENDIENTE_REPOSICION => "badge bg-warning text-dark",
        EstadoAlerta.RESUELTA => "badge bg-success",
        _ => "badge bg-secondary"
    };
    
    public string TipoLabel => Tipo switch
    {
        TipoAlerta.STOCK_AGOTADO => "Stock Agotado",
        TipoAlerta.STOCK_CRITICO => "Stock Crítico",
        TipoAlerta.STOCK_MINIMO => "Stock Mínimo",
        TipoAlerta.VENCIDO => "Vencido",
        TipoAlerta.VENCIMIENTO_INMEDIATO => "Vence Pronto",
        TipoAlerta.VENCIMIENTO_PROXIMO => "Próximo a Vencer",
        TipoAlerta.ORDEN_RETRASADA => "Orden Retrasada",
        _ => "Alerta"
    };
    
    public string TiempoTranscurrido
    {
        get
        {
            var diff = DateTime.Now - CreatedAt;
            if (diff.TotalMinutes < 1) return "Hace un momento";
            if (diff.TotalMinutes < 60) return $"Hace {(int)diff.TotalMinutes} min";
            if (diff.TotalHours < 24) return $"Hace {(int)diff.TotalHours}h";
            return $"Hace {(int)diff.TotalDays} días";
        }
    }
    
    // Propiedades helper para alertas de órdenes retrasadas
    public string? NumeroOrden => Metadatos?.GetValueOrDefault("numero_orden")?.ToString();
    public string? ProveedorNombre => Metadatos?.GetValueOrDefault("proveedor_nombre")?.ToString();
    
    public int? DiasRetraso
    {
        get
        {
            if (Metadatos?.GetValueOrDefault("dias_retraso") is object diasObj)
            {
                // Manejar JsonElement de System.Text.Json
                if (diasObj is System.Text.Json.JsonElement jsonElement)
                {
                    if (jsonElement.ValueKind == System.Text.Json.JsonValueKind.Number)
                    {
                        return jsonElement.GetInt32();
                    }
                }
                // Fallback para otros tipos
                else if (diasObj is int intValue)
                {
                    return intValue;
                }
                else
                {
                    try
                    {
                        return Convert.ToInt32(diasObj);
                    }
                    catch
                    {
                        return null;
                    }
                }
            }
            return null;
        }
    }
    
    public decimal? TotalEstimado
    {
        get
        {
            if (Metadatos?.GetValueOrDefault("total_estimado") is object totalObj)
            {
                // Manejar JsonElement de System.Text.Json
                if (totalObj is System.Text.Json.JsonElement jsonElement)
                {
                    if (jsonElement.ValueKind == System.Text.Json.JsonValueKind.Number)
                    {
                        return jsonElement.GetDecimal();
                    }
                }
                // Fallback para otros tipos
                else if (totalObj is decimal decValue)
                {
                    return decValue;
                }
                else
                {
                    try
                    {
                        return Convert.ToDecimal(totalObj);
                    }
                    catch
                    {
                        return null;
                    }
                }
            }
            return null;
        }
    }
}

/// <summary>
/// DTO para notificaciones de alertas
/// </summary>
public class NotificacionDto
{
    [JsonPropertyName("alert_id")]
    public string AlertId { get; set; } = string.Empty;
    
    [JsonPropertyName("event_type")]
    public string EventType { get; set; } = string.Empty; // 'created', 'updated', 'resolved'
    
    [JsonPropertyName("alert_type")]
    public string AlertType { get; set; } = string.Empty;
    
    public string Priority { get; set; } = string.Empty;
    
    [JsonPropertyName("medicamento_nombre")]
    public string MedicamentoNombre { get; set; } = string.Empty;
    
    [JsonPropertyName("medicamento_fabricante")]
    public string MedicamentoFabricante { get; set; } = string.Empty;
    
    [JsonPropertyName("medicamento_presentacion")]
    public string MedicamentoPresentacion { get; set; } = string.Empty;
    
    [JsonPropertyName("medicamento_lote")]
    public string MedicamentoLote { get; set; } = string.Empty;
    
    public string Mensaje { get; set; } = string.Empty;
    
    public string Timestamp { get; set; } = string.Empty;
    
    // Propiedades computadas
    public string EventIcon => EventType switch
    {
        "created" => "🔔",
        "updated" => "🔄",
        "resolved" => "✅",
        _ => "📌"
    };
    
    public string EventLabel => EventType switch
    {
        "created" => "Nueva alerta",
        "updated" => "Alerta actualizada",
        "resolved" => "Alerta resuelta",
        _ => "Notificación"
    };
}

/// <summary>
/// Estadísticas del dashboard de alertas
/// </summary>
public class AlertaStatsDto
{
    [JsonPropertyName("total_activas")]
    public int TotalActivas { get; set; }
    
    [JsonPropertyName("por_tipo")]
    public Dictionary<string, int> PorTipo { get; set; } = new();
    
    [JsonPropertyName("por_prioridad")]
    public Dictionary<string, int> PorPrioridad { get; set; } = new();
    
    [JsonPropertyName("por_estado")]
    public Dictionary<string, int> PorEstado { get; set; } = new();
    
    [JsonPropertyName("creadas_hoy")]
    public int CreadasHoy { get; set; }
    
    [JsonPropertyName("resueltas_hoy")]
    public int ResueltasHoy { get; set; }
}

/// <summary>
/// DTO para actualizar estado de alerta
/// </summary>
public class AlertaUpdateEstadoDto
{
    public EstadoAlerta Estado { get; set; }
    
    public string? Notas { get; set; }
}
