using System.Net.Http.Json;
using System.Text.Json;
using FrontEndBlazor.Models;

namespace FrontEndBlazor.Services;

public interface IProveedorService
{
    Task<List<ProveedorDto>> GetAllAsync(string? estado = null, string? nombre = null);
    Task<ProveedorDto?> GetByIdAsync(string id);
    Task<ProveedorDto> CreateAsync(ProveedorCreateDto dto);
    Task<ProveedorDto> UpdateAsync(string id, ProveedorUpdateDto dto);
    Task<bool> DeleteAsync(string id);
    Task<ProveedorDto> ActivateAsync(string id);
    Task<List<ProveedorShortDto>> SearchAsync(string query, int limit = 10);
    Task<ProveedorStatsDto> GetStatsAsync();
}

public class ProveedorService : IProveedorService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly JsonSerializerOptions _jsonOptions;

    public ProveedorService(IHttpClientFactory httpClientFactory)
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

    public async Task<List<ProveedorDto>> GetAllAsync(string? estado = null, string? nombre = null)
    {
        try
        {
            var client = CreateClient();
            
            // Construir query string con filtros
            var queryParams = new List<string>();
            if (!string.IsNullOrEmpty(estado)) 
                queryParams.Add($"estado={Uri.EscapeDataString(estado)}");
            if (!string.IsNullOrEmpty(nombre)) 
                queryParams.Add($"nombre={Uri.EscapeDataString(nombre)}");
            
            var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            var url = $"/api/proveedores/{queryString}";
            
            var response = await client.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Error al obtener proveedores: {response.StatusCode} - {error}");
            }

            var proveedores = await response.Content.ReadFromJsonAsync<List<ProveedorDto>>(_jsonOptions);
            return proveedores ?? new List<ProveedorDto>();
        }
        catch (HttpRequestException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al obtener proveedores: {ex.Message}", ex);
        }
    }

    public async Task<ProveedorDto?> GetByIdAsync(string id)
    {
        try
        {
            var client = CreateClient();
            var response = await client.GetAsync($"/api/proveedores/{id}");
            
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;
                
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Error al obtener proveedor: {response.StatusCode}");
            }

            var proveedor = await response.Content.ReadFromJsonAsync<ProveedorDto>(_jsonOptions);
            return proveedor;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al obtener proveedor: {ex.Message}", ex);
        }
    }

    public async Task<ProveedorDto> CreateAsync(ProveedorCreateDto dto)
    {
        try
        {
            var client = CreateClient();
            var response = await client.PostAsJsonAsync("/api/proveedores/", dto, _jsonOptions);
            var content = await response.Content.ReadAsStringAsync();
            
            if (!response.IsSuccessStatusCode)
            {
                // Intentar parsear error de API
                try
                {
                    var errorObj = JsonSerializer.Deserialize<Dictionary<string, object>>(content, _jsonOptions);
                    if (errorObj != null && errorObj.ContainsKey("detail"))
                    {
                        var detail = errorObj["detail"];
                        
                        // Manejar error de duplicado
                        if (detail is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Object)
                        {
                            var detailDict = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonElement.GetRawText(), _jsonOptions);
                            if (detailDict != null && detailDict.ContainsKey("error"))
                            {
                                var error = detailDict["error"]?.ToString();
                                if (error == "duplicate_nit")
                                {
                                    var message = detailDict.ContainsKey("message") 
                                        ? detailDict["message"]?.ToString() 
                                        : "El NIT ingresado ya está asociado a un proveedor existente";
                                    throw new InvalidOperationException(message ?? "NIT duplicado");
                                }
                            }
                        }
                        
                        // Error genérico
                        throw new HttpRequestException(detail?.ToString() ?? content);
                    }
                }
                catch (JsonException)
                {
                    // No es JSON, usar contenido directo
                }
                
                // Errores específicos por código de estado
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    throw new InvalidOperationException("No tiene permisos para crear proveedores. Esta acción requiere rol de administrador.");
                }
                
                throw new HttpRequestException($"Error al crear proveedor: {response.StatusCode} - {content}");
            }

            var proveedor = await response.Content.ReadFromJsonAsync<ProveedorDto>(_jsonOptions);
            return proveedor ?? throw new Exception("No se recibió el proveedor creado");
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
            throw new Exception($"Error al crear proveedor: {ex.Message}", ex);
        }
    }

    public async Task<ProveedorDto> UpdateAsync(string id, ProveedorUpdateDto dto)
    {
        try
        {
            var client = CreateClient();
            var response = await client.PutAsJsonAsync($"/api/proveedores/{id}", dto, _jsonOptions);
            var content = await response.Content.ReadAsStringAsync();
            
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    throw new InvalidOperationException("Proveedor no encontrado");
                }
                
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    throw new InvalidOperationException("No tiene permisos para actualizar proveedores. Esta acción requiere rol de administrador.");
                }
                
                throw new HttpRequestException($"Error al actualizar proveedor: {response.StatusCode} - {content}");
            }

            var proveedor = await response.Content.ReadFromJsonAsync<ProveedorDto>(_jsonOptions);
            return proveedor ?? throw new Exception("No se recibió el proveedor actualizado");
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al actualizar proveedor: {ex.Message}", ex);
        }
    }

    public async Task<bool> DeleteAsync(string id)
    {
        try
        {
            var client = CreateClient();
            var response = await client.DeleteAsync($"/api/proveedores/{id}");
            var content = await response.Content.ReadAsStringAsync();
            
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    throw new InvalidOperationException("Proveedor no encontrado");
                }
                
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    throw new InvalidOperationException("No tiene permisos para desactivar proveedores. Esta acción requiere rol de administrador.");
                }
                
                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    throw new InvalidOperationException("El proveedor ya está inactivo");
                }
                
                throw new HttpRequestException($"Error al desactivar proveedor: {response.StatusCode} - {content}");
            }

            return true;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al desactivar proveedor: {ex.Message}", ex);
        }
    }

    public async Task<ProveedorDto> ActivateAsync(string id)
    {
        try
        {
            var client = CreateClient();
            var response = await client.PostAsync($"/api/proveedores/{id}/activate", null);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    throw new InvalidOperationException("Proveedor no encontrado");
                }

                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    throw new InvalidOperationException("El proveedor ya está activo");
                }
                
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    throw new InvalidOperationException("No tiene permisos para reactivar proveedores. Esta acción requiere rol de administrador.");
                }

                throw new HttpRequestException($"Error al reactivar proveedor: {response.StatusCode} - {content}");
            }

            var proveedor = await response.Content.ReadFromJsonAsync<ProveedorDto>(_jsonOptions);
            return proveedor ?? throw new Exception("No se recibió respuesta de reactivación");
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al reactivar proveedor: {ex.Message}", ex);
        }
    }

    public async Task<List<ProveedorShortDto>> SearchAsync(string query, int limit = 10)
    {
        try
        {
            var client = CreateClient();
            var url = $"/api/proveedores/search?q={Uri.EscapeDataString(query)}&limit={limit}";
            var response = await client.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
            {
                return new List<ProveedorShortDto>();
            }

            var results = await response.Content.ReadFromJsonAsync<List<ProveedorShortDto>>(_jsonOptions);
            return results ?? new List<ProveedorShortDto>();
        }
        catch (Exception)
        {
            // En caso de error, retornar lista vacía (búsqueda no crítica)
            return new List<ProveedorShortDto>();
        }
    }

    public async Task<ProveedorStatsDto> GetStatsAsync()
    {
        try
        {
            var client = CreateClient();
            var response = await client.GetAsync("/api/proveedores/stats");
            
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Error al obtener estadísticas: {response.StatusCode}");
            }

            var stats = await response.Content.ReadFromJsonAsync<ProveedorStatsDto>(_jsonOptions);
            return stats ?? new ProveedorStatsDto();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al obtener estadísticas: {ex.Message}", ex);
        }
    }
}
