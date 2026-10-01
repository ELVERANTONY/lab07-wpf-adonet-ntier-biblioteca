using System.Configuration;
using System.Windows;
using System.Windows.Controls;
using Biblioteca.Entidades.Modelos;
using Biblioteca.Negocio.Reglas;

namespace Biblioteca.WPF;

public partial class MainWindow : Window
{
    private readonly LibroNegocio _libroNegocio;
    private readonly SocioNegocio _socioNegocio;
    private readonly PrestamoNegocio _prestamoNegocio;
    private readonly List<Libro> _librosSeleccionados = new();
    private Libro? _libroEnEdicion;
    private Socio? _socioEnEdicion;

    public MainWindow()
    {
        InitializeComponent();
        var cadena = ConfigurationManager.ConnectionStrings["BibliotecaDB"]?.ConnectionString;
        if (string.IsNullOrWhiteSpace(cadena))
            throw new ConfigurationErrorsException("No se encontró la cadena de conexión 'BibliotecaDB' en App.config.");

        _libroNegocio = new LibroNegocio(cadena);
        _socioNegocio = new SocioNegocio(cadena);
        _prestamoNegocio = new PrestamoNegocio(cadena);
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        FechaLimitePrestamo.SelectedDate = DateTime.Today.AddDays(7);
        FechaReporteInicio.SelectedDate = DateTime.Today.AddMonths(-1);
        FechaReporteFin.SelectedDate = DateTime.Today;
        await CargarInicialAsync();
    }

    private async Task CargarInicialAsync()
    {
        try
        {
            await CargarAutoresAsync();
            await CargarLibrosAsync();
            await CargarSociosAsync();
            await CargarLibrosDisponiblesAsync();
        }
        catch (Exception ex)
        {
            MostrarError(ex);
        }
    }

    private async Task CargarAutoresAsync()
    {
        CboAutor.ItemsSource = await _libroNegocio.ListarAutoresAsync();
    }

    private async Task CargarLibrosAsync(string? filtro = null)
    {
        GridLibros.ItemsSource = await _libroNegocio.ListarAsync(filtro);
    }

    private async Task CargarLibrosDisponiblesAsync()
    {
        GridLibrosDisponibles.ItemsSource = await _libroNegocio.ListarDisponiblesAsync();
    }

    private async Task CargarSociosAsync(string? filtro = null)
    {
        var socios = await _socioNegocio.ListarAsync(filtro);
        GridSocios.ItemsSource = socios;
        CboSocioPrestamo.ItemsSource = socios;
        CboSocioDevolucion.ItemsSource = socios;
    }

    private async void BuscarLibros_Click(object sender, RoutedEventArgs e)
    {
        try { await CargarLibrosAsync(TxtBuscarLibro.Text); }
        catch (Exception ex) { MostrarError(ex); }
    }

    private async void RefrescarLibros_Click(object sender, RoutedEventArgs e)
    {
        TxtBuscarLibro.Clear();
        try { await CargarLibrosAsync(); }
        catch (Exception ex) { MostrarError(ex); }
    }

