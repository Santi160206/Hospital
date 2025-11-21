using System.Net.Http.Json;
using System.Text.Json;
using FrontEndBlazor.Models;

namespace FrontEndBlazor.Services;

public interface IVentaService
{
    // CRUD de ventas
    Task<VentaDto> CreateAsync(VentaCreateDto dto);
    Task<List<VentaDto>> GetAllAsync(EstadoVenta? estado = null, DateTime? fechaInicio = null, DateTime? fechaFin = null);
    Task<VentaDto?> GetByIdAsync(string id);
    Task<VentaConfirmarResponse> ConfirmarPagoAsync(string id, VentaConfirmarPagoDto dto);
    
    //reportes y proyecciones
    Task<ReporteVentasResponse?> GenerarReporteVentasAsync(FiltrosReporteVentas filtros);
    Task<ProyeccionVentasResponse?> GenerarProyeccionDemandaAsync(FiltrosProyeccionVentas filtros);
    Task<EstadisticasVentas?> GetEstadisticasAsync(DateTime? fechaInicio = null, DateTime? fechaFin = null);
}

public class VentaService : IVentaService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly JsonSerializerOptions _jsonOptions;

    public VentaService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
        
        //Configurar JSON options para que los enums se serialicen en mayusculas, así lo espera el backend
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            PropertyNameCaseInsensitive = true,
            NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
        };
    }

    private HttpClient CreateClient()
    {
        return _httpClientFactory.CreateClient("AuthenticatedApi");
    }

    /// <summary>
    /// Crear nueva venta
    /// </summary>
    public async Task<VentaDto> CreateAsync(VentaCreateDto dto)
    {
        try
        {
            var client = CreateClient();
            var response = await client.PostAsJsonAsync("/api/ventas/", dto, _jsonOptions);
            var content = await response.Content.ReadAsStringAsync();
            
            if (!response.IsSuccessStatusCode)
            {
                // Parsear error
                try
                {
                    var errorObj = JsonSerializer.Deserialize<Dictionary<string, object>>(content, _jsonOptions);
                    if (errorObj != null && errorObj.ContainsKey("detail"))
                    {
                        var detail = errorObj["detail"]?.ToString();
                        
                        // Errores específicos
                        if (content.Contains("insufficient_stock"))
                        {
                            throw new InvalidOperationException(detail ?? "Stock insuficiente para completar la venta");
                        }
                        
                        throw new InvalidOperationException(detail ?? content);
                    }
                }
                catch (JsonException) { }
                
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    throw new InvalidOperationException("Medicamento no encontrado");
                }
                
                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    throw new InvalidOperationException(content);
                }
                
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    throw new InvalidOperationException("No tiene permisos para registrar ventas. Requiere rol de farmacéutico o administrador.");
                }
                
                throw new HttpRequestException($"Error al crear venta: {response.StatusCode} - {content}");
            }

            //parsear respuesta: puede ser StandardResponse o VentaDto directo
            try
            {
                // Intentar parsear como StandardResponse primero
                var standardResponse = await JsonSerializer.DeserializeAsync<StandardResponse<VentaDto>>(
                    new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content)), 
                    _jsonOptions
                );
                
                if (standardResponse?.Data != null)
                {
                    return standardResponse.Data;
                }
            }
            catch { }
            
            //fallback: parsear como VentaDto directo
            var venta = await response.Content.ReadFromJsonAsync<VentaDto>(_jsonOptions);
            return venta ?? throw new Exception("No se recibió la venta creada");
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
            throw new Exception($"Error al crear venta: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Listar ventas con filtros opcionales
    /// </summary>
    public async Task<List<VentaDto>> GetAllAsync(
        EstadoVenta? estado = null, 
        DateTime? fechaInicio = null, 
        DateTime? fechaFin = null)
    {
        try
        {
            var client = CreateClient();
            
            // Construir query string
            var queryParams = new List<string>();
            if (estado.HasValue) 
                queryParams.Add($"estado={estado.Value.ToString().ToUpper()}");
            if (fechaInicio.HasValue) 
                queryParams.Add($"fecha_inicio={fechaInicio.Value:yyyy-MM-dd}");
            if (fechaFin.HasValue) 
                queryParams.Add($"fecha_fin={fechaFin.Value:yyyy-MM-dd}");
            
            var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            var url = $"/api/ventas/{queryString}";
            
            var response = await client.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Error al obtener ventas: {response.StatusCode} - {error}");
            }

            var ventas = await response.Content.ReadFromJsonAsync<List<VentaDto>>(_jsonOptions);
            return ventas ?? new List<VentaDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en GetAllAsync: {ex.Message}");
            return new List<VentaDto>();
        }
    }

    /// <summary>
    ///obtener venta por ID
    /// </summary>
    public async Task<VentaDto?> GetByIdAsync(string id)
    {
        try
        {
            var client = CreateClient();
            var response = await client.GetAsync($"/api/ventas/{id}");
            
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;
                
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Error al obtener venta: {response.StatusCode}");
            }

            var venta = await response.Content.ReadFromJsonAsync<VentaDto>(_jsonOptions);
            return venta;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en GetByIdAsync: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    ///confirmar pago de venta pendiente
    /// </summary>
    public async Task<VentaConfirmarResponse> ConfirmarPagoAsync(string id, VentaConfirmarPagoDto dto)
    {
        try
        {
            var client = CreateClient();
            var response = await client.PostAsJsonAsync($"/api/ventas/{id}/confirmar-pago", dto, _jsonOptions);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    throw new InvalidOperationException("Venta no encontrada");
                }

                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    // Parsear mensaje específico
                    try
                    {
                        var errorObj = JsonSerializer.Deserialize<Dictionary<string, object>>(content, _jsonOptions);
                        if (errorObj != null && errorObj.ContainsKey("detail"))
                        {
                            var detail = errorObj["detail"]?.ToString();
                            
                            if (content.Contains("already_confirmed"))
                            {
                                throw new InvalidOperationException("La venta ya está confirmada");
                            }
                            if (content.Contains("cancelled"))
                            {
                                throw new InvalidOperationException("No se puede confirmar una venta cancelada");
                            }
                            if (content.Contains("insufficient_stock"))
                            {
                                throw new InvalidOperationException(detail ?? "Stock insuficiente para confirmar la venta");
                            }
                            
                            throw new InvalidOperationException(detail ?? content);
                        }
                    }
                    catch (JsonException) { }
                    
                    throw new InvalidOperationException(content);
                }
                
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    throw new InvalidOperationException("No tiene permisos para confirmar pagos.");
                }

                throw new HttpRequestException($"Error al confirmar pago: {response.StatusCode} - {content}");
            }

            var result = await response.Content.ReadFromJsonAsync<VentaConfirmarResponse>(_jsonOptions);
            return result ?? throw new Exception("No se recibió respuesta de confirmación");
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al confirmar pago: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Generar reporte de ventas por período
    /// </summary>
    public async Task<ReporteVentasResponse?> GenerarReporteVentasAsync(FiltrosReporteVentas filtros)
    {
        try
        {
            var client = CreateClient();
            var response = await client.PostAsJsonAsync("/api/ventas/reportes/ventas", filtros, _jsonOptions);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error en GenerarReporteVentasAsync: {response.StatusCode} - {errorContent}");
                
                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    throw new InvalidOperationException($"Error de validación: {errorContent}");
                }
                
                return null;
            }

            var reporte = await response.Content.ReadFromJsonAsync<ReporteVentasResponse>(_jsonOptions);
            return reporte;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en GenerarReporteVentasAsync: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    ///generar proyección de demanda
    /// </summary>
    public async Task<ProyeccionVentasResponse?> GenerarProyeccionDemandaAsync(FiltrosProyeccionVentas filtros)
    {
        try
        {
            var client = CreateClient();
            var response = await client.PostAsJsonAsync("/api/ventas/reportes/proyeccion", filtros, _jsonOptions);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error en GenerarProyeccionDemandaAsync: {response.StatusCode} - {errorContent}");
                
                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    throw new InvalidOperationException($"Error de validación: {errorContent}");
                }
                
                return null;
            }

            var proyeccion = await response.Content.ReadFromJsonAsync<ProyeccionVentasResponse>(_jsonOptions);
            return proyeccion;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en GenerarProyeccionDemandaAsync: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Obtener estadísticas generales de ventas
    /// </summary>
    public async Task<EstadisticasVentas?> GetEstadisticasAsync(DateTime? fechaInicio = null, DateTime? fechaFin = null)
    {
        try
        {
            var client = CreateClient();
            
            // Construir query string
            var queryParams = new List<string>();
            if (fechaInicio.HasValue) 
                queryParams.Add($"fecha_inicio={fechaInicio.Value:yyyy-MM-dd}");
            if (fechaFin.HasValue) 
                queryParams.Add($"fecha_fin={fechaFin.Value:yyyy-MM-dd}");
            
            var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            var url = $"/api/ventas/estadisticas{queryString}";
            
            var response = await client.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var stats = await response.Content.ReadFromJsonAsync<EstadisticasVentas>(_jsonOptions);
            return stats;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en GetEstadisticasAsync: {ex.Message}");
            return null;
        }
    }
}

/// <summary>
/// Helper para parsear respuesta estándar del backend
/// </summary>
public class StandardResponse<T>
{
    public bool Ok { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public string? Error { get; set; }
}
