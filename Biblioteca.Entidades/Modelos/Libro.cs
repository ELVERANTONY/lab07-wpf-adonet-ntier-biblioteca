namespace Biblioteca.Entidades.Modelos;

public class Libro
{
    public int LibroId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public int AutorId { get; set; }
    public int Ejemplares { get; set; }
    public bool Activo { get; set; } = true;

    // Campos extendidos para la capa visual / reportes
    public string NombreAutor { get; set; } = string.Empty;
}
