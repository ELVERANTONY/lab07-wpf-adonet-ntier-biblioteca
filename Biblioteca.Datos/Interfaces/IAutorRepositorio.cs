using Biblioteca.Entidades.Modelos;

namespace Biblioteca.Datos.Interfaces;

/// <summary>
/// Una clase de acceso por entidad (enunciado). Autores solo requiere lectura,
/// porque el alta de autores no forma parte del laboratorio.
/// </summary>
public interface IAutorRepositorio
{
    Task<List<Autor>> ListarActivosAsync();
}
