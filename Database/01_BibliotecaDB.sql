-- Script pendiente
USE master;
GO
IF DB_ID('BibliotecaDB') IS NOT NULL
BEGIN
    ALTER DATABASE BibliotecaDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE BibliotecaDB;
END
GO
CREATE DATABASE BibliotecaDB;
GO
USE BibliotecaDB;
GO
CREATE TABLE Autores (AutorId INT IDENTITY(1,1) PRIMARY KEY, Nombre NVARCHAR(100) NOT NULL, Nacionalidad NVARCHAR(50) NOT NULL, Activo BIT NOT NULL DEFAULT 1);
CREATE TABLE Libros (LibroId INT IDENTITY(1,1) PRIMARY KEY, Titulo NVARCHAR(150) NOT NULL, ISBN NVARCHAR(20) NOT NULL UNIQUE, AutorId INT NOT NULL, Ejemplares INT NOT NULL, Activo BIT NOT NULL DEFAULT 1, CONSTRAINT FK_Libros_Autores FOREIGN KEY (AutorId) REFERENCES Autores(AutorId));
CREATE TABLE Socios (SocioId INT IDENTITY(1,1) PRIMARY KEY, DNI NVARCHAR(8) NOT NULL UNIQUE, Nombre NVARCHAR(100) NOT NULL, Email NVARCHAR(100) NOT NULL, Activo BIT NOT NULL DEFAULT 1);
CREATE TABLE Prestamos (PrestamoId INT IDENTITY(1,1) PRIMARY KEY, SocioId INT NOT NULL, FechaPrestamo DATETIME NOT NULL DEFAULT GETDATE(), FechaLimite DATETIME NOT NULL, Estado NVARCHAR(20) NOT NULL DEFAULT 'Pendiente', CONSTRAINT FK_Prestamos_Socios FOREIGN KEY (SocioId) REFERENCES Socios(SocioId));
CREATE TABLE DetallePrestamo (PrestamoId INT NOT NULL, LibroId INT NOT NULL, FechaDevolucion DATETIME NULL, CONSTRAINT PK_DetallePrestamo PRIMARY KEY (PrestamoId, LibroId), CONSTRAINT FK_Detalle_Prestamos FOREIGN KEY (PrestamoId) REFERENCES Prestamos(PrestamoId), CONSTRAINT FK_Detalle_Libros FOREIGN KEY (LibroId) REFERENCES Libros(LibroId));
GO
INSERT INTO Autores (Nombre, Nacionalidad) VALUES ('Gabriel García Márquez','Colombiana'),('Mario Vargas Llosa','Peruana'),('Isabel Allende','Chilena'),('Jorge Luis Borges','Argentina'),('Julio Cortázar','Argentina'),('Pablo Neruda','Chilena'),('Octavio Paz','Mexicana'),('José Saramago','Portuguesa');
INSERT INTO Libros (Titulo, ISBN, AutorId, Ejemplares) VALUES ('Cien años de soledad','978-84-376-0494-7',1,5),('El amor en los tiempos del cólera','978-0-307-38973-2',1,3),('Crónica de una muerte anunciada','978-1-4000-3471-0',1,4),('La ciudad y los perros','978-84-204-7183-9',2,6),('La fiesta del chivo','978-84-204-4279-2',2,2),('Conversación en La Catedral','978-84-376-0691-0',2,3),('La casa de los espíritus','978-84-01-34185-3',3,5),('De amor y de sombra','978-84-01-38062-3',3,4),('Paula','978-0-06-092721-6',3,3),('Ficciones','978-84-206-3313-8',4,2),('El Aleph','978-84-206-3311-4',4,3),('Rayuela','978-84-376-2474-7',5,4),('Bestiario','978-84-376-0967-6',5,2),('Canto General','978-84-376-0275-2',6,3),('Veinte poemas de amor y una canción desesperada','978-84-206-3315-2',6,5),('El laberinto de la soledad','978-84-376-1188-4',7,4),('La llama doble','978-84-322-0693-1',7,3),('Ensayo sobre la ceguera','978-84-204-4286-0',8,5),('El evangelio según Jesucristo','978-84-204-4288-4',8,2),('Todos los nombres','978-84-204-8242-2',8,3);
INSERT INTO Socios (DNI, Nombre, Email) VALUES ('12345678','Juan Pérez','juan.perez@email.com'),('23456789','María González','maria.g@email.com'),('34567890','Carlos Ruiz','cruiz@email.com'),('45678901','Ana Silva','ana.silva@email.com'),('56789012','Luis Torres','luis.t@email.com'),('67890123','Laura Méndez','laura.m@email.com'),('78901234','Diego Vargas','diego.vargas@email.com'),('89012345','Sofía Castro','sofia.c@email.com'),('90123456','Miguel Rojas','mrojas@email.com'),('01234567','Lucía Vega','lucia.vega@email.com');
INSERT INTO Prestamos (SocioId, FechaPrestamo, FechaLimite, Estado) VALUES (1,'20230110','20230117','Devuelto');
INSERT INTO DetallePrestamo VALUES (1,1,'20230115');
INSERT INTO Prestamos (SocioId, FechaPrestamo, FechaLimite, Estado) VALUES (2,GETDATE(),DATEADD(day,7,GETDATE()),'Pendiente');
INSERT INTO DetallePrestamo VALUES (2,4,NULL),(2,7,NULL);
UPDATE Libros SET Ejemplares=Ejemplares-1 WHERE LibroId IN(4,7);
INSERT INTO Prestamos (SocioId, FechaPrestamo, FechaLimite, Estado) VALUES (3,'20230901','20230908','Pendiente');
INSERT INTO DetallePrestamo VALUES (3,10,NULL);
UPDATE Libros SET Ejemplares=Ejemplares-1 WHERE LibroId=10;
INSERT INTO Prestamos (SocioId, FechaPrestamo, FechaLimite, Estado) VALUES (4,GETDATE(),DATEADD(day,7,GETDATE()),'Pendiente');
INSERT INTO DetallePrestamo VALUES (4,12,NULL),(4,14,NULL),(4,18,NULL);
UPDATE Libros SET Ejemplares=Ejemplares-1 WHERE LibroId IN(12,14,18);
INSERT INTO Prestamos (SocioId, FechaPrestamo, FechaLimite, Estado) VALUES (5,GETDATE()-2,DATEADD(day,5,GETDATE()),'Pendiente');
INSERT INTO DetallePrestamo VALUES (5,2,GETDATE()-1),(5,5,NULL);
UPDATE Libros SET Ejemplares=Ejemplares-1 WHERE LibroId=5;
GO