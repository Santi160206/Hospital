using FrontEndBlazor.Components;
using FrontEndBlazor.Services;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Get backend URL from configuration or environment variable
var backendUrl = builder.Configuration["BackendUrl"] 
    ?? Environment.GetEnvironmentVariable("BACKEND_URL") 
    ?? "http://localhost:8000";

// Configure PUBLIC HttpClient (no authentication) - used for login/register
builder.Services.AddHttpClient("PublicApi", client => {
    client.BaseAddress = new Uri(backendUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Configure AUTHENTICATED HttpClient (with Bearer token) - used for protected endpoints
builder.Services.AddScoped<AuthMessageHandler>();
builder.Services.AddHttpClient("AuthenticatedApi", client => {
    client.BaseAddress = new Uri(backendUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
}).AddHttpMessageHandler<AuthMessageHandler>();

// Register TokenService as SINGLETON (one instance shared across all circuits)
// CRÍTICO: Debe ser Singleton para que AuthService y AuthMessageHandler usen la MISMA instancia
builder.Services.AddSingleton<TokenService>();

// Register AuthService with PUBLIC client (for login/register)
builder.Services.AddScoped<IAuthService>(sp => 
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var publicClient = httpClientFactory.CreateClient("PublicApi");
    var sessionStorage = sp.GetRequiredService<ProtectedSessionStorage>();
    var navManager = sp.GetRequiredService<NavigationManager>();
    var tokenService = sp.GetRequiredService<TokenService>();
    return new AuthService(publicClient, sessionStorage, navManager, tokenService);
});

// Register MedicamentoService (uses AuthenticatedApi client)
builder.Services.AddScoped<IMedicamentoService, MedicamentoService>();

builder.Services.AddScoped<IVentaService, VentaService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // Keep HSTS in production only; leave development untouched.
    app.UseHsts();
}

// Solo usar HTTPS redirection en producción (evita problemas en desarrollo)
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// The interactive server components and some endpoints include anti-forgery metadata.
// Middleware must be present to handle anti-forgery tokens. Register it here.
app.UseAntiforgery();

// Static assets and interactive components mapping.
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