    private void GridLibros_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (GridLibros.SelectedItem is not Libro libro) return;
        _libroEnEdicion = libro;
        TxtLibroTitulo.Text = libro.Titulo;
        TxtLibroIsbn.Text = libro.ISBN;
        TxtLibroEjemplares.Text = libro.Ejemplares.ToString();
        CboAutor.SelectedValue = libro.AutorId;
    }

    private void NuevoLibro_Click(object sender, RoutedEventArgs e)
    {
        _libroEnEdicion = null;
        GridLibros.SelectedItem = null;
        TxtLibroTitulo.Clear();
        TxtLibroIsbn.Clear();
        TxtLibroEjemplares.Text = "0";
        CboAutor.SelectedIndex = -1;
    }

    private async void GuardarLibro_Click(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(TxtLibroEjemplares.Text, out var ejemplares))
        {
            MessageBox.Show("Ingrese un número válido de ejemplares.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (CboAutor.SelectedValue is not int autorId)
        {
            MessageBox.Show("Seleccione un autor.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var libro = new Libro
        {
            LibroId = _libroEnEdicion?.LibroId ?? 0,
            Titulo = TxtLibroTitulo.Text,
            ISBN = TxtLibroIsbn.Text,
            AutorId = autorId,
            Ejemplares = ejemplares
        };

        try
        {
            if (_libroEnEdicion is null) await _libroNegocio.RegistrarAsync(libro);
            else await _libroNegocio.ActualizarAsync(libro);
            MessageBox.Show("Libro guardado correctamente.", "Biblioteca", MessageBoxButton.OK, MessageBoxImage.Information);
            NuevoLibro_Click(sender, e);
            await CargarLibrosAsync();
            await CargarLibrosDisponiblesAsync();
        }
        catch (Exception ex) { MostrarError(ex); }
    }

    private async void EliminarLibro_Click(object sender, RoutedEventArgs e)
    {
        if (_libroEnEdicion is null)
        {
            MessageBox.Show("Seleccione un libro de la lista.", "Biblioteca", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (MessageBox.Show($"¿Dar de baja lógicamente a '{_libroEnEdicion.Titulo}'?", "Confirmar baja", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;

        try
        {
            await _libroNegocio.EliminarAsync(_libroEnEdicion.LibroId);
            NuevoLibro_Click(sender, e);
            await CargarLibrosAsync();
            await CargarLibrosDisponiblesAsync();
        }
        catch (Exception ex) { MostrarError(ex); }
    }

    private async void BuscarSocios_Click(object sender, RoutedEventArgs e)
    {
        try { await CargarSociosAsync(TxtBuscarSocio.Text); }
        catch (Exception ex) { MostrarError(ex); }
    }

    private async void RefrescarSocios_Click(object sender, RoutedEventArgs e)
    {
        TxtBuscarSocio.Clear();
        try { await CargarSociosAsync(); }
        catch (Exception ex) { MostrarError(ex); }
    }

    private void GridSocios_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (GridSocios.SelectedItem is not Socio socio) return;
        _socioEnEdicion = socio;
        TxtSocioDni.Text = socio.DNI;
        TxtSocioNombre.Text = socio.Nombre;
        TxtSocioEmail.Text = socio.Email;
    }

    private void NuevoSocio_Click(object sender, RoutedEventArgs e)
    {
        _socioEnEdicion = null;
        GridSocios.SelectedItem = null;
        TxtSocioDni.Clear();
        TxtSocioNombre.Clear();
        TxtSocioEmail.Clear();
    }

    private async void GuardarSocio_Click(object sender, RoutedEventArgs e)
    {
        var socio = new Socio
        {
            SocioId = _socioEnEdicion?.SocioId ?? 0,
            DNI = TxtSocioDni.Text,
            Nombre = TxtSocioNombre.Text,
            Email = TxtSocioEmail.Text
        };
        try
        {
            if (_socioEnEdicion is null) await _socioNegocio.RegistrarAsync(socio);
            else await _socioNegocio.ActualizarAsync(socio);
            MessageBox.Show("Socio guardado correctamente.", "Biblioteca", MessageBoxButton.OK, MessageBoxImage.Information);
            NuevoSocio_Click(sender, e);
            await CargarSociosAsync();
        }
        catch (Exception ex) { MostrarError(ex); }
    }

    private async void EliminarSocio_Click(object sender, RoutedEventArgs e)
    {
        if (_socioEnEdicion is null)
        {
            MessageBox.Show("Seleccione un socio de la lista.", "Biblioteca", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (MessageBox.Show($"¿Dar de baja lógicamente a '{_socioEnEdicion.Nombre}'?", "Confirmar baja", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
        try
        {
            await _socioNegocio.EliminarAsync(_socioEnEdicion.SocioId);
            NuevoSocio_Click(sender, e);
            await CargarSociosAsync();
        }
        catch (Exception ex) { MostrarError(ex); }
    }

    private void AgregarLibroPrestamo_Click(object sender, RoutedEventArgs e)
    {
        foreach (var libro in GridLibrosDisponibles.SelectedItems.Cast<Libro>().Where(l => _librosSeleccionados.All(s => s.LibroId != l.LibroId)))
            _librosSeleccionados.Add(libro);
        ActualizarDetallePrestamo();
    }

    private void QuitarLibroPrestamo_Click(object sender, RoutedEventArgs e)
    {
        foreach (var libro in GridDetallePrestamo.SelectedItems.Cast<Libro>().ToList())
            _librosSeleccionados.RemoveAll(l => l.LibroId == libro.LibroId);
        ActualizarDetallePrestamo();
    }

    private void ActualizarDetallePrestamo()
    {
        GridDetallePrestamo.ItemsSource = null;
        GridDetallePrestamo.ItemsSource = _librosSeleccionados.ToList();
        TxtTotalPrestamo.Text = $"{_librosSeleccionados.Count} libro(s) seleccionado(s)";
    }

    private async void RegistrarPrestamo_Click(object sender, RoutedEventArgs e)
    {
        if (CboSocioPrestamo.SelectedValue is not int socioId || FechaLimitePrestamo.SelectedDate is not DateTime fechaLimite)
        {
            MessageBox.Show("Seleccione el socio y la fecha límite.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        var prestamo = new Prestamo
        {
            SocioId = socioId,
            FechaPrestamo = DateTime.Today,
            FechaLimite = fechaLimite,
            Detalles = _librosSeleccionados.Select(l => new DetallePrestamo { LibroId = l.LibroId }).ToList()
        };
        try
        {
            await _prestamoNegocio.RegistrarPrestamoAsync(prestamo);
            MessageBox.Show("Préstamo registrado correctamente.", "Biblioteca", MessageBoxButton.OK, MessageBoxImage.Information);
            _librosSeleccionados.Clear();
            ActualizarDetallePrestamo();
            await CargarLibrosDisponiblesAsync();
        }
        catch (Exception ex) { MostrarError(ex); }
    }

    private async void CboSocioDevolucion_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        await CargarDevolucionesAsync();
    }

    private async void RefrescarDevoluciones_Click(object sender, RoutedEventArgs e)
    {
        await CargarDevolucionesAsync();
    }

    private async Task CargarDevolucionesAsync()
    {
        if (CboSocioDevolucion.SelectedValue is not int socioId)
        {
            GridDevoluciones.ItemsSource = null;
            return;
        }
        try { GridDevoluciones.ItemsSource = await _prestamoNegocio.ListarDetallesPendientesAsync(socioId); }
        catch (Exception ex) { MostrarError(ex); }
    }

    private async void RegistrarDevolucion_Click(object sender, RoutedEventArgs e)
    {
        if (GridDevoluciones.SelectedItem is not DetallePrestamo detalle)
        {
            MessageBox.Show("Seleccione el libro que se va a devolver.", "Biblioteca", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        try
        {
            var resultado = await _prestamoNegocio.RegistrarDevolucionAsync(detalle.PrestamoId, detalle.LibroId, detalle.FechaLimite);
            MessageBox.Show($"Devolución registrada. Multa: S/ {resultado.Multa:N2}", "Biblioteca", MessageBoxButton.OK, MessageBoxImage.Information);
            await CargarDevolucionesAsync();
            await CargarLibrosDisponiblesAsync();
        }
        catch (Exception ex) { MostrarError(ex); }
    }

    private async void GenerarReporte_Click(object sender, RoutedEventArgs e)
    {
        if (FechaReporteInicio.SelectedDate is not DateTime inicio || FechaReporteFin.SelectedDate is not DateTime fin)
        {
            MessageBox.Show("Seleccione ambas fechas.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        try { GridReporte.ItemsSource = await _prestamoNegocio.GenerarReporteFechasAsync(inicio, fin); }
        catch (Exception ex) { MostrarError(ex); }
    }

    private async void MainTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.Source != MainTabs) return;
        try
        {
            if (MainTabs.SelectedIndex == 2) await CargarLibrosDisponiblesAsync();
            if (MainTabs.SelectedIndex == 3) await CargarSociosAsync();
        }
        catch (Exception ex) { MostrarError(ex); }
    }

    private static void MostrarError(Exception ex)
    {
        var mensaje = ex is ReglaNegocioException ? ex.Message : "No fue posible completar la operación. Verifique la conexión y los datos ingresados.";
        MessageBox.Show(mensaje, "Biblioteca", MessageBoxButton.OK, MessageBoxImage.Warning);
    }
}
