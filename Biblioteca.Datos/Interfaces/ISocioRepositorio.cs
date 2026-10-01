using Biblioteca.Entidades.Modelos;

namespace Biblioteca.Datos.Interfaces;

public interface ISocioRepositorio
{
    Task<List<Socio>> ListarAsync(string? filtro = null);
    Task<int> CrearAsync(Socio socio);
    Task ActualizarAsync(Socio socio);
    Task EliminarAsync(int id);
    Task<bool> ExisteDNIAsync(string dni, int? excludeSocioId = null);
}
