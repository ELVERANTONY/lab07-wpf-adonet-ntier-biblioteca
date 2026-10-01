using System.Data;
using Microsoft.Data.SqlClient;
using Biblioteca.Entidades.Modelos;
using Biblioteca.Datos.Interfaces;
using static Biblioteca.Datos.Repositorios.DbHelper;

namespace Biblioteca.Datos.Repositorios;

public class PrestamoRepositorio : IPrestamoRepositorio
{
    private readonly string _cs;
    public PrestamoRepositorio(string cs) => _cs = cs;

    public async Task<int> ObtenerCantidadLibrosPendientesAsync(int socioId)
    {
        await using var c = new SqlConnection(_cs);
        // Cuenta cuantos libros no devueltos tiene este socio
        var query = @"
            SELECT COUNT(1) 
            FROM DetallePrestamo DP
            INNER JOIN Prestamos P ON DP.PrestamoId = P.PrestamoId
            WHERE P.SocioId = @SocioId AND DP.FechaDevolucion IS NULL";
        
        await using var cmd = new SqlCommand(query, c);
        cmd.Parameters.AddWithValue("@SocioId", socioId);
        await c.OpenAsync();
        return (int)await cmd.ExecuteScalarAsync();
    }

    public async Task<bool> SocioTienePrestamosPendientesAsync(int socioId)
    {
        return await ObtenerCantidadLibrosPendientesAsync(socioId) > 0;
    }

    public async Task<bool> LibroEstaEnPrestamoPendienteAsync(int libroId)
    {
        await using var c = new SqlConnection(_cs);
        var query = "SELECT COUNT(1) FROM DetallePrestamo WHERE LibroId = @LibroId AND FechaDevolucion IS NULL";
        await using var cmd = new SqlCommand(query, c);
        cmd.Parameters.AddWithValue("@LibroId", libroId);
        await c.OpenAsync();
        return (int)await cmd.ExecuteScalarAsync() > 0;
    }

    public async Task RegistrarPrestamoAsync(Prestamo prestamo)
    {
        await using var c = new SqlConnection(_cs);
        await c.OpenAsync();
        
        // REGLA: Uso estricto de Transacción para garantizar consistencia
        await using var t = (SqlTransaction)await c.BeginTransactionAsync();
        
        try
        {
            // 1. Insertar Cabecera (Prestamos)
            var qCabecera = @"
                INSERT INTO Prestamos (SocioId, FechaPrestamo, FechaLimite, Estado) 
                OUTPUT INSERTED.PrestamoId
                VALUES (@SocioId, @FechaPrestamo, @FechaLimite, 'Pendiente')";
            await using var cmdCab = new SqlCommand(qCabecera, c, t);
            cmdCab.Parameters.AddWithValue("@SocioId", prestamo.SocioId);
            cmdCab.Parameters.AddWithValue("@FechaPrestamo", prestamo.FechaPrestamo);
            cmdCab.Parameters.AddWithValue("@FechaLimite", prestamo.FechaLimite);
            
            int prestamoId = (int)await cmdCab.ExecuteScalarAsync();

            // 2. Insertar Detalles y descontar stock
            foreach (var det in prestamo.Detalles)
            {
                var qDetalle = "INSERT INTO DetallePrestamo (PrestamoId, LibroId) VALUES (@PId, @LId)";
                await using var cmdDet = new SqlCommand(qDetalle, c, t);
                cmdDet.Parameters.AddWithValue("@PId", prestamoId);
                cmdDet.Parameters.AddWithValue("@LId", det.LibroId);
                await cmdDet.ExecuteNonQueryAsync();

                var qStock = "UPDATE Libros SET Ejemplares = Ejemplares - 1 WHERE LibroId = @LId";
                await using var cmdStock = new SqlCommand(qStock, c, t);
                cmdStock.Parameters.AddWithValue("@LId", det.LibroId);
                await cmdStock.ExecuteNonQueryAsync();
            }

            // Confirmar transacción si todo salio bien
            await t.CommitAsync();
        }
        catch
        {
            // Revertir si hubo cualquier error
            await t.RollbackAsync();
            throw;
        }
    }

