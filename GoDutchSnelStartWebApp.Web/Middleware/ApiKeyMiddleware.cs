using System.Security.Cryptography;
using System.Text;
using GoDutchSnelStartWebApp.Web.Configuration;
using Microsoft.Extensions.Options;

namespace GoDutchSnelStartWebApp.Web.Middleware;

public sealed class ApiKeyMiddleware
{
    private const string HeaderName = "X-Api-Key";

    // Deze endpoints zijn bewust publiek: de health-check voor monitoring, en de
    // leadformulier-endpoint die rechtstreeks door een browser op de publieke landingspagina
    // wordt aangeroepen (beperkt door de "LandingPage" CORS-policy, niet door een API-key).
    private static readonly string[] ExemptPathPrefixes =
    {
        "/api/health",
        "/health",
        "/api/godutch-leads"
    };

    private readonly RequestDelegate _next;
    private readonly ILogger<ApiKeyMiddleware> _logger;

    public ApiKeyMiddleware(RequestDelegate next, ILogger<ApiKeyMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IOptions<ApiSecurityOptions> options)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        if (Array.Exists(ExemptPathPrefixes, prefix => path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
        {
            await _next(context);
            return;
        }

        var configuredKey = options.Value.ApiKey;

        if (string.IsNullOrWhiteSpace(configuredKey))
        {
            // Zonder geconfigureerde sleutel zou de API openstaan voor iedereen; dat is erger
            // dan even plat liggen, dus weigeren we expliciet i.p.v. "open by default".
            _logger.LogError(
                "ApiSecurity:ApiKey is niet geconfigureerd; verzoek naar {Path} wordt geweigerd.",
                path);
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            await context.Response.WriteAsync("API security is not configured.");
            return;
        }

        var providedKey = context.Request.Headers[HeaderName].ToString();

        if (string.IsNullOrEmpty(providedKey) || !KeysMatch(providedKey, configuredKey))
        {
            _logger.LogWarning(
                "Verzoek naar {Path} geweigerd: ontbrekende of onjuiste {Header}-header.",
                path,
                HeaderName);
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Missing or invalid API key.");
            return;
        }

        await _next(context);
    }

    private static bool KeysMatch(string provided, string configured)
    {
        var providedBytes = Encoding.UTF8.GetBytes(provided);
        var configuredBytes = Encoding.UTF8.GetBytes(configured);

        return CryptographicOperations.FixedTimeEquals(providedBytes, configuredBytes);
    }
}
