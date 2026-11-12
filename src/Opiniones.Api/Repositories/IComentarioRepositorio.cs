using Opiniones.Api.Models;

namespace Opiniones.Api.Repositories;

public interface IComentarioRepositorio
{
    int Total { get; }
    IReadOnlyList<ComentarioSocial> ObtenerTodos();
    ComentarioSocial? ObtenerPorId(string id);
}
