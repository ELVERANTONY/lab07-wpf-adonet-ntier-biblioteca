using Biblioteca.Entidades.Modelos;

namespace Biblioteca.Datos.Interfaces;

/// <summary>
/// Contrato de lectura/escritura de Libros. Devuelve entidades o List&lt;T&gt;,
/// nunca SqlDataReader ni DataTable (enunciado).
/// </summary>
public interface ILibroRepositorio
{
    Task<List<Libro>> ListarAsync(string? filtro = null);
    Task<List<Libro>> ListarDisponiblesAsync(string? filtro = null);
    Task<List<Autor>> ListarAutoresAsync();
    Task<Libro?> ObtenerPorIdAsync(int id);
    Task<int> CrearAsync(Libro libro);
    Task ActualizarAsync(Libro libro);
    Task EliminarAsync(int id);
    Task<bool> ExisteISBNAsync(string isbn, int? excludeLibroId = null);
}
