namespace Biblioteca.Entidades.Modelos;

public class DetallePrestamo
{
    public int PrestamoId { get; set; }
    public int LibroId { get; set; }
    public DateTime? FechaDevolucion { get; set; }

    // Propiedades extendidas
    public string NombreLibro { get; set; } = string.Empty;
}
