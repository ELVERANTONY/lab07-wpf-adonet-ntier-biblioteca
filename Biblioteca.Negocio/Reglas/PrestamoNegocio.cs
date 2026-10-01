using Biblioteca.Entidades.Modelos;
using Biblioteca.Datos.Interfaces;
using Biblioteca.Datos.Repositorios;

namespace Biblioteca.Negocio.Reglas;

public class PrestamoNegocio
{
    private readonly IPrestamoRepositorio _prestamoRepo;
    private readonly ILibroRepositorio _libroRepo;
    private readonly ISocioRepositorio _socioRepo;

    public PrestamoNegocio(string cs)
    {
        _prestamoRepo = new PrestamoRepositorio(cs);
        _libroRepo = new LibroRepositorio(cs);
        _socioRepo = new SocioRepositorio(cs);
    }

    public async Task RegistrarPrestamoAsync(Prestamo prestamo)
    {
        if (prestamo.SocioId <= 0)
            throw new ReglaNegocioException("Debe seleccionar un socio válido.");

        var socio = await _socioRepo.ObtenerPorIdAsync(prestamo.SocioId);
        if (socio == null || !socio.Activo)
            throw new ReglaNegocioException("El socio seleccionado no existe o está dado de baja.");
        
        if (prestamo.Detalles == null || !prestamo.Detalles.Any())
            throw new ReglaNegocioException("Debe agregar al menos un libro al préstamo.");

        if (prestamo.Detalles.GroupBy(d => d.LibroId).Any(g => g.Count() > 1))
            throw new ReglaNegocioException("No se puede registrar el mismo libro más de una vez en un préstamo.");

        if (prestamo.FechaLimite <= prestamo.FechaPrestamo)
            throw new ReglaNegocioException("La fecha límite debe ser mayor a la fecha de préstamo.");

        // Regla: No más de 3 libros pendientes por socio
        int pendientes = await _prestamoRepo.ObtenerCantidadLibrosPendientesAsync(prestamo.SocioId);
        if (pendientes + prestamo.Detalles.Count > 3)
            throw new ReglaNegocioException($"El socio ya tiene {pendientes} libros pendientes. El máximo permitido es 3. No puede llevar {prestamo.Detalles.Count} libros más.");

        // Reglas de Libros: Stock mayor a 0 y Activo
        foreach (var det in prestamo.Detalles)
        {
            var libro = await _libroRepo.ObtenerPorIdAsync(det.LibroId);
            if (libro == null || !libro.Activo)
                throw new ReglaNegocioException($"El libro con ID {det.LibroId} no existe o está dado de baja.");
            
            if (libro.Ejemplares <= 0)
                throw new ReglaNegocioException($"No hay stock disponible para el libro '{libro.Titulo}'.");
        }

        // Delegar a capa de datos que manejará la transacción
        await _prestamoRepo.RegistrarPrestamoAsync(prestamo);
    }

    public async Task<(bool Exito, decimal Multa)> RegistrarDevolucionAsync(int prestamoId, int libroId, DateTime fechaLimite)
    {
        DateTime fechaDevolucion = DateTime.Now;
        
        // Regla: Multa de S/ 1.50 por cada día de retraso
        decimal multa = 0;
        if (fechaDevolucion.Date > fechaLimite.Date)
        {
            int diasRetraso = (fechaDevolucion.Date - fechaLimite.Date).Days;
            multa = diasRetraso * 1.50m;
        }

        await _prestamoRepo.RegistrarDevolucionAsync(prestamoId, libroId, fechaDevolucion);

        return (true, multa);
    }

    public async Task<List<DetallePrestamo>> ListarDetallesPendientesAsync(int socioId)
    {
        return await _prestamoRepo.ListarDetallesPendientesAsync(socioId);
    }

    public async Task<List<Prestamo>> GenerarReporteFechasAsync(DateTime inicio, DateTime fin)
    {
        if (inicio > fin)
            throw new ReglaNegocioException("La fecha de inicio no puede ser mayor a la fecha de fin.");
            
        return await _prestamoRepo.ListarPorFechasAsync(inicio, fin);
    }
}
