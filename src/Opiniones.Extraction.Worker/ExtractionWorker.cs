using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Opiniones.Extraction.Application;
using Opiniones.Extraction.Domain;
using Opiniones.Extraction.Infrastructure.Logging;

namespace Opiniones.Extraction.Worker;

public sealed class ExtractionWorker : BackgroundService
{
    private readonly IOrquestadorExtraccion _orquestador;
    private readonly LoggerService _loggerService;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly ILogger<ExtractionWorker> _logger;

    public ExtractionWorker(
        IOrquestadorExtraccion orquestador,
        LoggerService loggerService,
        IHostApplicationLifetime lifetime,
        ILogger<ExtractionWorker> logger)
    {
        _orquestador = orquestador;
        _loggerService = loggerService;
        _lifetime = lifetime;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _loggerService.RegistrarInicio("Actividad 1, extraccion ETL de Opiniones");

            var resumen = await _orquestador.EjecutarAsync(stoppingToken);
            _loggerService.RegistrarResumen(
                resumen.TotalRegistros,
                resumen.TotalErrores,
                resumen.DuracionTotal);

            ImprimirResumen(resumen);

            if (resumen.TotalErrores > 0)
                Environment.ExitCode = 1;
        }
        catch (Exception ex)
        {
            Environment.ExitCode = 1;
            _logger.LogError(ex, "La extraccion fallo.");
        }
        finally
        {
            _lifetime.StopApplication();
        }
    }

    private static void ImprimirResumen(ResumenExtraccion resumen)
    {
        Console.WriteLine();
        Console.WriteLine("======================================================================");
        Console.WriteLine(" RESUMEN DE EXTRACCION");
        Console.WriteLine("======================================================================");
        Console.WriteLine($"{"Fuente",-38}{"Registros",12}{"Errores",10}{"Duracion ms",14}");
        Console.WriteLine(new string('-', 74));

        foreach (var resultado in resumen.Resultados)
        {
            Console.WriteLine(
                $"{resultado.NombreFuente,-38}{resultado.Total,12}{resultado.Errores.Count,10}" +
                $"{resultado.Duracion.TotalMilliseconds,14:0}");
        }

        Console.WriteLine(new string('-', 74));
        Console.WriteLine(
            $"{"TOTAL",-38}{resumen.TotalRegistros,12}{resumen.TotalErrores,10}" +
            $"{resumen.DuracionTotal.TotalMilliseconds,14:0}");
        Console.WriteLine();
        Console.WriteLine($"Salida: {resumen.CarpetaSalida}");
        Console.WriteLine("======================================================================");
        Console.WriteLine();
    }
}
