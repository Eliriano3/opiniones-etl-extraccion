using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Opiniones.Extraction.Domain;
using Opiniones.Extraction.Infrastructure.Opciones;

namespace Opiniones.Extraction.Infrastructure.Extractores;

public sealed class ApiExtractor : IExtractor
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ApiOptions _opciones;
    private readonly ILogger<ApiExtractor> _logger;

    public ApiExtractor(
        IHttpClientFactory httpClientFactory,
        IOptions<ApiOptions> opciones,
        ILogger<ApiExtractor> logger)
    {
        _httpClientFactory = httpClientFactory;
        _opciones = opciones.Value;
        _logger = logger;
    }

    public FuenteDatos Fuente => FuenteDatos.ApiRest;
    public string Nombre => "Comentarios sociales (API REST)";

    public async Task<ExtraccionResult> ExtraerAsync(CancellationToken cancellationToken)
    {
        var resultado = new ExtraccionResult { Fuente = Fuente, NombreFuente = Nombre };
        var cliente = _httpClientFactory.CreateClient("comentarios");
        var reintentos = Math.Max(1, _opciones.Reintentos);

        for (var intento = 1; intento <= reintentos; intento++)
        {
            try
            {
                using var respuesta = await cliente.GetAsync(_opciones.Endpoint, cancellationToken);
                respuesta.EnsureSuccessStatusCode();

                var contenido = await respuesta.Content.ReadAsStringAsync(cancellationToken);
                var nodo = JsonNode.Parse(contenido);

                if (nodo is not JsonArray arreglo)
                {
                    throw new InvalidOperationException(
                        "La API no devolvio un arreglo JSON en el endpoint configurado.");
                }

                foreach (var item in arreglo)
                {
                    if (item is JsonObject objeto)
                        resultado.Registros.Add(objeto);
                }

                _logger.LogInformation(
                    "API {Url}: {Total} comentarios extraidos en el intento {Intento}.",
                    $"{_opciones.BaseUrl.TrimEnd('/')}/{_opciones.Endpoint.TrimStart('/')}",
                    resultado.Total,
                    intento);

                return resultado;
            }
            catch (Exception ex) when (intento < reintentos && !cancellationToken.IsCancellationRequested)
            {
                var espera = TimeSpan.FromSeconds(Math.Pow(2, intento));
                _logger.LogWarning(
                    ex,
                    "Fallo el intento {Intento} de la API. Reintentando en {Espera} segundos.",
                    intento,
                    espera.TotalSeconds);

                await Task.Delay(espera, cancellationToken);
            }
        }

        return resultado;
    }
}
