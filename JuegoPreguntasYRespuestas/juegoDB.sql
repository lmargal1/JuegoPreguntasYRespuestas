//Crea base de datos
CREATE DATABASE IF NOT EXISTS juegoDB
USE juegoDB

DROP TABLE IF EXISTS Opciones;
DROP TABLE IF EXISTS Preguntas;
DROP TABLE IF EXISTS Categorias;
DROP TABLE IF EXISTS Partidas;

//TABLAS
//Crea tabla Categorias
CREATE TABLE Categorias
(
	idCategoria INT PRIMARY KEY AUTO_INCREMENT NOT NULL,
	nombreCategoria VARCHAR(100) NOT NULL
);

//Crea tabla Preguntas
CREATE TABLE Preguntas
(
	idPregunta INT PRIMARY KEY AUTO_INCREMENT NOT NULL,
	textoPregunta TEXT NOT NULL,
	tipo ENUM('texto', 'imagen') NOT NULL,
	idCategoria INT NOT NULL,
	FOREIGN KEY (idCategoria) REFERENCES Categorias(idCategoria)
);

//Crea tabla Opciones
CREATE TABLE Opciones
(
	idOpcion INT PRIMARY KEY AUTO_INCREMENT NOT NULL,
	idPregunta INT NOT NULL,
	textoOpcion TEXT NOT NULL,
	rutaImagen TEXT,
	esCorrecta BOOLEAN NOT NULL,
	FOREIGN KEY (idPregunta) REFERENCES Preguntas(idPregunta)
);

//Crea tabla Partidas
CREATE TABLE Partidas
(
	idPartida INT PRIMARY KEY AUTO_INCREMENT NOT NULL,
	idCategoria INT NOT NULL,
	correctas INT NOT NULL,
	incorrectas INT NOT NULL,
	FOREIGN KEY (idCategoria) REFERENCES Categorias (idCategoria)
);


//CATEGORÍAS
//Inserta datos en tabla Categorias
INSERT INTO Categorias(nombreCategoria) VALUES
('Deportes'), ('Música'), ('Animales'), ('Curiosidades'), ('Películas');

