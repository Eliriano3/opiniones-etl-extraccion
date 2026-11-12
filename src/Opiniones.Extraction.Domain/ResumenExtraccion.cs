namespace Opiniones.Extraction.Domain;

public sealed class ResumenExtraccion
{
    public IReadOnlyList<ExtraccionResult> Resultados { get; init; } = Array.Empty<ExtraccionResult>();
    public TimeSpan DuracionTotal { get; init; }
    public string CarpetaSalida { get; init; } = "";
    public int TotalRegistros => Resultados.Sum(r => r.Total);
    public int TotalErrores => Resultados.Sum(r => r.Errores.Count);
}
