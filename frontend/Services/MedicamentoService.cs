using System.Net.Http.Json;
using System.Text.Json;
using FrontEndBlazor.Models;

namespace FrontEndBlazor.Services;

public interface IMedicamentoService
{
    Task<List<MedicamentoDto>> GetAllAsync(string? nombre = null, string? fabricante = null, string? lote = null, string? estado = null, string? fechaVencimiento = null, bool? stockBajo = null);
    Task<MedicamentoDto?> GetByIdAsync(string id);
    Task<MedicamentoDto> CreateAsync(MedicamentoCreateDto dto);
    Task<MedicamentoDto> UpdateAsync(string id, MedicamentoUpdateDto dto);
    Task<DeleteResponse> DeleteAsync(string id);
    Task<ReactivateResponse> ReactivateAsync(string id);
    Task<List<MovimientoDto>> GetMovimientosAsync(string medicamentoId);
    Task<MovimientoDto> CreateMovimientoAsync(string medicamentoId, MovimientoCreateDto dto);
    Task<List<AuditLogDto>> GetAuditLogsAsync(string medicamentoId);
    Task<List<MedicamentoShortDto>> SearchAsync(string query, string filter = "nombre", int limit = 8);
}

public class MedicamentoService : IMedicamentoService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly JsonSerializerOptions _jsonOptions;

    public MedicamentoService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
        
        // Configurar JSON options para snake_case
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            PropertyNameCaseInsensitive = true,
            // Permitir conversión de números a decimal
            NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
        };
    }

    private HttpClient CreateClient()
    {
        return _httpClientFactory.CreateClient("AuthenticatedApi");
    }

    public async Task<List<MedicamentoDto>> GetAllAsync(
        string? nombre = null, 
        string? fabricante = null,
        string? lote = null,
        string? estado = null, 
        string? fechaVencimiento = null, 
        bool? stockBajo = null)
    {
        try
        {
            var client = CreateClient();
            
            // Construir query string con filtros
            var queryParams = new List<string>();
            if (!string.IsNullOrEmpty(nombre)) queryParams.Add($"nombre={Uri.EscapeDataString(nombre)}");
            if (!string.IsNullOrEmpty(fabricante)) queryParams.Add($"fabricante={Uri.EscapeDataString(fabricante)}");
            if (!string.IsNullOrEmpty(lote)) queryParams.Add($"lote={Uri.EscapeDataString(lote)}");
            if (!string.IsNullOrEmpty(estado)) queryParams.Add($"estado={Uri.EscapeDataString(estado)}");
            if (!string.IsNullOrEmpty(fechaVencimiento)) queryParams.Add($"fecha_vencimiento={Uri.EscapeDataString(fechaVencimiento)}");
            if (stockBajo.HasValue && stockBajo.Value) queryParams.Add("stock_bajo=true");
            
            var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            var url = $"/api/medicamentos/{queryString}";
            
            var response = await client.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Error al obtener medicamentos: {response.StatusCode} - {error}");
            }

            var medicamentos = await response.Content.ReadFromJsonAsync<List<MedicamentoDto>>(_jsonOptions);
            return medicamentos ?? new List<MedicamentoDto>();
        }
        catch (HttpRequestException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al obtener medicamentos: {ex.Message}", ex);
        }
    }

    public async Task<MedicamentoDto?> GetByIdAsync(string id)
    {
        try
        {
            var client = CreateClient();
            var response = await client.GetAsync($"/api/medicamentos/{id}");
            
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;
                
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Error al obtener medicamento: {response.StatusCode}");
            }

            var medicamento = await response.Content.ReadFromJsonAsync<MedicamentoDto>(_jsonOptions);
            return medicamento;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al obtener medicamento: {ex.Message}", ex);
        }
    }

    public async Task<MedicamentoDto> CreateAsync(MedicamentoCreateDto dto)
    {
        try
        {
            var client = CreateClient();
            var response = await client.PostAsJsonAsync("/api/medicamentos/", dto, _jsonOptions);
            var content = await response.Content.ReadAsStringAsync();
            
            if (!response.IsSuccessStatusCode)
            {
                // Intentar parsear error de API
                try
                {
                    var errorObj = JsonSerializer.Deserialize<ApiErrorResponse>(content, _jsonOptions);
                    if (errorObj != null)
                    {
                        if (errorObj.Error == "Medicamento duplicado")
                        {
                            var message = errorObj.Message ?? "Ya existe un medicamento con estos datos";
                            throw new InvalidOperationException(message);
                        }
                        throw new HttpRequestException(errorObj.Detail ?? errorObj.Error ?? content);
                    }
                }
                catch (JsonException)
                {
                    // No es JSON, usar contenido directo
                }
                
                throw new HttpRequestException($"Error al crear medicamento: {response.StatusCode} - {content}");
            }

            var medicamento = await response.Content.ReadFromJsonAsync<MedicamentoDto>(_jsonOptions);
            return medicamento ?? throw new Exception("No se recibió el medicamento creado");
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (HttpRequestException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al crear medicamento: {ex.Message}", ex);
        }
    }

    public async Task<MedicamentoDto> UpdateAsync(string id, MedicamentoUpdateDto dto)
    {
        try
        {
            var client = CreateClient();
            var response = await client.PutAsJsonAsync($"/api/medicamentos/{id}", dto, _jsonOptions);
            var content = await response.Content.ReadAsStringAsync();
            
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    throw new InvalidOperationException("Medicamento no encontrado");
                }
                
                if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    throw new InvalidOperationException("Ya existe otro medicamento con estos datos");
                }
                
                throw new HttpRequestException($"Error al actualizar medicamento: {response.StatusCode} - {content}");
            }

            var medicamento = await response.Content.ReadFromJsonAsync<MedicamentoDto>(_jsonOptions);
            return medicamento ?? throw new Exception("No se recibió el medicamento actualizado");
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al actualizar medicamento: {ex.Message}", ex);
        }
    }

    public async Task<DeleteResponse> DeleteAsync(string id)
    {
        try
        {
            var client = CreateClient();
            var response = await client.DeleteAsync($"/api/medicamentos/{id}");
            var content = await response.Content.ReadAsStringAsync();
            
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    throw new InvalidOperationException("Medicamento no encontrado");
                }
                
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    throw new InvalidOperationException("No tiene permisos para eliminar medicamentos (requiere rol Admin)");
                }
                
                throw new HttpRequestException($"Error al eliminar medicamento: {response.StatusCode} - {content}");
            }

            var result = await response.Content.ReadFromJsonAsync<DeleteResponse>(_jsonOptions);
            return result ?? throw new Exception("No se recibió respuesta de eliminación");
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al eliminar medicamento: {ex.Message}", ex);
        }
    }

    public async Task<ReactivateResponse> ReactivateAsync(string id)
    {
        try
        {
            var client = CreateClient();
            var response = await client.PostAsync($"/api/medicamentos/{id}/reactivar", null);
            var content = await response.Content.ReadAsStringAsync();
            
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    throw new InvalidOperationException("Medicamento no encontrado");
                }
                
                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    throw new InvalidOperationException("No es posible reactivar: la fecha de vencimiento ya expiró");
                }
                
                throw new HttpRequestException($"Error al reactivar medicamento: {response.StatusCode} - {content}");
            }

            var result = await response.Content.ReadFromJsonAsync<ReactivateResponse>(_jsonOptions);
            return result ?? throw new Exception("No se recibió respuesta de reactivación");
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al reactivar medicamento: {ex.Message}", ex);
        }
    }

    public async Task<List<MovimientoDto>> GetMovimientosAsync(string medicamentoId)
    {
        try
        {
            var client = CreateClient();
            var response = await client.GetAsync($"/api/medicamentos/{medicamentoId}/movimientos");
            
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Error al obtener movimientos: {response.StatusCode}");
            }

            var movimientos = await response.Content.ReadFromJsonAsync<List<MovimientoDto>>(_jsonOptions);
            return movimientos ?? new List<MovimientoDto>();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al obtener movimientos: {ex.Message}", ex);
        }
    }

    public async Task<MovimientoDto> CreateMovimientoAsync(string medicamentoId, MovimientoCreateDto dto)
    {
        try
        {
            var client = CreateClient();
            var response = await client.PostAsJsonAsync($"/api/medicamentos/{medicamentoId}/movimientos", dto, _jsonOptions);
            var content = await response.Content.ReadAsStringAsync();
            
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    throw new InvalidOperationException("Medicamento no encontrado");
                }
                
                // Parsear mensaje de error específico del backend
                try
                {
                    var errorObj = JsonSerializer.Deserialize<Dictionary<string, object>>(content, _jsonOptions);
                    if (errorObj != null && errorObj.ContainsKey("detail"))
                    {
                        var detail = errorObj["detail"]?.ToString() ?? content;
                        throw new InvalidOperationException(detail);
                    }
                }
                catch (JsonException)
                {
                    // No es JSON, usar contenido directo
                }
                
                throw new HttpRequestException($"Error al registrar movimiento: {content}");
            }

            var movimiento = await response.Content.ReadFromJsonAsync<MovimientoDto>(_jsonOptions);
            return movimiento ?? throw new Exception("No se recibió el movimiento creado");
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al registrar movimiento: {ex.Message}", ex);
        }
    }

    public async Task<List<AuditLogDto>> GetAuditLogsAsync(string medicamentoId)
    {
        try
        {
            var client = CreateClient();
            var response = await client.GetAsync($"/api/medicamentos/{medicamentoId}/audit");
            
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Error al obtener auditoría: {response.StatusCode}");
            }

            var auditLogs = await response.Content.ReadFromJsonAsync<List<AuditLogDto>>(_jsonOptions);
            return auditLogs ?? new List<AuditLogDto>();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al obtener auditoría: {ex.Message}", ex);
        }
    }

    public async Task<List<MedicamentoShortDto>> SearchAsync(string query, string filter = "nombre", int limit = 8)
    {
        try
        {
            var client = CreateClient();
            var url = $"/api/medicamentos/search?query={Uri.EscapeDataString(query)}&filter={filter}&limit={limit}";
            var response = await client.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
            {
                return new List<MedicamentoShortDto>();
            }

            var results = await response.Content.ReadFromJsonAsync<List<MedicamentoShortDto>>(_jsonOptions);
            return results ?? new List<MedicamentoShortDto>();
        }
        catch (Exception)
        {
            // En caso de error, retornar lista vacía (búsqueda no crítica)
            return new List<MedicamentoShortDto>();
        }
    }
}