    public async Task RegistrarDevolucionAsync(int prestamoId, int libroId, DateTime fechaDevolucion)
    {
        await using var c = new SqlConnection(_cs);
        await c.OpenAsync();
        await using var t = (SqlTransaction)await c.BeginTransactionAsync();

        try
        {
            // 1. Actualizar Fecha de Devolución
            var qDev = "UPDATE DetallePrestamo SET FechaDevolucion = @Fecha WHERE PrestamoId = @PId AND LibroId = @LId";
            await using var cmdDev = new SqlCommand(qDev, c, t);
            cmdDev.Parameters.AddWithValue("@Fecha", fechaDevolucion);
            cmdDev.Parameters.AddWithValue("@PId", prestamoId);
            cmdDev.Parameters.AddWithValue("@LId", libroId);
            await cmdDev.ExecuteNonQueryAsync();

            // 2. Devolver stock al libro
            var qStock = "UPDATE Libros SET Ejemplares = Ejemplares + 1 WHERE LibroId = @LId";
            await using var cmdStock = new SqlCommand(qStock, c, t);
            cmdStock.Parameters.AddWithValue("@LId", libroId);
            await cmdStock.ExecuteNonQueryAsync();

            // 3. Revisar si ya no quedan libros pendientes en este préstamo para cambiarlo a 'Devuelto'
            var qPend = "SELECT COUNT(1) FROM DetallePrestamo WHERE PrestamoId = @PId AND FechaDevolucion IS NULL";
            await using var cmdPend = new SqlCommand(qPend, c, t);
            cmdPend.Parameters.AddWithValue("@PId", prestamoId);
            int pendientes = (int)await cmdPend.ExecuteScalarAsync();

            if (pendientes == 0)
            {
                var qEstado = "UPDATE Prestamos SET Estado = 'Devuelto' WHERE PrestamoId = @PId";
                await using var cmdEstado = new SqlCommand(qEstado, c, t);
                cmdEstado.Parameters.AddWithValue("@PId", prestamoId);
                await cmdEstado.ExecuteNonQueryAsync();
            }

            await t.CommitAsync();
        }
        catch
        {
            await t.RollbackAsync();
            throw;
        }
    }

    public async Task<List<Prestamo>> ListarPorFechasAsync(DateTime inicio, DateTime fin)
    {
        var lista = new List<Prestamo>();
        await using var c = new SqlConnection(_cs);
        
        // Reporte con INNER JOIN como pide la rúbrica
        var query = @"
            SELECT P.PrestamoId, P.FechaPrestamo, P.FechaLimite, P.Estado, 
                   S.Nombre AS NombreSocio, S.DNI AS DniSocio, 
                   DP.LibroId, L.Titulo AS NombreLibro, DP.FechaDevolucion
            FROM Prestamos P
            INNER JOIN Socios S ON P.SocioId = S.SocioId
            INNER JOIN DetallePrestamo DP ON P.PrestamoId = DP.PrestamoId
            INNER JOIN Libros L ON DP.LibroId = L.LibroId
            WHERE P.FechaPrestamo >= @Inicio AND P.FechaPrestamo <= @Fin
            ORDER BY P.FechaPrestamo DESC";

        await using var cmd = new SqlCommand(query, c);
        // Asegurar que abarque todo el día
        cmd.Parameters.AddWithValue("@Inicio", inicio.Date);
        cmd.Parameters.AddWithValue("@Fin", fin.Date.AddDays(1).AddSeconds(-1));

        await c.OpenAsync();
        await using var r = await cmd.ExecuteReaderAsync();
        
        // Agrupar por PrestamoId
        while (await r.ReadAsync())
        {
            int pId = r.GetInt32(r.GetOrdinal("PrestamoId"));
            var prestamo = lista.FirstOrDefault(x => x.PrestamoId == pId);
            if (prestamo == null)
            {
                prestamo = new Prestamo
                {
                    PrestamoId = pId,
                    SocioId = 0, // No lo necesitamos para el reporte
                    NombreSocio = r.GetString(r.GetOrdinal("NombreSocio")),
                    DniSocio = r.GetString(r.GetOrdinal("DniSocio")),
                    FechaPrestamo = r.GetDateTime(r.GetOrdinal("FechaPrestamo")),
                    FechaLimite = r.GetDateTime(r.GetOrdinal("FechaLimite")),
                    Estado = r.GetString(r.GetOrdinal("Estado")),
                    Detalles = new List<DetallePrestamo>()
                };
                lista.Add(prestamo);
            }

            prestamo.Detalles.Add(new DetallePrestamo
            {
                PrestamoId = pId,
                LibroId = r.GetInt32(r.GetOrdinal("LibroId")),
                NombreLibro = r.GetString(r.GetOrdinal("NombreLibro")),
                FechaDevolucion = r.GetDateOrNull("FechaDevolucion")
            });
        }
        return lista;
    }

    public async Task<List<DetallePrestamo>> ListarDetallesPendientesAsync(int socioId)
    {
        var lista = new List<DetallePrestamo>();
        await using var c = new SqlConnection(_cs);
        
        var query = @"
            SELECT DP.PrestamoId, DP.LibroId, L.Titulo AS NombreLibro
            FROM DetallePrestamo DP
            INNER JOIN Prestamos P ON DP.PrestamoId = P.PrestamoId
            INNER JOIN Libros L ON DP.LibroId = L.LibroId
            WHERE P.SocioId = @SocioId AND DP.FechaDevolucion IS NULL";

        await using var cmd = new SqlCommand(query, c);
        cmd.Parameters.AddWithValue("@SocioId", socioId);
        
        await c.OpenAsync();
        await using var r = await cmd.ExecuteReaderAsync();
        while (await r.ReadAsync())
        {
            lista.Add(new DetallePrestamo
            {
                PrestamoId = r.GetInt32(r.GetOrdinal("PrestamoId")),
                LibroId = r.GetInt32(r.GetOrdinal("LibroId")),
                NombreLibro = r.GetString(r.GetOrdinal("NombreLibro")),
                FechaDevolucion = null
            });
        }
        return lista;
    }
}
