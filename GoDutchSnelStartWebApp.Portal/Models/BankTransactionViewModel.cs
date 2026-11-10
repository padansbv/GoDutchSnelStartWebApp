namespace GoDutchSnelStartWebApp.Portal.Models;

public sealed class BankTransactionViewModel
{
    public string Id { get; set; } = string.Empty;
    public DateTime? BookingDate { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal? BalanceAfter { get; set; }
    public string? Currency { get; set; }
    public string? Status { get; set; }
}
