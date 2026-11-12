namespace Opiniones.Extraction.Infrastructure.Opciones;

public sealed class CsvOptions
{
    public string Ruta { get; set; } = "data/input/encuestas.csv";
    public string Delimitador { get; set; } = ",";
}

public sealed class DatabaseOptions
{
    public string ConnectionString { get; set; } = "";
    public int TiempoEsperaSegundos { get; set; } = 30;
}

public sealed class ApiOptions
{
    public string BaseUrl { get; set; } = "http://localhost:5080/";
    public string Endpoint { get; set; } = "api/comentarios";
    public int TimeoutSegundos { get; set; } = 30;
    public int Reintentos { get; set; } = 3;
}

public sealed class SalidaOptions
{
    public string Ruta { get; set; } = "data/extracted";
}
