namespace GoDutchSnelStartWebApp.Web.Configuration;

public sealed class ApiSecurityOptions
{
    public const string SectionName = "ApiSecurity";

    /// <summary>
    /// Shared secret that the Portal sends on every request via de X-Api-Key header.
    /// Zelfde waarde moet in de Portal's appsettings.Production.json staan.
    /// Wordt gezet in appsettings.Production.json op de server — niet in source control.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;
}
