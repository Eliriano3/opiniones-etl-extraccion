using System.Data;
using System.Globalization;
using System.Text.Json.Nodes;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Opiniones.Extraction.Domain;
using Opiniones.Extraction.Infrastructure.Opciones;

namespace Opiniones.Extraction.Infrastructure.Extractores;

public sealed class DatabaseExtractor : IExtractor
{
    private const string Consulta = """
        SELECT r.IdResena,
               c.Email,
               p.Codigo,
               r.Calificacion,
               r.Titulo,
               r.Comentario,
               r.FechaPublicacion,
               r.Estado
          FROM dbo.Resena r
          INNER JOIN dbo.Cliente c  ON c.IdCliente = r.IdCliente
          INNER JOIN dbo.Producto p ON p.IdProducto = r.IdProducto
         ORDER BY r.IdResena;
        """;

    private readonly DatabaseOptions _opciones;
    private readonly ILogger<DatabaseExtractor> _logger;

    public DatabaseExtractor(IOptions<DatabaseOptions> opciones, ILogger<DatabaseExtractor> logger)
    {
        _opciones = opciones.Value;
        _logger = logger;
    }

    public FuenteDatos Fuente => FuenteDatos.BaseDatosRelacional;
    public string Nombre => "Resenas web (base de datos)";

    public async Task<ExtraccionResult> ExtraerAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_opciones.ConnectionString))
        {
            throw new InvalidOperationException(
                "Falta la cadena de conexion. Configure ConnectionStrings__OpinionesWebDB o appsettings.");
        }

        var resultado = new ExtraccionResult { Fuente = Fuente, NombreFuente = Nombre };

        await using var conexion = new SqlConnection(_opciones.ConnectionString);
        await conexion.OpenAsync(cancellationToken);

        await using var comando = new SqlCommand(Consulta, conexion)
        {
            CommandTimeout = _opciones.TiempoEsperaSegundos
        };

        await using var lector = await comando.ExecuteReaderAsync(cancellationToken);

        while (await lector.ReadAsync(cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var registro = new JsonObject
            {
                ["IdResena"] = lector.GetInt32(0),
                ["ClienteEmail"] = lector.GetString(1),
                ["ProductoCodigo"] = lector.GetString(2),
                ["Calificacion"] = lector.GetByte(3),
                ["Titulo"] = lector.IsDBNull(4) ? null : lector.GetString(4),
                ["Comentario"] = lector.IsDBNull(5) ? null : lector.GetString(5),
                ["FechaPublicacion"] = lector.GetDateTime(6).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                ["Estado"] = lector.GetString(7)
            };

            resultado.Registros.Add(registro);
        }

        _logger.LogInformation(
            "Base de datos: {Total} resenas extraidas de OpinionesWebDB.",
            resultado.Total);

        return resultado;
    }
}
