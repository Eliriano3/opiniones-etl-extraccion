using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Opiniones.Extraction.Domain;

namespace Opiniones.Extraction.Application;

public sealed class OrquestadorExtraccion : IOrquestadorExtraccion
{
    private readonly IReadOnlyList<IExtractor> _extractores;
    private readonly IDataLoader _dataLoader;
    private readonly ILogger<OrquestadorExtraccion> _logger;
    private readonly OrquestacionOptions _opciones;

    public OrquestadorExtraccion(
        IEnumerable<IExtractor> extractores,
        IDataLoader dataLoader,
        IOptions<OrquestacionOptions> opciones,
        ILogger<OrquestadorExtraccion> logger)
    {
        _extractores = extractores.ToList();
        _dataLoader = dataLoader;
        _logger = logger;
        _opciones = opciones.Value;
    }

    public async Task<ResumenExtraccion> EjecutarAsync(CancellationToken cancellationToken)
    {
        var cronometro = Stopwatch.StartNew();
        _logger.LogInformation(
            "Iniciando extraccion con {Cantidad} fuentes. Modo paralelo: {Paralelo}.",
            _extractores.Count,
            _opciones.Paralelo);

        List<ExtraccionResult> resultados;

        if (_opciones.Paralelo)
        {
            var tareas = _extractores.Select(e => EjecutarExtractorAsync(e, cancellationToken));
            resultados = (await Task.WhenAll(tareas)).ToList();
        }
        else
        {
            resultados = new List<ExtraccionResult>();
            foreach (var extractor in _extractores)
                resultados.Add(await EjecutarExtractorAsync(extractor, cancellationToken));
        }

        var carpeta = await _dataLoader.GuardarAsync(resultados, cancellationToken);
        cronometro.Stop();

        _logger.LogInformation(
            "Extraccion finalizada. Registros: {Total}. Errores: {Errores}. Duracion: {Duracion} ms. Salida: {Carpeta}.",
            resultados.Sum(r => r.Total),
            resultados.Sum(r => r.Errores.Count),
            cronometro.ElapsedMilliseconds,
            carpeta);

        return new ResumenExtraccion
        {
            Resultados = resultados,
            DuracionTotal = cronometro.Elapsed,
            CarpetaSalida = carpeta
        };
    }

    private async Task<ExtraccionResult> EjecutarExtractorAsync(
        IExtractor extractor,
        CancellationToken cancellationToken)
    {
        var cronometro = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation("Extrayendo desde {Fuente}.", extractor.Nombre);
            var resultado = await extractor.ExtraerAsync(cancellationToken);
            resultado.Duracion = cronometro.Elapsed;

            _logger.LogInformation(
                "{Fuente}: {Total} registros y {Errores} errores en {Duracion} ms.",
                extractor.Nombre,
                resultado.Total,
                resultado.Errores.Count,
                cronometro.ElapsedMilliseconds);

            return resultado;
        }
        catch (Exception ex)
        {
            cronometro.Stop();
            _logger.LogError(ex, "Fallo la extraccion de {Fuente}.", extractor.Nombre);

            return new ExtraccionResult
            {
                Fuente = extractor.Fuente,
                NombreFuente = extractor.Nombre,
                Duracion = cronometro.Elapsed,
                Errores = { ex.Message }
            };
        }
    }
}
