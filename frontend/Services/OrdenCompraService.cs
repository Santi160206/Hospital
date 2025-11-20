using System.Net.Http.Json;
using System.Text.Json;
using FrontEndBlazor.Models;

namespace FrontEndBlazor.Services;

public interface IOrdenCompraService
{
    // CRUD básico
    Task<List<OrdenCompraDto>> GetAllAsync(string? estado = null, string? proveedorId = null);
    Task<OrdenCompraDto?> GetByIdAsync(string id);
    Task<OrdenCompraDto> CreateAsync(OrdenCompraCreateDto dto);
    Task<OrdenCompraDto> UpdateAsync(string id, OrdenCompraUpdateDto dto);
    
    // Acciones de flujo
    Task<OrdenCompraDto> MarcarEnviadaAsync(string id, MarcarEnviadaDto dto);
    Task<RecepcionOrdenResponse> RecibirOrdenAsync(string id, RecepcionOrdenDto dto);
    
    // Consultas
    Task<List<OrdenCompraDto>> GetRetrasadasAsync();
    Task<OrdenCompraStatsDto> GetStatsAsync();
}

public class OrdenCompraService : IOrdenCompraService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly JsonSerializerOptions _jsonOptions;

    public OrdenCompraService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
        
        // Configurar JSON options para snake_case
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            PropertyNameCaseInsensitive = true,
            NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
        };
    }

    private HttpClient CreateClient()
    {
        return _httpClientFactory.CreateClient("AuthenticatedApi");
    }

    public async Task<List<OrdenCompraDto>> GetAllAsync(string? estado = null, string? proveedorId = null)
    {
        try
        {
            var client = CreateClient();
            
            // Construir query string
            var queryParams = new List<string>();
            if (!string.IsNullOrEmpty(estado)) 
                queryParams.Add($"estado={Uri.EscapeDataString(estado)}");
            if (!string.IsNullOrEmpty(proveedorId)) 
                queryParams.Add($"proveedor_id={Uri.EscapeDataString(proveedorId)}");
            
            var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            var url = $"/api/ordenes/{queryString}";
            
            var response = await client.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Error al obtener órdenes: {response.StatusCode} - {error}");
            }

            var ordenes = await response.Content.ReadFromJsonAsync<List<OrdenCompraDto>>(_jsonOptions);
            return ordenes ?? new List<OrdenCompraDto>();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al obtener órdenes: {ex.Message}", ex);
        }
    }

    public async Task<OrdenCompraDto?> GetByIdAsync(string id)
    {
        try
        {
            var client = CreateClient();
            var response = await client.GetAsync($"/api/ordenes/{id}");
            
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;
                
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Error al obtener orden: {response.StatusCode}");
            }

            var orden = await response.Content.ReadFromJsonAsync<OrdenCompraDto>(_jsonOptions);
            return orden;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al obtener orden: {ex.Message}", ex);
        }
    }

    public async Task<OrdenCompraDto> CreateAsync(OrdenCompraCreateDto dto)
    {
        try
        {
            var client = CreateClient();
            var response = await client.PostAsJsonAsync("/api/ordenes/", dto, _jsonOptions);
            var content = await response.Content.ReadAsStringAsync();
            
            if (!response.IsSuccessStatusCode)
            {
                // Manejo de errores específicos
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    throw new InvalidOperationException("Proveedor o medicamento no encontrado");
                }
                
                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    // Intentar parsear error
                    try
                    {
                        var errorObj = JsonSerializer.Deserialize<Dictionary<string, object>>(content, _jsonOptions);
                        if (errorObj != null && errorObj.ContainsKey("detail"))
                        {
                            throw new InvalidOperationException(errorObj["detail"]?.ToString() ?? content);
                        }
                    }
                    catch (JsonException) { }
                    
                    throw new InvalidOperationException(content);
                }
                
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    throw new InvalidOperationException("No tiene permisos para crear órdenes. Esta acción requiere rol de administrador o compras.");
                }
                
                throw new HttpRequestException($"Error al crear orden: {response.StatusCode} - {content}");
            }

            var orden = await response.Content.ReadFromJsonAsync<OrdenCompraDto>(_jsonOptions);
            return orden ?? throw new Exception("No se recibió la orden creada");
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al crear orden: {ex.Message}", ex);
        }
    }

    public async Task<OrdenCompraDto> UpdateAsync(string id, OrdenCompraUpdateDto dto)
    {
        try
        {
            var client = CreateClient();
            var response = await client.PutAsJsonAsync($"/api/ordenes/{id}", dto, _jsonOptions);
            var content = await response.Content.ReadAsStringAsync();
            
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    throw new InvalidOperationException("Orden no encontrada");
                }
                
                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    throw new InvalidOperationException("Solo se puede editar una orden en estado PENDIENTE");
                }
                
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    throw new InvalidOperationException("No tiene permisos para actualizar órdenes.");
                }
                
                throw new HttpRequestException($"Error al actualizar orden: {response.StatusCode} - {content}");
            }

            var orden = await response.Content.ReadFromJsonAsync<OrdenCompraDto>(_jsonOptions);
            return orden ?? throw new Exception("No se recibió la orden actualizada");
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al actualizar orden: {ex.Message}", ex);
        }
    }

    public async Task<OrdenCompraDto> MarcarEnviadaAsync(string id, MarcarEnviadaDto dto)
    {
        try
        {
            var client = CreateClient();
            var response = await client.PostAsJsonAsync($"/api/ordenes/{id}/enviar", dto, _jsonOptions);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    throw new InvalidOperationException("Orden no encontrada");
                }

                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    throw new InvalidOperationException("Solo se puede enviar una orden en estado PENDIENTE");
                }
                
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    throw new InvalidOperationException("No tiene permisos para enviar órdenes.");
                }

                throw new HttpRequestException($"Error al marcar como enviada: {response.StatusCode} - {content}");
            }

            var orden = await response.Content.ReadFromJsonAsync<OrdenCompraDto>(_jsonOptions);
            return orden ?? throw new Exception("No se recibió respuesta");
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al marcar como enviada: {ex.Message}", ex);
        }
    }

    public async Task<RecepcionOrdenResponse> RecibirOrdenAsync(string id, RecepcionOrdenDto dto)
    {
        try
        {
            var client = CreateClient();
            var response = await client.PostAsJsonAsync($"/api/ordenes/{id}/recibir", dto, _jsonOptions);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    throw new InvalidOperationException("Orden no encontrada");
                }

                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    throw new InvalidOperationException("Solo se puede recibir una orden en estado ENVIADA o RETRASADA");
                }
                
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    throw new InvalidOperationException("No tiene permisos para recibir órdenes.");
                }

                throw new HttpRequestException($"Error al recibir orden: {response.StatusCode} - {content}");
            }

            var result = await response.Content.ReadFromJsonAsync<RecepcionOrdenResponse>(_jsonOptions);
            return result ?? throw new Exception("No se recibió respuesta de recepción");
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al recibir orden: {ex.Message}", ex);
        }
    }

    public async Task<List<OrdenCompraDto>> GetRetrasadasAsync()
    {
        try
        {
            var client = CreateClient();
            var response = await client.GetAsync("/api/ordenes/retrasadas");
            
            if (!response.IsSuccessStatusCode)
            {
                return new List<OrdenCompraDto>();
            }

            var ordenes = await response.Content.ReadFromJsonAsync<List<OrdenCompraDto>>(_jsonOptions);
            return ordenes ?? new List<OrdenCompraDto>();
        }
        catch (Exception)
        {
            return new List<OrdenCompraDto>();
        }
    }

    public async Task<OrdenCompraStatsDto> GetStatsAsync()
    {
        try
        {
            var client = CreateClient();
            var response = await client.GetAsync("/api/ordenes/stats");
            
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Error al obtener estadísticas: {response.StatusCode}");
            }

            var stats = await response.Content.ReadFromJsonAsync<OrdenCompraStatsDto>(_jsonOptions);
            return stats ?? new OrdenCompraStatsDto();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al obtener estadísticas: {ex.Message}", ex);
        }
    }
}
