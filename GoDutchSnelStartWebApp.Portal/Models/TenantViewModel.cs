namespace GoDutchSnelStartWebApp.Portal.Models;

public sealed class TenantViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? CustomerCode { get; set; }
    public string? CompanyName { get; set; }
    public string? ContactName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? PostalCode { get; set; }
    public string? City { get; set; }
    public string? KvkNumber { get; set; }
    public bool GoDutchEnabled { get; set; }
    public bool MyPosEnabled { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime? TrialStartsUtc { get; set; }
    public DateTime? TrialEndsUtc { get; set; }
    public DateTime CreatedUtc { get; set; }
}

public sealed class CreateTenantRequestViewModel
{
    public string Name { get; set; } = string.Empty;
    public string? CustomerCode { get; set; }
    public string? CompanyName { get; set; }
    public string? ContactName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? PostalCode { get; set; }
    public string? City { get; set; }
    public string? KvkNumber { get; set; }
    public bool GoDutchEnabled { get; set; }
    public bool MyPosEnabled { get; set; }
    public string? Status { get; set; }
    public bool? IsActive { get; set; }
}

public sealed class UpdateTenantRequestViewModel
{
    public string Name { get; set; } = string.Empty;
    public string? CustomerCode { get; set; }
    public string? CompanyName { get; set; }
    public string? ContactName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? PostalCode { get; set; }
    public string? City { get; set; }
    public string? KvkNumber { get; set; }
    public bool GoDutchEnabled { get; set; }
    public bool MyPosEnabled { get; set; }
    public string Status { get; set; } = "Draft";
    public bool IsActive { get; set; } = true;
}

public sealed class SelfChangePasswordRequestViewModel
{
    public string Username { get; set; } = string.Empty;
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string? NotificationEmail { get; set; }
}

public sealed class SubscriptionChangeRequestViewModel
{
    public bool GoDutchEnabled { get; set; }
    public bool MyPosEnabled { get; set; }
}

public sealed class SubscriptionCancelRequestViewModel
{
    public string? Reason { get; set; }
}

public sealed class NewModuleCredentialViewModel
{
    public string Label { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
