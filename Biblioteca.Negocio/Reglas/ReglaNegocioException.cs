namespace Biblioteca.Negocio.Reglas;

public class ReglaNegocioException : Exception
{
    public ReglaNegocioException(string message) : base(message)
    {
    }
}
