using GoDutchSnelStartWebApp.Domain.Entities.MyPos;
using GoDutchSnelStartWebApp.Domain.Enums;

namespace GoDutchSnelStartWebApp.Application.Abstractions.Repositories.MyPos;

public interface IMyPosExportBatchRepository
{
    Task<MyPosExportBatch?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MyPosExportBatch>> GetByTenantAsync(Guid tenantId, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default);
    Task CreateAsync(MyPosExportBatch batch, CancellationToken cancellationToken = default);
    Task UpdateStatusAsync(Guid id, MyPosExportBatchStatus status, string? validationMessage, string? errorMessage, DateTime modifiedUtc, CancellationToken cancellationToken = default);
    Task MarkExportedAsync(
    Guid batchId,
    string snelStartReference,
    DateTime exportedUtc,
    CancellationToken cancellationToken = default);

    Task MarkExportFailedAsync(
        Guid batchId,
        string errorMessage,
        DateTime modifiedUtc,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Draait een eerder geëxporteerde batch lokaal terug (nadat de bijbehorende boeking
    /// handmatig in SnelStart is verwijderd). Zet Status op Reverted; raakt de raw
    /// transacties niet aan -- dat doet IMyPosRawTransactionRepository.ResetExportedForBatchAsync.
    /// </summary>
    Task MarkRevertedAsync(
        Guid batchId,
        string reason,
        DateTime modifiedUtc,
        CancellationToken cancellationToken = default);
}
