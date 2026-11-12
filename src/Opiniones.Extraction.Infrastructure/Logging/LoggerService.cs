using Microsoft.Extensions.Logging;

namespace Opiniones.Extraction.Infrastructure.Logging;

public sealed class LoggerService
{
    private readonly ILogger<LoggerService> _logger;

    public LoggerService(ILogger<LoggerService> logger)
    {
        _logger = logger;
    }

    public void RegistrarInicio(string aplicacion)
    {
        _logger.LogInformation("Iniciando {Aplicacion}.", aplicacion);
    }

    public void RegistrarResumen(int totalRegistros, int totalErrores, TimeSpan duracion)
    {
        _logger.LogInformation(
            "Resumen de extraccion: {Total} registros, {Errores} errores, {Duracion} ms.",
            totalRegistros,
            totalErrores,
            duracion.TotalMilliseconds);
    }

    public void RegistrarError(Exception ex, string mensaje)
    {
        _logger.LogError(ex, "{Mensaje}", mensaje);
    }
}
