namespace Opiniones.Extraction.Domain;

public interface IExtractor
{
    FuenteDatos Fuente { get; }
    string Nombre { get; }
    Task<ExtraccionResult> ExtraerAsync(CancellationToken cancellationToken);
}
