using CsvHelper.Configuration.Attributes;

namespace Opiniones.Api.Models;

public sealed class ComentarioSocial
{
    [Name("IdComment")]
    public string IdComment { get; set; } = "";

    [Name("IdCliente")]
    public string? IdCliente { get; set; }

    [Name("IdProducto")]
    public string IdProducto { get; set; } = "";

    [Name("Fuente")]
    public string Plataforma { get; set; } = "";

    [Name("Fecha")]
    public string Fecha { get; set; } = "";

    [Name("Comentario")]
    public string Comentario { get; set; } = "";
}
