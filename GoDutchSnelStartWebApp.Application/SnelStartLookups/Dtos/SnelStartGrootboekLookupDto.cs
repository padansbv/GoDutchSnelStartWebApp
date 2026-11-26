namespace GoDutchSnelStartWebApp.Application.SnelStartLookups.Dtos;

public sealed class SnelStartGrootboekLookupDto
{
    public Guid Id { get; set; }
    public string? Nummer { get; set; }
    public string? Omschrijving { get; set; }

    // De btw-soorten waarmee deze grootboekrekening in SnelStart zelf is ingericht
    // (bv. ["Laag"]); leeg of ["Geen"] betekent dat de rekening geen btw verwacht.
    public IReadOnlyList<string> BtwSoort { get; set; } = Array.Empty<string>();
}