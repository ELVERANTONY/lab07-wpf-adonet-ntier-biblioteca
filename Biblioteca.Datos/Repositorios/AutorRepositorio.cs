using Biblioteca.Datos.Interfaces;
using Biblioteca.Entidades.Modelos;
using Microsoft.Data.SqlClient;

namespace Biblioteca.Datos.Repositorios;

public class AutorRepositorio : IAutorRepositorio
{
    private readonly string _cs;

    public AutorRepositorio(string cs) => _cs = cs;

    public async Task<List<Autor>> ListarActivosAsync()
    {
        var autores = new List<Autor>();
        await using var c = new SqlConnection(_cs);
        await using var cmd = new SqlCommand("SELECT AutorId, Nombre, Nacionalidad, Activo FROM Autores WHERE Activo = 1 ORDER BY Nombre", c);
        await c.OpenAsync();
        await using var r = await cmd.ExecuteReaderAsync();
        while (await r.ReadAsync())
        {
            autores.Add(new Autor
            {
                AutorId = r.GetInt32(0),
                Nombre = r.GetString(1),
                Nacionalidad = r.GetString(2),
                Activo = r.GetBoolean(3)
            });
        }
        return autores;
    }
}
