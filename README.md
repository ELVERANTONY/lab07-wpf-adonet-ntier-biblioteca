# BibliotecaDB — Semana 07

Aplicación de escritorio WPF para administrar una biblioteca con arquitectura en capas.

## Puesta en marcha

1. Ejecute `Database/01_BibliotecaDB.sql` en SQL Server Management Studio. El script crea `BibliotecaDB`, las cinco tablas y datos de prueba.
2. En `Biblioteca.WPF/App.config`, ajuste `Server=localhost` si su instancia de SQL Server usa otro nombre, por ejemplo `Server=.\\SQLEXPRESS`.
3. Abra `Biblioteca.slnx` con Visual Studio 2022, establezca **Biblioteca.WPF** como proyecto de inicio y presione `F5`.

## Arquitectura

| Proyecto | Responsabilidad | Referencias |
|---|---|---|
| `Biblioteca.Entidades` | Clases `Autor`, `Libro`, `Socio`, `Prestamo` y `DetallePrestamo`; solo propiedades. | Ninguna |
| `Biblioteca.Datos` | Repositorios ADO.NET parametrizados. Devuelven entidades y `List<T>`. | Entidades |
| `Biblioteca.Negocio` | Validaciones y excepciones de reglas de negocio. | Datos, Entidades |
| `Biblioteca.WPF` | Pantallas y eventos `async/await`; no usa `SqlClient` ni clases de Datos. | Negocio, Entidades |

## Funcionalidades WPF

- **Libros:** búsqueda por título/autor, alta, edición, baja lógica y autor visible.
- **Socios:** búsqueda por nombre/DNI, alta, edición y baja lógica.
- **Préstamos:** selección de socio, múltiples libros con stock y registro transaccional.
- **Devoluciones:** selección de préstamo pendiente y visualización de multa en soles.
- **Reporte por fechas:** socio, libros, fechas y estado obtenidos mediante `INNER JOIN`.

## Reglas implementadas en Negocio

`LibroNegocio` valida título, ISBN, autor y ejemplares antes de crear o actualizar. También evita ISBN repetido y bloquea la baja lógica cuando el libro tiene préstamos pendientes.

`SocioNegocio` valida DNI de ocho caracteres y nombre obligatorio, evita DNI repetido y bloquea la baja lógica de socios con préstamos sin devolver.

`PrestamoNegocio` valida que el socio exista y esté activo, que se registre al menos un libro sin repetirlo, que la fecha límite sea posterior, que el socio no exceda tres libros pendientes y que cada libro exista, esté activo y tenga stock. En la devolución calcula la multa de S/ 1.50 por día de retraso.

Estas reglas viven en **Negocio** porque deben funcionar igual desde cualquier interfaz y no corresponden a la persistencia. **Datos** solo ejecuta consultas y transacciones; **WPF** solo presenta información y muestra los mensajes de `ReglaNegocioException`.
