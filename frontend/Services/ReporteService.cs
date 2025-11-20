using System.Net.Http.Json;
using System.Text.Json;
using FrontEndBlazor.Models;

namespace FrontEndBlazor.Services;

public interface IReporteService
{
    Task<ComparacionPreciosResponse?> ComparacionPreciosAsync(FiltrosReporteRequest filtros);
    Task<ReporteComprasResponse?> ReporteComprasAsync(FiltrosReporteRequest filtros);
}

public class ReporteService : IReporteService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly JsonSerializerOptions _jsonOptions;

    public ReporteService(IHttpClientFactory httpClientFactory)
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

    public async Task<ComparacionPreciosResponse?> ComparacionPreciosAsync(FiltrosReporteRequest filtros)
    {
        try
        {
            var client = CreateClient();
            var response = await client.PostAsJsonAsync("/api/reportes/comparacion-precios", filtros, _jsonOptions);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ComparacionPreciosResponse>(_jsonOptions);
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Error en ComparacionPreciosAsync: {response.StatusCode} - {errorContent}");
            
            // Si es 400, puede ser error de validación (rango > 12 meses)
            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                throw new HttpRequestException($"Error de validación: {errorContent}");
            }
            
            return null;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"HttpRequestException en ComparacionPreciosAsync: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception en ComparacionPreciosAsync: {ex.Message}");
            throw;
        }
    }

    public async Task<ReporteComprasResponse?> ReporteComprasAsync(FiltrosReporteRequest filtros)
    {
        try
        {
            var client = CreateClient();
            var response = await client.PostAsJsonAsync("/api/reportes/compras", filtros, _jsonOptions);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ReporteComprasResponse>(_jsonOptions);
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Error en ReporteComprasAsync: {response.StatusCode} - {errorContent}");
            
            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                throw new HttpRequestException($"Error de validación: {errorContent}");
            }
            
            return null;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"HttpRequestException en ReporteComprasAsync: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception en ReporteComprasAsync: {ex.Message}");
            throw;
        }
    }
}
