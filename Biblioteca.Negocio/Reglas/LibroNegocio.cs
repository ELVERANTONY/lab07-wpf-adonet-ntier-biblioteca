using Biblioteca.Entidades.Modelos;
using Biblioteca.Datos.Interfaces;
using Biblioteca.Datos.Repositorios;

namespace Biblioteca.Negocio.Reglas;

public class LibroNegocio
{
    private readonly ILibroRepositorio _repositorio;
    private readonly IPrestamoRepositorio _prestamoRepo;

    // Se inyecta la dependencia instanciando la capa de datos (o se puede recibir por inyección directa)
    public LibroNegocio(string cs)
    {
        _repositorio = new LibroRepositorio(cs);
        _prestamoRepo = new PrestamoRepositorio(cs);
    }

    public async Task<List<Libro>> ListarAsync(string? filtro = null)
    {
        return await _repositorio.ListarAsync(filtro);
    }

    public async Task<List<Autor>> ListarAutoresAsync()
    {
        return await _repositorio.ListarAutoresAsync();
    }

    public async Task RegistrarAsync(Libro libro)
    {
        if (string.IsNullOrWhiteSpace(libro.Titulo))
            throw new ReglaNegocioException("El título del libro no puede estar vacío.");
        if (string.IsNullOrWhiteSpace(libro.ISBN))
            throw new ReglaNegocioException("El ISBN no puede estar vacío.");
        if (libro.AutorId <= 0)
            throw new ReglaNegocioException("Debe seleccionar un autor válido.");
        if (libro.Ejemplares < 0)
            throw new ReglaNegocioException("La cantidad de ejemplares no puede ser negativa.");

        if (await _repositorio.ExisteISBNAsync(libro.ISBN))
            throw new ReglaNegocioException($"El ISBN {libro.ISBN} ya se encuentra registrado en otro libro activo.");

        await _repositorio.CrearAsync(libro);
    }

    public async Task ActualizarAsync(Libro libro)
    {
        if (libro.LibroId <= 0)
            throw new ReglaNegocioException("ID de libro inválido.");
        if (string.IsNullOrWhiteSpace(libro.Titulo))
            throw new ReglaNegocioException("El título del libro no puede estar vacío.");
        
        if (await _repositorio.ExisteISBNAsync(libro.ISBN, libro.LibroId))
            throw new ReglaNegocioException($"El ISBN {libro.ISBN} ya pertenece a otro libro activo.");

        await _repositorio.ActualizarAsync(libro);
    }

    public async Task EliminarAsync(int id)
    {
        if (await _prestamoRepo.LibroEstaEnPrestamoPendienteAsync(id))
            throw new ReglaNegocioException("No se puede dar de baja este libro porque tiene ejemplares prestados sin devolver.");

        await _repositorio.EliminarAsync(id);
    }
}
