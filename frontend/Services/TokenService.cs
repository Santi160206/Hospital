namespace FrontEndBlazor.Services;

/// <summary>
/// Servicio para almacenar el token JWT en memoria (Scoped).
/// Simplificado para un solo circuito de Blazor Server.
/// </summary>
public class TokenService
{
    private string? _token;
    
    public string? Token 
    { 
        get => _token;
        set => _token = value;
    }
    
    public bool HasToken => !string.IsNullOrEmpty(_token);
    
    public void ClearToken()
    {
        _token = null;
    }
}
