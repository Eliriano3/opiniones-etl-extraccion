using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using Opiniones.Api.Models;
using Opiniones.Api.Rutas;

namespace Opiniones.Api.Repositories;

public sealed class CsvComentarioRepositorio : IComentarioRepositorio
{
    private readonly List<ComentarioSocial> _comentarios;

    public CsvComentarioRepositorio(IConfiguration configuracion, ILogger<CsvComentarioRepositorio> logger)
    {
        var ruta = RutaProyecto.Resolver(
            configuracion["Api:ArchivoCsv"] ?? "data/input/comentarios_sociales.csv");

        if (!File.Exists(ruta))
            throw new FileNotFoundException($"No existe el CSV de comentarios: {ruta}", ruta);

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            TrimOptions = TrimOptions.Trim,
            MissingFieldFound = null,
            HeaderValidated = null
        };

        using var lector = new StreamReader(ruta, Encoding.UTF8);
        using var csv = new CsvReader(lector, config);

        _comentarios = csv.GetRecords<ComentarioSocial>().ToList();

        foreach (var comentario in _comentarios)
        {
            if (string.IsNullOrWhiteSpace(comentario.IdCliente))
                comentario.IdCliente = null;
        }

        logger.LogInformation(
            "API de comentarios: {Total} registros cargados desde {Ruta}.",
            _comentarios.Count,
            ruta);
    }

    public int Total => _comentarios.Count;

    public IReadOnlyList<ComentarioSocial> ObtenerTodos() => _comentarios;

    public ComentarioSocial? ObtenerPorId(string id) =>
        _comentarios.FirstOrDefault(c =>
            string.Equals(c.IdComment, id, StringComparison.OrdinalIgnoreCase));
}
