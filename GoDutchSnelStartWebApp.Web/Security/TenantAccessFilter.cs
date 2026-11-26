using GoDutchSnelStartWebApp.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GoDutchSnelStartWebApp.Web.Security;

/// <summary>
/// Dwingt tenant-isolatie af op basis van de identiteit die de Portal meestuurt
/// (X-User-Module / X-User-Tenant-Id), zodat de API dit zelf controleert i.p.v. alleen te
/// vertrouwen op de Portal-UI die de tenant al filtert. Beheerder-aanroepen mogen over tenants
/// heen werken en worden hier nooit geblokkeerd.
///
/// Dekking: elke actie met een Guid-parameter genaamd "tenantId" (zo goed als alle
/// tenant-gescopeerde endpoints volgen dit patroon), plus TenantsController specifiek — daar IS
/// de route-parameter "id" de tenant zelf, en zijn GetAll/Create/Onboard beheerder-only.
/// Endpoints zonder een tenantId-route-parameter (bv. een koppel-id waarvan de eigenaar-tenant
/// eerst zou moeten worden opgezocht, zoals BankAccountSnelStartLinksController of
/// AppUsersController bij een andere gebruiker dan zichzelf) vallen hier nog niet onder.
/// </summary>
public sealed class TenantAccessFilter : IAsyncActionFilter
{
    private const string ModuleHeader = "X-User-Module";
    private const string TenantHeader = "X-User-Tenant-Id";
    private const string BeheerderModule = "Beheerder";

    private static readonly string[] TenantsControllerBeheerderOnlyActions =
    {
        nameof(TenantsController.GetAll),
        nameof(TenantsController.Create),
        nameof(TenantsController.Onboard)
    };

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var module = context.HttpContext.Request.Headers[ModuleHeader].ToString();

        // Geen module-header betekent: geen ingelogde Portal-sessie bij dit verzoek (login,
        // wachtwoord-reset, of een endpoint dat al zonder X-Api-Key is uitgezonderd). Niets te
        // controleren.
        if (string.IsNullOrWhiteSpace(module))
        {
            await next();
            return;
        }

        if (string.Equals(module, BeheerderModule, StringComparison.OrdinalIgnoreCase))
        {
            await next();
            return;
        }

        if (context.Controller is TenantsController)
        {
            var actionName = context.ActionDescriptor.RouteValues.TryGetValue("action", out var action)
                ? action
                : null;

            if (TenantsControllerBeheerderOnlyActions.Contains(actionName, StringComparer.OrdinalIgnoreCase))
            {
                context.Result = Forbidden();
                return;
            }

            if (context.ActionArguments.TryGetValue("id", out var idValue) && idValue is Guid tenantsControllerId)
            {
                if (!MatchesSessionTenant(context, tenantsControllerId))
                {
                    context.Result = Forbidden();
                    return;
                }

                await next();
                return;
            }
        }

        if (context.ActionArguments.TryGetValue("tenantId", out var tenantIdValue) && tenantIdValue is Guid routeTenantId)
        {
            if (!MatchesSessionTenant(context, routeTenantId))
            {
                context.Result = Forbidden();
                return;
            }
        }

        await next();
    }

    private static bool MatchesSessionTenant(ActionExecutingContext context, Guid routeTenantId)
    {
        var header = context.HttpContext.Request.Headers[TenantHeader].ToString();
        return Guid.TryParse(header, out var sessionTenantId) && sessionTenantId == routeTenantId;
    }

    private static ObjectResult Forbidden()
    {
        return new ObjectResult(new { statusCode = 403, message = "Niet toegestaan voor deze tenant." })
        {
            StatusCode = StatusCodes.Status403Forbidden
        };
    }
}
