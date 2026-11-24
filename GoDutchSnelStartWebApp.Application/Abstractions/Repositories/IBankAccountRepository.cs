using GoDutchSnelStartWebApp.Domain.Entities;

namespace GoDutchSnelStartWebApp.Application.Abstractions.Repositories;

public interface IBankAccountRepository
{
    Task<IReadOnlyList<BankAccount>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<BankAccount?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Zoekt bankaccounts op IBAN over alle tenants heen (niet gefilterd op tenant).
    /// Gebruikt om te detecteren of een IBAN al bij een andere tenant geregistreerd staat.
    /// </summary>
    Task<IReadOnlyList<BankAccount>> GetByIbanAsync(string iban, CancellationToken cancellationToken = default);
    Task CreateAsync(BankAccount bankAccount, CancellationToken cancellationToken = default);
    Task UpdateAsync(BankAccount bankAccount, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}