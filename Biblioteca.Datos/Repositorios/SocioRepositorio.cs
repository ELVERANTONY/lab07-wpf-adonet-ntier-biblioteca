using System.Data;
using Microsoft.Data.SqlClient;
using Biblioteca.Entidades.Modelos;
using Biblioteca.Datos.Interfaces;
using static Biblioteca.Datos.Repositorios.DbHelper;

namespace Biblioteca.Datos.Repositorios;

public class SocioRepositorio : ISocioRepositorio
{
    private readonly string _cs;
    public SocioRepositorio(string cs) => _cs = cs;

    public async Task<List<Socio>> ListarAsync(string? filtro = null)
    {
        var lista = new List<Socio>();
        await using var c = new SqlConnection(_cs);
        var query = "SELECT SocioId, DNI, Nombre, Email, Activo FROM Socios WHERE Activo = 1";
        
        if (!string.IsNullOrWhiteSpace(filtro))
            query += " AND (Nombre LIKE @filtro OR DNI LIKE @filtro)";

        await using var cmd = new SqlCommand(query, c);
        if (!string.IsNullOrWhiteSpace(filtro))
            cmd.Parameters.AddWithValue("@filtro", $"%{filtro.Trim()}%");

        await c.OpenAsync();
        await using var r = await cmd.ExecuteReaderAsync();
        while (await r.ReadAsync())
        {
            lista.Add(new Socio
            {
                SocioId = r.GetInt32(r.GetOrdinal("SocioId")),
                DNI = r.GetString(r.GetOrdinal("DNI")),
                Nombre = r.GetString(r.GetOrdinal("Nombre")),
                Email = r.GetString(r.GetOrdinal("Email")),
                Activo = r.GetBoolean(r.GetOrdinal("Activo"))
            });
        }
        return lista;
    }

    public async Task<Socio?> ObtenerPorIdAsync(int id)
    {
        await using var c = new SqlConnection(_cs);
        await using var cmd = new SqlCommand("SELECT SocioId, DNI, Nombre, Email, Activo FROM Socios WHERE SocioId = @id", c);
        cmd.Parameters.AddWithValue("@id", id);
        await c.OpenAsync();
        await using var r = await cmd.ExecuteReaderAsync();
        if (!await r.ReadAsync()) return null;

        return new Socio
        {
            SocioId = r.GetInt32(r.GetOrdinal("SocioId")),
            DNI = r.GetString(r.GetOrdinal("DNI")),
            Nombre = r.GetString(r.GetOrdinal("Nombre")),
            Email = r.GetString(r.GetOrdinal("Email")),
            Activo = r.GetBoolean(r.GetOrdinal("Activo"))
        };
    }

    public async Task<int> CrearAsync(Socio socio)
    {
        await using var c = new SqlConnection(_cs);
        await using var cmd = new SqlCommand(@"
            INSERT INTO Socios (DNI, Nombre, Email) 
            OUTPUT INSERTED.SocioId
            VALUES (@DNI, @Nombre, @Email)", c);
        cmd.Parameters.AddWithValue("@DNI", socio.DNI.Trim());
        cmd.Parameters.AddWithValue("@Nombre", socio.Nombre.Trim());
        cmd.Parameters.AddWithValue("@Email", socio.Email.Trim());
        await c.OpenAsync();
        return Convert.ToInt32(await cmd.ExecuteScalarAsync());
    }

    public async Task ActualizarAsync(Socio socio)
    {
        await using var c = new SqlConnection(_cs);
        await using var cmd = new SqlCommand(@"
            UPDATE Socios SET DNI = @DNI, Nombre = @Nombre, Email = @Email
            WHERE SocioId = @SocioId", c);
        cmd.Parameters.AddWithValue("@DNI", socio.DNI.Trim());
        cmd.Parameters.AddWithValue("@Nombre", socio.Nombre.Trim());
        cmd.Parameters.AddWithValue("@Email", socio.Email.Trim());
        cmd.Parameters.AddWithValue("@SocioId", socio.SocioId);
        await c.OpenAsync();
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task EliminarAsync(int id)
    {
        await using var c = new SqlConnection(_cs);
        await using var cmd = new SqlCommand("UPDATE Socios SET Activo = 0 WHERE SocioId = @id", c);
        cmd.Parameters.AddWithValue("@id", id);
        await c.OpenAsync();
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<bool> ExisteDNIAsync(string dni, int? excludeSocioId = null)
    {
        await using var c = new SqlConnection(_cs);
        var query = "SELECT COUNT(1) FROM Socios WHERE DNI = @dni AND Activo = 1";
        if (excludeSocioId.HasValue) query += " AND SocioId != @id";
        
        await using var cmd = new SqlCommand(query, c);
        cmd.Parameters.AddWithValue("@dni", dni.Trim());
        if (excludeSocioId.HasValue) cmd.Parameters.AddWithValue("@id", excludeSocioId.Value);
        
        await c.OpenAsync();
        return Convert.ToInt32(await cmd.ExecuteScalarAsync()) > 0;
    }
}
