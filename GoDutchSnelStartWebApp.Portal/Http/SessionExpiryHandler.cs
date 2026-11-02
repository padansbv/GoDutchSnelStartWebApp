using System.Net;
using GoDutchSnelStartWebApp.Portal.Services;

namespace GoDutchSnelStartWebApp.Portal.Http;

public sealed class SessionExpiryHandler : DelegatingHandler
{
    private readonly AppSession _session;

    public SessionExpiryHandler(AppSession session)
    {
        _session = session;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);

        // Alleen reageren als er al een ingelogde sessie was; anders triggert dit
        // ook op een gewone mislukte inlogpoging vanaf de landingspagina.
        if (response.StatusCode == HttpStatusCode.Unauthorized && _session.IsLoggedIn)
            _session.MarkSessionExpired();

        return response;
    }
}
