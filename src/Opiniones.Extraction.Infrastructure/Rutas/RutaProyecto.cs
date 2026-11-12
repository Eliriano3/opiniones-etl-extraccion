namespace Opiniones.Extraction.Infrastructure.Rutas;

public static class RutaProyecto
{
    public static string Obtener()
    {
        var directorio = new DirectoryInfo(AppContext.BaseDirectory);

        while (directorio is not null)
        {
            if (File.Exists(Path.Combine(directorio.FullName, "OpinionesExtraccion.sln")))
                return directorio.FullName;

            directorio = directorio.Parent;
        }

        return Directory.GetCurrentDirectory();
    }

    public static string Resolver(string ruta)
    {
        return Path.IsPathRooted(ruta)
            ? Path.GetFullPath(ruta)
            : Path.GetFullPath(Path.Combine(Obtener(), ruta));
    }
}
