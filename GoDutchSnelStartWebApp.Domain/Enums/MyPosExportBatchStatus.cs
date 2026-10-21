namespace GoDutchSnelStartWebApp.Domain.Enums;

public enum MyPosExportBatchStatus
{
    Concept,
    Ready,
    Exported,
    Failed,
    ExportFailed,

    /// <summary>
    /// De export is lokaal teruggedraaid nadat de bijbehorende boeking handmatig in
    /// SnelStart is verwijderd. De onderliggende raw transacties zijn weer vrijgegeven
    /// (IsExported=0) zodat er een nieuwe batch voor gemaakt kan worden.
    /// </summary>
    Reverted
}
