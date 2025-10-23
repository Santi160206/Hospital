namespace FrontEndBlazor.Models;

/// <summary>
/// DTO de log de auditoría (lectura desde API)
/// </summary>
public class AuditLogDto
{
    public string Id { get; set; } = string.Empty;
    
    public string Entidad { get; set; } = string.Empty;
    
    public string EntidadId { get; set; } = string.Empty;
    
    public string? UsuarioId { get; set; }
    
    public string Accion { get; set; } = string.Empty;
    
    public string? Campo { get; set; }
    
    public string? ValorAnterior { get; set; }
    
    public string? ValorNuevo { get; set; }
    
    public object? Metadatos { get; set; }
    
    public DateTime Timestamp { get; set; }
    
    // Propiedades computadas para UI
    public string AccionBadgeClass => Accion switch
    {
        "CREATE" => "badge bg-success",
        "UPDATE" => "badge bg-info",
        "DELETE_SOFT" => "badge bg-danger",
        "DEACTIVATE" => "badge bg-warning",
        "REACTIVATE" => "badge bg-primary",
        _ => "badge bg-secondary"
    };
    
    public string AccionTexto => Accion switch
    {
        "CREATE" => "Creado",
        "UPDATE" => "Actualizado",
        "DELETE_SOFT" => "Eliminado",
        "DEACTIVATE" => "Inactivado",
        "REACTIVATE" => "Reactivado",
        _ => Accion
    };
}
