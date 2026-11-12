using Microsoft.Extensions.Options;
using Opiniones.Extraction.Application;
using Opiniones.Extraction.Domain;
using Opiniones.Extraction.Infrastructure.Carga;
using Opiniones.Extraction.Infrastructure.Extractores;
using Opiniones.Extraction.Infrastructure.Logging;
using Opiniones.Extraction.Infrastructure.Opciones;
using Opiniones.Extraction.Infrastructure.Rutas;
using Opiniones.Extraction.Worker;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        Path.Combine(RutaProyecto.Obtener(), "logs", "extraccion-.log"),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7)
    .CreateLogger();

try
{
    var builder = Host.CreateApplicationBuilder(args);

    builder.Logging.ClearProviders();
    builder.Logging.AddSerilog(Log.Logger, dispose: true);

    builder.Services.Configure<CsvOptions>(builder.Configuration.GetSection("Extraction:Csv"));
    builder.Services.Configure<ApiOptions>(builder.Configuration.GetSection("Extraction:Api"));
    builder.Services.Configure<SalidaOptions>(builder.Configuration.GetSection("Extraction:Salida"));
    builder.Services.Configure<OrquestacionOptions>(builder.Configuration.GetSection("Extraction:Orquestacion"));

    builder.Services.Configure<DatabaseOptions>(opciones =>
    {
        opciones.ConnectionString = builder.Configuration.GetConnectionString("OpinionesWebDB") ?? "";
        opciones.TiempoEsperaSegundos =
            builder.Configuration.GetValue<int?>("Extraction:Database:TiempoEsperaSegundos") ?? 30;
    });

    builder.Services.AddHttpClient("comentarios", (proveedor, cliente) =>
    {
        var opciones = proveedor.GetRequiredService<IOptions<ApiOptions>>().Value;
        cliente.BaseAddress = new Uri(opciones.BaseUrl, UriKind.Absolute);
        cliente.Timeout = TimeSpan.FromSeconds(opciones.TimeoutSegundos);
    });

    builder.Services.AddTransient<IExtractor, CsvExtractor>();
    builder.Services.AddTransient<IExtractor, DatabaseExtractor>();
    builder.Services.AddTransient<IExtractor, ApiExtractor>();
    builder.Services.AddSingleton<IDataLoader, JsonDataLoader>();
    builder.Services.AddSingleton<IOrquestadorExtraccion, OrquestadorExtraccion>();
    builder.Services.AddSingleton<LoggerService>();
    builder.Services.AddHostedService<ExtractionWorker>();

    var host = builder.Build();
    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "La aplicacion termino de forma inesperada.");
    Environment.ExitCode = 1;
}
finally
{
    await Log.CloseAndFlushAsync();
}
