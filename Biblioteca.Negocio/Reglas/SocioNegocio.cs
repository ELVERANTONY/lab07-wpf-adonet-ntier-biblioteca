using Biblioteca.Entidades.Modelos;
using Biblioteca.Datos.Interfaces;
using Biblioteca.Datos.Repositorios;

namespace Biblioteca.Negocio.Reglas;

public class SocioNegocio
{
    private readonly ISocioRepositorio _repositorio;
    private readonly IPrestamoRepositorio _prestamoRepo;

    public SocioNegocio(string cs)
    {
        _repositorio = new SocioRepositorio(cs);
        _prestamoRepo = new PrestamoRepositorio(cs);
    }

    public async Task<List<Socio>> ListarAsync(string? filtro = null)
    {
        return await _repositorio.ListarAsync(filtro);
    }

    public async Task RegistrarAsync(Socio socio)
    {
        if (string.IsNullOrWhiteSpace(socio.DNI) || socio.DNI.Length != 8)
            throw new ReglaNegocioException("El DNI debe tener exactamente 8 caracteres.");
        if (string.IsNullOrWhiteSpace(socio.Nombre))
            throw new ReglaNegocioException("El nombre del socio es obligatorio.");

        if (await _repositorio.ExisteDNIAsync(socio.DNI))
            throw new ReglaNegocioException($"El DNI {socio.DNI} ya está registrado.");

        await _repositorio.CrearAsync(socio);
    }

    public async Task ActualizarAsync(Socio socio)
    {
        if (string.IsNullOrWhiteSpace(socio.DNI) || socio.DNI.Length != 8)
            throw new ReglaNegocioException("El DNI debe tener exactamente 8 caracteres.");
        if (string.IsNullOrWhiteSpace(socio.Nombre))
            throw new ReglaNegocioException("El nombre del socio es obligatorio.");

        if (await _repositorio.ExisteDNIAsync(socio.DNI, socio.SocioId))
            throw new ReglaNegocioException($"El DNI {socio.DNI} ya pertenece a otro socio.");

        await _repositorio.ActualizarAsync(socio);
    }

    public async Task EliminarAsync(int id)
    {
        if (await _prestamoRepo.SocioTienePrestamosPendientesAsync(id))
            throw new ReglaNegocioException("No se puede dar de baja al socio porque tiene libros pendientes de devolución.");

        await _repositorio.EliminarAsync(id);
    }
}
