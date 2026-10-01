using System.Data;
using Microsoft.Data.SqlClient;
using Biblioteca.Entidades.Modelos;
using Biblioteca.Datos.Interfaces;

namespace Biblioteca.Datos.Repositorios;

public class LibroRepositorio : ILibroRepositorio
{
    private readonly string _cs;
    public LibroRepositorio(string cs) => _cs = cs;

    public async Task<List<Libro>> ListarAsync(string? filtro = null)
    {
        var lista = new List<Libro>();
        await using var c = new SqlConnection(_cs);
        var query = @"SELECT L.LibroId, L.Titulo, L.ISBN, L.AutorId, L.Ejemplares, L.Activo, A.Nombre AS NombreAutor
                      FROM Libros L INNER JOIN Autores A ON L.AutorId = A.AutorId WHERE L.Activo = 1";
        if (!string.IsNullOrWhiteSpace(filtro))
            query += " AND (L.Titulo LIKE @filtro OR A.Nombre LIKE @filtro)";
        await using var cmd = new SqlCommand(query, c);
        if (!string.IsNullOrWhiteSpace(filtro))
            cmd.Parameters.AddWithValue("@filtro", $"%{filtro.Trim()}%");
        await c.OpenAsync();
        await using var r = await cmd.ExecuteReaderAsync();
        while (await r.ReadAsync())
            lista.Add(new Libro { LibroId = r.GetInt32(0), Titulo = r.GetString(1), ISBN = r.GetString(2), AutorId = r.GetInt32(3), Ejemplares = r.GetInt32(4), Activo = r.GetBoolean(5), NombreAutor = r.GetString(6) });
        return lista;
    }

    public async Task<List<Autor>> ListarAutoresAsync()
    {
        var lista = new List<Autor>();
        await using var c = new SqlConnection(_cs);
        await using var cmd = new SqlCommand("SELECT AutorId, Nombre FROM Autores WHERE Activo = 1", c);
        await c.OpenAsync();
        await using var r = await cmd.ExecuteReaderAsync();
        while (await r.ReadAsync())
            lista.Add(new Autor { AutorId = r.GetInt32(0), Nombre = r.GetString(1) });
        return lista;
    }

    public async Task<List<Libro>> ListarDisponiblesAsync(string? filtro = null)
    {
        var lista = new List<Libro>();
        await using var c = new SqlConnection(_cs);
        var query = @"SELECT L.LibroId, L.Titulo, L.ISBN, L.AutorId, L.Ejemplares, L.Activo, A.Nombre AS NombreAutor
                      FROM Libros L INNER JOIN Autores A ON L.AutorId = A.AutorId
                      WHERE L.Activo = 1 AND L.Ejemplares > 0";
        if (!string.IsNullOrWhiteSpace(filtro))
            query += " AND (L.Titulo LIKE @filtro OR A.Nombre LIKE @filtro)";
        await using var cmd = new SqlCommand(query, c);
        if (!string.IsNullOrWhiteSpace(filtro))
            cmd.Parameters.AddWithValue("@filtro", $"%{filtro.Trim()}%");
        await c.OpenAsync();
        await using var r = await cmd.ExecuteReaderAsync();
        while (await r.ReadAsync())
            lista.Add(new Libro { LibroId = r.GetInt32(0), Titulo = r.GetString(1), ISBN = r.GetString(2), AutorId = r.GetInt32(3), Ejemplares = r.GetInt32(4), Activo = r.GetBoolean(5), NombreAutor = r.GetString(6) });
        return lista;
    }

    public async Task<Libro?> ObtenerPorIdAsync(int id)
    {
        await using var c = new SqlConnection(_cs);
        await using var cmd = new SqlCommand("SELECT LibroId, Titulo, ISBN, AutorId, Ejemplares, Activo FROM Libros WHERE LibroId = @id", c);
        cmd.Parameters.AddWithValue("@id", id);
        await c.OpenAsync();
        await using var r = await cmd.ExecuteReaderAsync();
        if (await r.ReadAsync())
            return new Libro { LibroId = r.GetInt32(0), Titulo = r.GetString(1), ISBN = r.GetString(2), AutorId = r.GetInt32(3), Ejemplares = r.GetInt32(4), Activo = r.GetBoolean(5) };
        return null;
    }

    public async Task<int> CrearAsync(Libro libro)
    {
        await using var c = new SqlConnection(_cs);
        await using var cmd = new SqlCommand("INSERT INTO Libros (Titulo,ISBN,AutorId,Ejemplares) OUTPUT INSERTED.LibroId VALUES(@Titulo,@ISBN,@AutorId,@Ejemplares)", c);
        cmd.Parameters.AddWithValue("@Titulo", libro.Titulo); cmd.Parameters.AddWithValue("@ISBN", libro.ISBN);
        cmd.Parameters.AddWithValue("@AutorId", libro.AutorId); cmd.Parameters.AddWithValue("@Ejemplares", libro.Ejemplares);
        await c.OpenAsync();
        return (int)(await cmd.ExecuteScalarAsync())!;
    }

    public async Task ActualizarAsync(Libro libro)
    {
        await using var c = new SqlConnection(_cs);
        await using var cmd = new SqlCommand("UPDATE Libros SET Titulo=@Titulo,ISBN=@ISBN,AutorId=@AutorId,Ejemplares=@Ejemplares WHERE LibroId=@id", c);
        cmd.Parameters.AddWithValue("@Titulo", libro.Titulo); cmd.Parameters.AddWithValue("@ISBN", libro.ISBN);
        cmd.Parameters.AddWithValue("@AutorId", libro.AutorId); cmd.Parameters.AddWithValue("@Ejemplares", libro.Ejemplares);
        cmd.Parameters.AddWithValue("@id", libro.LibroId);
        await c.OpenAsync(); await cmd.ExecuteNonQueryAsync();
    }

    public async Task EliminarAsync(int id)
    {
        await using var c = new SqlConnection(_cs);
        await using var cmd = new SqlCommand("UPDATE Libros SET Activo=0 WHERE LibroId=@id", c);
        cmd.Parameters.AddWithValue("@id", id);
        await c.OpenAsync(); await cmd.ExecuteNonQueryAsync();
    }

    public async Task<bool> ExisteISBNAsync(string isbn, int? excludeLibroId = null)
    {
        await using var c = new SqlConnection(_cs);
        var q = "SELECT COUNT(1) FROM Libros WHERE ISBN=@isbn AND Activo=1" + (excludeLibroId.HasValue ? " AND LibroId!=@id" : "");
        await using var cmd = new SqlCommand(q, c);
        cmd.Parameters.AddWithValue("@isbn", isbn);
        if (excludeLibroId.HasValue) cmd.Parameters.AddWithValue("@id", excludeLibroId.Value);
        await c.OpenAsync();
        return (int)(await cmd.ExecuteScalarAsync())! > 0;
    }
}
