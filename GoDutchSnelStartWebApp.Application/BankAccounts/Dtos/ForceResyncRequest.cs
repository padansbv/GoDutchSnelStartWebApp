namespace GoDutchSnelStartWebApp.Application.BankAccounts.Dtos;

public sealed class ForceResyncRequest
{
    public DateTime FromUtc { get; set; }
    public DateTime? ToUtc { get; set; }
}