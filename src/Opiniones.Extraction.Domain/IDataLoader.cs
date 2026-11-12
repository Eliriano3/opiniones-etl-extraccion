namespace Opiniones.Extraction.Domain;

public interface IDataLoader
{
    Task<string> GuardarAsync(
        IReadOnlyList<ExtraccionResult> resultados,
        CancellationToken cancellationToken);
}
