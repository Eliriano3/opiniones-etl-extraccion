using Opiniones.Extraction.Domain;

namespace Opiniones.Extraction.Application;

public interface IOrquestadorExtraccion
{
    Task<ResumenExtraccion> EjecutarAsync(CancellationToken cancellationToken);
}
