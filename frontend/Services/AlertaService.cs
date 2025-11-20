using System.Net.Http.Json;
using System.Text.Json;
using FrontEndBlazor.Models;

namespace FrontEndBlazor.Services;

public interface IAlertaService
{
    Task<List<AlertaDto>> GetAlertasActivasAsync(TipoAlerta? tipo = null, PrioridadAlerta? prioridad = null, int limit = 100);
    Task<List<AlertaDto>> GetHistorialAlertasAsync(string? medicamentoId = null, EstadoAlerta? estado = null, int limit = 100);
    Task<AlertaDto?> GetAlertaDetalleAsync(string alertaId);
    Task<List<NotificacionDto>> GetMisNotificacionesAsync(int count = 10);
    Task LimpiarNotificacionesAsync();
    Task<AlertaStatsDto> GetEstadisticasAsync();
    Task<AlertaDto> ActualizarEstadoAsync(string alertaId, AlertaUpdateEstadoDto dto);
    Task VerificarAlertasMedicamentoAsync(string medicamentoId);
}

public class AlertaService : IAlertaService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly JsonSerializerOptions _jsonOptions;

    public AlertaService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
        
        // Configurar JSON options para snake_case
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            PropertyNameCaseInsensitive = true,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
        };
    }

    private HttpClient CreateClient()
    {
        return _httpClientFactory.CreateClient("AuthenticatedApi");
    }

    public async Task<List<AlertaDto>> GetAlertasActivasAsync(
        TipoAlerta? tipo = null, 
        PrioridadAlerta? prioridad = null, 
        int limit = 100)
    {
        try
        {
            var client = CreateClient();
            
            // Construir query string
            var queryParams = new List<string> { $"limit={limit}" };
            if (tipo.HasValue) queryParams.Add($"tipo={tipo.Value}");
            if (prioridad.HasValue) queryParams.Add($"prioridad={prioridad.Value}");
            
            var queryString = string.Join("&", queryParams);
            var url = $"/api/alertas/activas?{queryString}";
            
            var response = await client.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Error al obtener alertas activas: {response.StatusCode} - {error}");
            }

            var alertas = await response.Content.ReadFromJsonAsync<List<AlertaDto>>(_jsonOptions);
            return alertas ?? new List<AlertaDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en GetAlertasActivasAsync: {ex.Message}");
            return new List<AlertaDto>();
        }
    }

    public async Task<List<AlertaDto>> GetHistorialAlertasAsync(
        string? medicamentoId = null, 
        EstadoAlerta? estado = null, 
        int limit = 100)
    {
        try
        {
            var client = CreateClient();
            
            var queryParams = new List<string> { $"limit={limit}" };
            if (!string.IsNullOrEmpty(medicamentoId)) queryParams.Add($"medicamento_id={medicamentoId}");
            if (estado.HasValue) queryParams.Add($"estado={estado.Value}");
            
            var queryString = string.Join("&", queryParams);
            var url = $"/api/alertas/historial?{queryString}";
            
            var response = await client.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Error al obtener historial: {response.StatusCode} - {error}");
            }

            var alertas = await response.Content.ReadFromJsonAsync<List<AlertaDto>>(_jsonOptions);
            return alertas ?? new List<AlertaDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en GetHistorialAlertasAsync: {ex.Message}");
            return new List<AlertaDto>();
        }
    }

    public async Task<AlertaDto?> GetAlertaDetalleAsync(string alertaId)
    {
        try
        {
            var client = CreateClient();
            var response = await client.GetAsync($"/api/alertas/{alertaId}");
            
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;
                
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Error al obtener detalle: {response.StatusCode} - {error}");
            }

            return await response.Content.ReadFromJsonAsync<AlertaDto>(_jsonOptions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en GetAlertaDetalleAsync: {ex.Message}");
            return null;
        }
    }

    public async Task<List<NotificacionDto>> GetMisNotificacionesAsync(int count = 10)
    {
        try
        {
            var client = CreateClient();
            var response = await client.GetAsync($"/api/alertas/notificaciones/mis-alertas?count={count}");
            
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Error al obtener notificaciones: {response.StatusCode} - {error}");
            }

            var notificaciones = await response.Content.ReadFromJsonAsync<List<NotificacionDto>>(_jsonOptions);
            return notificaciones ?? new List<NotificacionDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en GetMisNotificacionesAsync: {ex.Message}");
            return new List<NotificacionDto>();
        }
    }

    public async Task LimpiarNotificacionesAsync()
    {
        try
        {
            var client = CreateClient();
            var response = await client.DeleteAsync("/api/alertas/notificaciones/limpiar");
            
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Error al limpiar notificaciones: {response.StatusCode} - {error}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en LimpiarNotificacionesAsync: {ex.Message}");
            throw;
        }
    }

    public async Task<AlertaStatsDto> GetEstadisticasAsync()
    {
        try
        {
            var client = CreateClient();
            var response = await client.GetAsync("/api/alertas/stats/resumen");
            
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Error al obtener estadísticas: {response.StatusCode} - {error}");
            }

            var stats = await response.Content.ReadFromJsonAsync<AlertaStatsDto>(_jsonOptions);
            return stats ?? new AlertaStatsDto();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en GetEstadisticasAsync: {ex.Message}");
            return new AlertaStatsDto();
        }
    }

    public async Task<AlertaDto> ActualizarEstadoAsync(string alertaId, AlertaUpdateEstadoDto dto)
    {
        try
        {
            var client = CreateClient();
            var response = await client.PatchAsJsonAsync($"/api/alertas/{alertaId}/estado", dto, _jsonOptions);
            
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Error al actualizar estado: {response.StatusCode} - {error}");
            }

            var alerta = await response.Content.ReadFromJsonAsync<AlertaDto>(_jsonOptions);
            return alerta ?? throw new Exception("Respuesta inválida del servidor");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en ActualizarEstadoAsync: {ex.Message}");
            throw;
        }
    }

    public async Task VerificarAlertasMedicamentoAsync(string medicamentoId)
    {
        try
        {
            var client = CreateClient();
            var response = await client.PostAsync($"/api/alertas/check/{medicamentoId}", null);
            
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Error al verificar alertas: {response.StatusCode} - {error}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en VerificarAlertasMedicamentoAsync: {ex.Message}");
            throw;
        }
    }
}
