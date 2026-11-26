using GoDutchSnelStartWebApp.Portal.Services;

namespace GoDutchSnelStartWebApp.Portal.Http;

/// <summary>
/// Stuurt de module en actieve tenant van de ingelogde Portal-gebruiker mee op elk verzoek naar
/// de Web API (X-User-Module / X-User-Tenant-Id), zodat de API zelf tenant-isolatie kan
/// afdwingen i.p.v. alleen te vertrouwen op de Portal-UI. Vóór het inloggen (login, wachtwoord-
/// reset) is er nog geen sessie, dus wordt er dan niets meegestuurd.
/// </summary>
public sealed class UserContextHandler : DelegatingHandler
{
    private readonly AppSession _session;

    public UserContextHandler(AppSession session)
    {
        _session = session;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (_session.IsLoggedIn)
        {
            request.Headers.Add("X-User-Module", _session.Module);

            if (_session.TenantId.HasValue)
            {
                request.Headers.Add("X-User-Tenant-Id", _session.TenantId.Value.ToString());
            }
        }

        return base.SendAsync(request, cancellationToken);
    }
}
