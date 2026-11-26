using System.Reflection;
using GoDutchSnelStartWebApp.Application.Abstractions.Repositories;
using GoDutchSnelStartWebApp.Application.SnelStartAdministrations.Interfaces;
using GoDutchSnelStartWebApp.Web.Controllers;
using GoDutchSnelStartWebApp.Web.Controllers.MyPos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GoDutchSnelStartWebApp.Web.Security;

/// <summary>
/// Dwingt tenant-isolatie af op basis van de identiteit die de Portal meestuurt
/// (X-User-Module / X-User-Tenant-Id), zodat de API dit zelf controleert i.p.v. alleen te
/// vertrouwen op de Portal-UI die de tenant al filtert. Beheerder-aanroepen mogen over tenants
/// heen werken en worden hier nooit geblokkeerd.
///
/// Dekking:
/// - Elke actie met een Guid "tenantId" (als route-parameter, of als eigenschap op een
///   [FromBody]-request zoals CreateSnelStartAdministrationRequest.TenantId).
/// - Endpoints die alleen een gekoppeld id hebben (bv. bankAccountId, of een koppel-/
///   administratie-id) zoeken de eigenaar-tenant zelf op via de bijbehorende repository/service.
/// - Controllers die geen tenant-data tonen maar wel gevoelig zijn (gebruikersbeheer, globale
///   achtergrondtaak-instellingen, systeembrede notificaties) zijn hier beheerder-only gemaakt.
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

    private static readonly string[] AppUsersControllerBeheerderOnlyActions =
    {
        nameof(AppUsersController.GetAll),
        nameof(AppUsersController.GetById),
        nameof(AppUsersController.Create),
        nameof(AppUsersController.Update),
        nameof(AppUsersController.ChangePassword),
        nameof(AppUsersController.Delete)
    };

    private readonly IBankAccountRepository _bankAccountRepository;
    private readonly IBankAccountSnelStartLinkService _bankAccountSnelStartLinkService;
    private readonly ISnelStartAdministrationService _snelStartAdministrationService;

    public TenantAccessFilter(
        IBankAccountRepository bankAccountRepository,
        IBankAccountSnelStartLinkService bankAccountSnelStartLinkService,
        ISnelStartAdministrationService snelStartAdministrationService)
    {
        _bankAccountRepository = bankAccountRepository;
        _bankAccountSnelStartLinkService = bankAccountSnelStartLinkService;
        _snelStartAdministrationService = snelStartAdministrationService;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var module = context.HttpContext.Request.Headers[ModuleHeader].ToString();

        // Geen module-header betekent: geen ingelogde Portal-sessie bij dit verzoek (login,
        // wachtwoord-reset, of een endpoint dat al zonder X-Api-Key is uitgezonderd).
        if (string.IsNullOrWhiteSpace(module) ||
            string.Equals(module, BeheerderModule, StringComparison.OrdinalIgnoreCase))
        {
            await next();
            return;
        }

        if (IsBeheerderOnlyAction(context))
        {
            context.Result = Forbidden("Deze actie is alleen beschikbaar voor Beheerder.");
            return;
        }

        var requiredTenantId = await TryResolveTenantIdAsync(context, context.HttpContext.RequestAborted);

        if (requiredTenantId.HasValue && !MatchesSessionTenant(context, requiredTenantId.Value))
        {
            context.Result = Forbidden("Niet toegestaan voor deze tenant.");
            return;
        }

        await next();
    }

    private static bool IsBeheerderOnlyAction(ActionExecutingContext context)
    {
        if (context.Controller is NotificationsController || context.Controller is MyPosAutoSyncController)
        {
            return true;
        }

        if (context.Controller is AppUsersController)
        {
            var actionName = GetActionName(context);
            return AppUsersControllerBeheerderOnlyActions.Contains(actionName, StringComparer.OrdinalIgnoreCase);
        }

        if (context.Controller is TenantsController)
        {
            var actionName = GetActionName(context);
            return TenantsControllerBeheerderOnlyActions.Contains(actionName, StringComparer.OrdinalIgnoreCase);
        }

        return false;
    }

    private async Task<Guid?> TryResolveTenantIdAsync(ActionExecutingContext context, CancellationToken cancellationToken)
    {
        if (TryFindGuidValue(context, "tenantId", out var directTenantId))
        {
            return directTenantId;
        }

        // TenantsController: de route-parameter "id" IS de tenant zelf (GetById/Update/Delete).
        if (context.Controller is TenantsController &&
            context.ActionArguments.TryGetValue("id", out var tenantsId) && tenantsId is Guid tenantsControllerTenantId)
        {
            return tenantsControllerTenantId;
        }

        // Rechtstreeks of via [FromBody] meegegeven bankAccountId -> BankAccount.TenantId.
        if (TryFindGuidValue(context, "bankAccountId", out var bankAccountId))
        {
            var bankAccount = await _bankAccountRepository.GetByIdAsync(bankAccountId, cancellationToken);
            return bankAccount?.TenantId;
        }

        // BankAccountSnelStartLinksController: id = koppel-id -> BankAccountId -> TenantId.
        if (context.Controller is BankAccountSnelStartLinksController &&
            context.ActionArguments.TryGetValue("id", out var linkIdValue) && linkIdValue is Guid linkId)
        {
            var link = await _bankAccountSnelStartLinkService.GetByIdAsync(linkId, cancellationToken);
            if (link is null)
            {
                return null;
            }

            var bankAccount = await _bankAccountRepository.GetByIdAsync(link.BankAccountId, cancellationToken);
            return bankAccount?.TenantId;
        }

        // SnelStartAdministrationsController: id = administratie-id -> TenantId.
        if (context.Controller is SnelStartAdministrationsController &&
            context.ActionArguments.TryGetValue("id", out var adminIdValue) && adminIdValue is Guid adminId)
        {
            var administration = await _snelStartAdministrationService.GetByIdAsync(adminId, cancellationToken);
            return administration?.TenantId;
        }

        return null;
    }

    private static bool TryFindGuidValue(ActionExecutingContext context, string name, out Guid value)
    {
        foreach (var argument in context.ActionArguments)
        {
            if (string.Equals(argument.Key, name, StringComparison.OrdinalIgnoreCase) && argument.Value is Guid direct)
            {
                value = direct;
                return true;
            }
        }

        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null || argument is Guid || argument is string)
            {
                continue;
            }

            var property = argument.GetType().GetProperty(
                name,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            if (property?.PropertyType == typeof(Guid) && property.GetValue(argument) is Guid nested)
            {
                value = nested;
                return true;
            }
        }

        value = Guid.Empty;
        return false;
    }

    private static bool MatchesSessionTenant(ActionExecutingContext context, Guid requiredTenantId)
    {
        var header = context.HttpContext.Request.Headers[TenantHeader].ToString();
        return Guid.TryParse(header, out var sessionTenantId) && sessionTenantId == requiredTenantId;
    }

    private static string? GetActionName(ActionExecutingContext context)
    {
        return context.ActionDescriptor.RouteValues.TryGetValue("action", out var action) ? action : null;
    }

    private static ObjectResult Forbidden(string message)
    {
        return new ObjectResult(new { statusCode = 403, message })
        {
            StatusCode = StatusCodes.Status403Forbidden
        };
    }
}
