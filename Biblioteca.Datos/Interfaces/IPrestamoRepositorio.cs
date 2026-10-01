using Biblioteca.Entidades.Modelos;

namespace Biblioteca.Datos.Interfaces;

public interface IPrestamoRepositorio
{
    Task<int> ObtenerCantidadLibrosPendientesAsync(int socioId);
    Task<bool> SocioTienePrestamosPendientesAsync(int socioId);
    Task<bool> LibroEstaEnPrestamoPendienteAsync(int libroId);
    Task RegistrarPrestamoAsync(Prestamo prestamo);
    Task RegistrarDevolucionAsync(int prestamoId, int libroId, DateTime fechaDevolucion);
    
    // Para reportes
    Task<List<Prestamo>> ListarPorFechasAsync(DateTime inicio, DateTime fin);
    Task<List<DetallePrestamo>> ListarDetallesPendientesAsync(int socioId);
}
