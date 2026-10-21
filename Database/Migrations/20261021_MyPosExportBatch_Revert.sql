-- Maakt het mogelijk om een eerder geëxporteerde myPOS-exportbatch lokaal terug te draaien,
-- voor het geval de bijbehorende boeking handmatig in SnelStart is verwijderd. Draait de
-- batch terug naar Status='Reverted' (geen schemawijziging nodig: hergebruikt ErrorMessage
-- voor de reden en ModifiedUtc als tijdstip) en geeft de onderliggende raw transacties weer
-- vrij (IsExported=0, ExportBatchId=NULL) zodat er een nieuwe batch voor gemaakt kan worden.

CREATE PROCEDURE dbo.MyPosExportBatches_MarkReverted
    @Id uniqueidentifier,
    @Reason nvarchar(2000),
    @ModifiedUtc datetime2(7)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.MyPosExportBatches
    SET
        Status = 'Reverted',
        IsReadyForExport = 0,
        ErrorMessage = @Reason,
        ModifiedUtc = @ModifiedUtc
    WHERE Id = @Id;
END
GO

CREATE PROCEDURE dbo.MyPosRawTransactions_ResetExportedForBatch
    @TenantId uniqueidentifier,
    @ExportBatchId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.MyPosRawTransactions
    SET
        IsExported = 0,
        ExportBatchId = NULL
    WHERE TenantId = @TenantId
      AND ExportBatchId = @ExportBatchId;
END
GO
