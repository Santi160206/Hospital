using System.Net.Http.Json;
using System.Text.Json;
using FrontEndBlazor.Models;

namespace FrontEndBlazor.Services;

public interface IVentaService
{
    Task<VentaDto> CrearVentaAsync(VentaCreateDto dto);
    Task<List<VentaDto>> ListarVentasAsync(DateTime? fechaInicio = null, DateTime? fechaFin = null, string? usuarioId = null, int limit = 100);
    Task<VentaDto?> ObtenerVentaPorIdAsync(string id);
    Task<ReporteCompletoDto> GenerarReporteVentasAsync(DateTime fechaInicio, DateTime fechaFin);
    Task<ProyeccionDemandaDto> GenerarProyeccionDemandaAsync(int diasProyeccion = 30);
    Task<object> ObtenerVentasDiariasAsync(DateTime fecha);
}

public class VentaService : IVentaService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly JsonSerializerOptions _jsonOptions;

    public VentaService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
        
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

    public async Task<VentaDto> CrearVentaAsync(VentaCreateDto dto)
    {
        try
        {
            var client = CreateClient();
            var response = await client.PostAsJsonAsync("/api/ventas/", dto, _jsonOptions);
            var content = await response.Content.ReadAsStringAsync();
            
            if (!response.IsSuccessStatusCode)
            {
                // Manejar errores específicos de ventas
                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    try
                    {
                        var errorObj = JsonSerializer.Deserialize<ApiErrorResponse>(content, _jsonOptions);
                        if (errorObj != null)
                        {
                            if (errorObj.Error == "Stock insuficiente")
                            {
                                throw new InvalidOperationException($"Stock insuficiente: {errorObj.Detail}");
                            }
                            throw new InvalidOperationException(errorObj.Detail ?? errorObj.Error ?? content);
                        }
                    }
                    catch (JsonException)
                    {
                        // No es JSON, usar contenido directo
                    }
                }
                
                throw new HttpRequestException($"Error al crear venta: {response.StatusCode} - {content}");
            }

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

    public async Task<List<VentaDto>> ListarVentasAsync(
        DateTime? fechaInicio = null, 
        DateTime? fechaFin = null, 
        string? usuarioId = null, 
        int limit = 100)
    {
        try
        {
            var client = CreateClient();
            
            // Construir query string
            var queryParams = new List<string>();
            if (fechaInicio.HasValue) queryParams.Add($"fecha_inicio={fechaInicio.Value:yyyy-MM-dd}");
            if (fechaFin.HasValue) queryParams.Add($"fecha_fin={fechaFin.Value:yyyy-MM-dd}");
            if (!string.IsNullOrEmpty(usuarioId)) queryParams.Add($"usuario_id={Uri.EscapeDataString(usuarioId)}");
            queryParams.Add($"limit={limit}");
            
            var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            var url = $"/api/ventas/{queryString}";
            
            var response = await client.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Error al obtener ventas: {response.StatusCode}");
            }

            var ventas = await response.Content.ReadFromJsonAsync<List<VentaDto>>(_jsonOptions);
            return ventas ?? new List<VentaDto>();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al obtener ventas: {ex.Message}", ex);
        }
    }

    public async Task<VentaDto?> ObtenerVentaPorIdAsync(string id)
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
            throw new Exception($"Error al obtener venta: {ex.Message}", ex);
        }
    }

    public async Task<ReporteCompletoDto> GenerarReporteVentasAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        try
        {
            var client = CreateClient();
            var url = $"/api/ventas/reportes/ventas-periodo?fecha_inicio={fechaInicio:yyyy-MM-dd}&fecha_fin={fechaFin:yyyy-MM-dd}";
            
            var response = await client.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Error al generar reporte: {response.StatusCode}");
            }

            var reporte = await response.Content.ReadFromJsonAsync<ReporteCompletoDto>(_jsonOptions);
            return reporte ?? throw new Exception("No se recibió el reporte");
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al generar reporte: {ex.Message}", ex);
        }
    }

    public async Task<ProyeccionDemandaDto> GenerarProyeccionDemandaAsync(int diasProyeccion = 30)
    {
        try
        {
            var client = CreateClient();
            var url = $"/api/ventas/reportes/proyeccion-demanda?dias_proyeccion={diasProyeccion}";
            
            var response = await client.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Error al generar proyección: {response.StatusCode}");
            }

            var proyeccion = await response.Content.ReadFromJsonAsync<ProyeccionDemandaDto>(_jsonOptions);
            return proyeccion ?? throw new Exception("No se recibió la proyección");
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al generar proyección: {ex.Message}", ex);
        }
    }

    public async Task<object> ObtenerVentasDiariasAsync(DateTime fecha)
    {
        try
        {
            var client = CreateClient();
            var url = $"/api/ventas/reportes/ventas-diarias?fecha={fecha:yyyy-MM-dd}";
            
            var response = await client.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Error al obtener ventas diarias: {response.StatusCode}");
            }

            var contenido = await response.Content.ReadAsStringAsync();
            var ventasDiarias = JsonSerializer.Deserialize<object>(contenido, _jsonOptions);
            return ventasDiarias ?? new object();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al obtener ventas diarias: {ex.Message}", ex);
        }
    }
}