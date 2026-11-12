using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Opiniones.Extraction.Domain;
using Opiniones.Extraction.Infrastructure.Opciones;
using Opiniones.Extraction.Infrastructure.Rutas;

namespace Opiniones.Extraction.Infrastructure.Extractores;

public sealed class CsvExtractor : IExtractor
{
    private static readonly string[] EncabezadosEsperados =
    {
        "IdOpinion", "IdCliente", "IdProducto", "Fecha", "Comentario",
        "Clasificación", "PuntajeSatisfacción", "Fuente"
    };

    private readonly CsvOptions _opciones;
    private readonly ILogger<CsvExtractor> _logger;

    public CsvExtractor(IOptions<CsvOptions> opciones, ILogger<CsvExtractor> logger)
    {
        _opciones = opciones.Value;
        _logger = logger;
    }

    public FuenteDatos Fuente => FuenteDatos.Csv;
    public string Nombre => "Encuestas internas (CSV)";

    public async Task<ExtraccionResult> ExtraerAsync(CancellationToken cancellationToken)
    {
        var resultado = new ExtraccionResult { Fuente = Fuente, NombreFuente = Nombre };
        var ruta = RutaProyecto.Resolver(_opciones.Ruta);

        if (!File.Exists(ruta))
            throw new FileNotFoundException($"No existe el archivo CSV: {ruta}", ruta);

        var configuracion = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            Delimiter = string.IsNullOrWhiteSpace(_opciones.Delimitador) ? "," : _opciones.Delimitador,
            TrimOptions = TrimOptions.Trim,
            MissingFieldFound = null,
            HeaderValidated = null
        };

        using var lector = new StreamReader(ruta, Encoding.UTF8);
        using var csv = new CsvReader(lector, configuracion);

        await csv.ReadAsync();
        csv.ReadHeader();

        var encabezados = csv.HeaderRecord ?? Array.Empty<string>();
        if (encabezados.Length != EncabezadosEsperados.Length ||
            !encabezados.Select(h => h.Trim()).SequenceEqual(EncabezadosEsperados, StringComparer.OrdinalIgnoreCase))
        {
            resultado.Errores.Add(
                $"Encabezado invalido. Se esperaba: {string.Join(", ", EncabezadosEsperados)}. " +
                $"Se encontro: {string.Join(", ", encabezados)}.");

            _logger.LogWarning("El CSV {Ruta} tiene un encabezado invalido.", ruta);
            return resultado;
        }

        while (await csv.ReadAsync())
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var encuesta = csv.GetRecord<EncuestaCsv>();
                var nodo = JsonSerializer.SerializeToNode(encuesta) as JsonObject;

                if (nodo is not null)
                    resultado.Registros.Add(nodo);
            }
            catch (Exception ex)
            {
                resultado.Errores.Add($"Linea {csv.Parser.Row}: {ex.Message}");
            }
        }

        _logger.LogInformation(
            "CSV {Ruta}: {Total} encuestas extraidas, {Errores} errores.",
            ruta,
            resultado.Total,
            resultado.Errores.Count);

        return resultado;
    }
}
