namespace GoDutchSnelStartWebApp.Application.MyPos.Dtos;

public sealed class RevertMyPosExportBatchRequest
{
    /// <summary>
    /// Optionele toelichting, bv. "Grootboek 8214 bleek verwijderd, boeking handmatig
    /// gecorrigeerd in SnelStart." Als leeg wordt een standaardtekst gebruikt.
    /// </summary>
    public string? Reason { get; set; }
}
