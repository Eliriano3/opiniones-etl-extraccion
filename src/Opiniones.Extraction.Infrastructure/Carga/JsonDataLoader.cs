using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Opiniones.Extraction.Domain;
using Opiniones.Extraction.Infrastructure.Opciones;
using Opiniones.Extraction.Infrastructure.Rutas;

namespace Opiniones.Extraction.Infrastructure.Carga;

public sealed class JsonDataLoader : IDataLoader
{
    private readonly SalidaOptions _opciones;
    private readonly ILogger<JsonDataLoader> _logger;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public JsonDataLoader(IOptions<SalidaOptions> opciones, ILogger<JsonDataLoader> logger)
    {
        _opciones = opciones.Value;
        _logger = logger;
    }

    public Task<string> GuardarAsync(
        IReadOnlyList<ExtraccionResult> resultados,
        CancellationToken cancellationToken)
    {
        var carpeta = RutaProyecto.Resolver(_opciones.Ruta);
        Directory.CreateDirectory(carpeta);

        var fuentes = new List<object>();

        foreach (var resultado in resultados)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var nombreArchivo = resultado.Fuente switch
            {
                FuenteDatos.Csv => "encuestas.json",
                FuenteDatos.BaseDatosRelacional => "resenas_web.json",
                FuenteDatos.ApiRest => "comentarios_sociales.json",
                _ => $"{resultado.Fuente}.json"
            };

            var rutaArchivo = Path.Combine(carpeta, nombreArchivo);
            var contenido = new
            {
                fuente = resultado.Fuente.ToString(),
                nombre = resultado.NombreFuente,
                total = resultado.Total,
                duracionMs = Math.Round(resultado.Duracion.TotalMilliseconds, 0),
                errores = resultado.Errores,
                registros = resultado.Registros
            };

            File.WriteAllText(
                rutaArchivo,
                JsonSerializer.Serialize(contenido, _jsonOptions),
                new UTF8Encoding(false));

            fuentes.Add(new
            {
                fuente = resultado.Fuente.ToString(),
                archivo = nombreArchivo,
                total = resultado.Total,
                duracionMs = Math.Round(resultado.Duracion.TotalMilliseconds, 0),
                errores = resultado.Errores.Count
            });
        }

        var manifiesto = new
        {
            fecha = DateTimeOffset.Now,
            totalRegistros = resultados.Sum(r => r.Total),
            totalErrores = resultados.Sum(r => r.Errores.Count),
            fuentes
        };

        var rutaManifiesto = Path.Combine(carpeta, "manifest.json");
        File.WriteAllText(
            rutaManifiesto,
            JsonSerializer.Serialize(manifiesto, _jsonOptions),
            new UTF8Encoding(false));

        _logger.LogInformation(
            "Datos extraidos guardados en {Carpeta}. Manifiesto: {Manifiesto}.",
            carpeta,
            rutaManifiesto);

        return Task.FromResult(carpeta);
    }
}
