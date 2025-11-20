using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace FrontEndBlazor.Services;

public class LoginRequest
{
    public string username { get; set; } = string.Empty;
    public string password { get; set; } = string.Empty;
}

public class RegisterRequest
{
    public string username { get; set; } = string.Empty;
    public string full_name { get; set; } = string.Empty;
    public string email { get; set; } = string.Empty;
    public string password { get; set; } = string.Empty;
    public string role { get; set; } = "farmaceutico";
}

public interface IAuthService
{
    Task<bool> LoginAsync(string username, string password);
    Task<bool> RegisterAsync(RegisterRequest request);
    Task LogoutAsync();
    Task<bool> IsAuthenticatedAsync();
    Task LoadTokenAsync();
    Task<string> GetUserRoleAsync();
}

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly ProtectedSessionStorage _sessionStorage;
    private readonly NavigationManager _navigationManager;
    private readonly TokenService _tokenService;
    private const string TOKEN_KEY = "auth_token";

    public AuthService(
        HttpClient httpClient, 
        ProtectedSessionStorage sessionStorage, 
        NavigationManager navigationManager,
        TokenService tokenService)
    {
        _httpClient = httpClient;
        _sessionStorage = sessionStorage;
        _navigationManager = navigationManager;
        _tokenService = tokenService;
    }

    public async Task<bool> LoginAsync(string username, string password)
    {
        try
        {
            var formData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("username", username),
                new KeyValuePair<string, string>("password", password),
                new KeyValuePair<string, string>("grant_type", "password"),
            });

            var response = await _httpClient.PostAsync("/api/auth/signin", formData);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = "Error al iniciar sesión";
                try
                {
                    var error = JsonSerializer.Deserialize<Dictionary<string, object>>(responseContent);
                    if (error != null && error.ContainsKey("detail"))
                        errorMessage = error["detail"]?.ToString() ?? errorMessage;
                }
                catch { errorMessage = responseContent; }
                throw new Exception(errorMessage);
            }

            var result = await response.Content.ReadFromJsonAsync<TokenResponse>();
            if (result == null || string.IsNullOrEmpty(result.access_token))
            {
                throw new Exception("No se recibió el token de acceso desde el servidor");
            }
            
            // Guardar en ProtectedSessionStorage y TokenService
            await _sessionStorage.SetAsync(TOKEN_KEY, result.access_token);
            _tokenService.Token = result.access_token;
            
            // DEBUG: Verificar que se guardó
            Console.WriteLine($"[AuthService.Login] ✅ Token guardado en TokenService: {result.access_token.Substring(0, 20)}...");
            Console.WriteLine($"[AuthService.Login] TokenService.Token = {(_tokenService.Token != null ? "SET" : "NULL")}");
            
            return true;
        }
        catch (HttpRequestException)
        {
            throw new Exception("No se pudo conectar con el servidor. Asegúrese de que el backend esté corriendo en http://localhost:8000");
        }
    }

    public async Task<bool> RegisterAsync(RegisterRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/users/", request);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = "Error al registrar usuario";
                try
                {
                    var error = JsonSerializer.Deserialize<Dictionary<string, object>>(content);
                    if (error != null && error.ContainsKey("detail"))
                        errorMessage = error["detail"]?.ToString() ?? errorMessage;
                }
                catch
                {
                    errorMessage = content;
                }
                throw new Exception(errorMessage);
            }

            return true;
        }
        catch (HttpRequestException)
        {
            throw new Exception("No se pudo conectar con el servidor. Asegúrese de que el backend esté corriendo.");
        }
    }

    public async Task LogoutAsync()
    {
        await _sessionStorage.DeleteAsync(TOKEN_KEY);
        _tokenService.ClearToken();
        _navigationManager.NavigateTo("/login", true);
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        try
        {
            var result = await _sessionStorage.GetAsync<string>(TOKEN_KEY);
            return result.Success && !string.IsNullOrEmpty(result.Value);
        }
        catch
        {
            return false;
        }
    }
    
    /// <summary>
    /// Carga el token desde ProtectedSessionStorage al TokenService
    /// </summary>
    public async Task LoadTokenAsync()
    {
        try
        {
            Console.WriteLine($"[AuthService.LoadToken] Intentando cargar token...");
            var result = await _sessionStorage.GetAsync<string>(TOKEN_KEY);
            
            if (result.Success && !string.IsNullOrEmpty(result.Value))
            {
                _tokenService.Token = result.Value;
                Console.WriteLine($"[AuthService.LoadToken] ✅ Token cargado: {result.Value.Substring(0, 20)}...");
                Console.WriteLine($"[AuthService.LoadToken] TokenService.Token = {(_tokenService.Token != null ? "SET" : "NULL")}");
            }
            else
            {
                Console.WriteLine($"[AuthService.LoadToken] ❌ No hay token en ProtectedSessionStorage");
                Console.WriteLine($"[AuthService.LoadToken] result.Success = {result.Success}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AuthService.LoadToken] ❌ Error al cargar token: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Obtiene el rol del usuario desde el token JWT
    /// </summary>
    public async Task<string> GetUserRoleAsync()
    {
        try
        {
            var result = await _sessionStorage.GetAsync<string>(TOKEN_KEY);
            if (!result.Success || string.IsNullOrEmpty(result.Value))
                return string.Empty;
            
            // Decodificar JWT (formato: header.payload.signature)
            var parts = result.Value.Split('.');
            if (parts.Length != 3)
                return string.Empty;
            
            // Decodificar payload (base64)
            var payload = parts[1];
            // Agregar padding si es necesario
            var padding = payload.Length % 4;
            if (padding > 0)
                payload += new string('=', 4 - padding);
            
            var payloadBytes = Convert.FromBase64String(payload);
            var payloadJson = System.Text.Encoding.UTF8.GetString(payloadBytes);
            
            // Parsear JSON y obtener rol
            var payloadData = JsonSerializer.Deserialize<Dictionary<string, object>>(payloadJson);
            if (payloadData != null && payloadData.ContainsKey("role"))
            {
                var role = payloadData["role"]?.ToString() ?? string.Empty;
                return role.ToLower();
            }
            
            return string.Empty;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AuthService.GetUserRole] Error: {ex.Message}");
            return string.Empty;
        }
    }
}

public class TokenResponse
{
    public string access_token { get; set; } = string.Empty;
    public string token_type { get; set; } = string.Empty;
}