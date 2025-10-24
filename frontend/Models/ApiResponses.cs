namespace FrontEndBlazor.Models;

/// <summary>
/// Respuesta de eliminación
/// </summary>
public class DeleteResponse
{
    public bool Deleted { get; set; }
    public int Dependencias { get; set; }
}

/// <summary>
/// Respuesta de reactivación
/// </summary>
public class ReactivateResponse
{
    public bool Reactivated { get; set; }
    public MedicamentoDto? Medicamento { get; set; }
}

/// <summary>
/// Respuesta genérica con mensaje
/// </summary>
public class MessageResponse
{
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Respuesta de error de la API
/// </summary>
public class ApiErrorResponse
{
    public string? Error { get; set; }
    public string? Detail { get; set; }
    public string? Message { get; set; }
    public string? ExistingId { get; set; }
}
