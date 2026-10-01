namespace Biblioteca.Entidades.Modelos;

public class Prestamo
{
    public int PrestamoId { get; set; }
    public int SocioId { get; set; }
    public DateTime FechaPrestamo { get; set; }
    public DateTime FechaLimite { get; set; }
    public string Estado { get; set; } = string.Empty;

    // Propiedades extendidas para navegación
    public List<DetallePrestamo> Detalles { get; set; } = new();
    public string NombreSocio { get; set; } = string.Empty;
    public string DniSocio { get; set; } = string.Empty;
}
