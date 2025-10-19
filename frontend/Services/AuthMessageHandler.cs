namespace FrontEndBlazor.Services;

/// <summary>
/// HTTP Message Handler que agrega el Bearer token a las peticiones HTTP
/// </summary>
public class AuthMessageHandler : DelegatingHandler
{
    private readonly TokenService _tokenService;

    public AuthMessageHandler(TokenService tokenService)
    {
        _tokenService = tokenService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Obtener token del servicio en memoria
        var token = _tokenService.Token;
        
        // DEBUG: Log temporal
        Console.WriteLine($"[AuthMessageHandler] Request: {request.RequestUri}");
        Console.WriteLine($"[AuthMessageHandler] Token disponible: {(!string.IsNullOrEmpty(token) ? "SÍ" : "NO")}");
        
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            Console.WriteLine($"[AuthMessageHandler] Authorization header agregado");
        }
        else
        {
            Console.WriteLine($"[AuthMessageHandler] ⚠️ NO HAY TOKEN - Request sin autorización");
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
