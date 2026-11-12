using System.Text.Json.Nodes;

namespace Opiniones.Extraction.Domain;

public sealed class ExtraccionResult
{
    public FuenteDatos Fuente { get; init; }
    public string NombreFuente { get; init; } = "";
    public List<JsonObject> Registros { get; init; } = new();
    public List<string> Errores { get; init; } = new();
    public TimeSpan Duracion { get; set; }
    public int Total => Registros.Count;
    public bool Exitosa => Errores.Count == 0;
}
