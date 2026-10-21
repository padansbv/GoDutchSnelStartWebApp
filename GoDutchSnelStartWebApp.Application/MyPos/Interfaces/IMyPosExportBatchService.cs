using GoDutchSnelStartWebApp.Application.MyPos.Dtos;

namespace GoDutchSnelStartWebApp.Application.MyPos.Interfaces;

public interface IMyPosExportBatchService
{
    Task<MyPosExportBatchDto> CreateConceptBatchAsync(
        Guid tenantId,
        CreateMyPosExportBatchRequest request,
        CancellationToken cancellationToken = default);

    Task<MyPosExportBatchDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MyPosExportBatchDto>> GetByTenantAsync(
        Guid tenantId,
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Draait een eerder geëxporteerde batch lokaal terug (Status wordt Reverted) en geeft de
    /// onderliggende raw transacties weer vrij voor een nieuwe batch. Gebruik dit pas nadat de
    /// bijbehorende boeking handmatig in SnelStart is verwijderd -- dit roept SnelStart niet aan.
    /// </summary>
    Task<MyPosExportBatchDto> RevertExportAsync(
        Guid tenantId,
        Guid batchId,
        string? reason,
        CancellationToken cancellationToken = default);
}
