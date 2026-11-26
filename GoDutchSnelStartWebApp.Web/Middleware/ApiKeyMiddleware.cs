using System.Security.Cryptography;
using System.Text;
using GoDutchSnelStartWebApp.Web.Configuration;
using Microsoft.Extensions.Options;

namespace GoDutchSnelStartWebApp.Web.Middleware;

public sealed class ApiKeyMiddleware
{
    private const string HeaderName = "X-Api-Key";

    // Deze endpoints zijn bewust publiek, dus NOOIT achter de API-key: de health-check voor
    // monitoring; het leadformulier en de publieke aanmelding die rechtstreeks door een browser
    // op de publieke landingspagina worden aangeroepen; en de GoDutch-webhook, die rechtstreeks
    // door GoDutch's eigen servers wordt aangeroepen (die kennen onze API-key niet).
    private static readonly string[] ExemptPathPrefixes =
    {
        "/api/health",
        "/health",
        "/api/godutch-leads",
        "/api/public/register",
        "/api/godutch-webhooks"
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
