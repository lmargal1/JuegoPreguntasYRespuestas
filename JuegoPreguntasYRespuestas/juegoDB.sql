//Crea base de datos
CREATE DATABASE IF NOT EXISTS juegoDB
USE juegoDB

DROP TABLE IF EXISTS Opciones;
DROP TABLE IF EXISTS Preguntas;
DROP TABLE IF EXISTS Categorias;
DROP TABLE IF EXISTS Partidas;

//Crea tabla Categorias
CREATE TABLE Categorias
(
	idCategoria INT PRIMARY KEY AUTO_INCREMENT,
	nombreCategoria VARCHAR(100) NOT NULL
);

//Crea tabla Preguntas
CREATE TABLE Preguntas
(
	idPregunta INT PRIMARY KEY AUTO_INCREMENT,
	textoPregunta TEXT NOT NULL,
	tipo ENUM('texto', 'imagen') NOT NULL,
	idCategoria INT NOT NULL,
	FOREIGN KEY (idCategoria) REFERENCES Categorias(idCategoria)
);

//Crea tabla Opciones
CREATE TABLE Opciones
(
	idOpcion INT PRIMARY KEY AUTO_INCREMENT,
	idPregunta INT NOT NULL,
	textoOpcion TEXT NOT NULL,
	rutaImagen TEXT,
	esCorrecta BOOLEAN NOT NULL,
	FOREIGN KEY (idPregunta) REFERENCES Preguntas(idPregunta)
);

//Crea tabla Partidas
CREATE TABLE Partidas
(
	idPartida INT PRIMARY KEY AUTO_INCREMENT,
	idCategoria INT,
	correctas INT,
	incorrectas INT,
	FOREIGN KEY (idCategoria) REFERENCES Categorias (idCategoria)
);