using Biblioteca.Entidades.Modelos;

namespace Biblioteca.Datos.Interfaces;

public interface ILibroRepositorio
{
    Task<List<Libro>> ListarAsync(string? filtro = null);
    Task<int> CrearAsync(Libro libro);
    Task ActualizarAsync(Libro libro);
    Task EliminarAsync(int id);
    Task<Libro?> ObtenerPorIdAsync(int id);
    Task<bool> ExisteISBNAsync(string isbn, int? excludeLibroId = null);
    Task<List<Autor>> ListarAutoresAsync();
}
