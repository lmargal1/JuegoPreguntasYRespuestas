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


//PREGUNTAS
//Inserta datos en tabla Preguntas
//Deportes -> idCategoria == 1
INSERT INTO Preguntas(textoPregunta, tipo, idCategoria) VALUES
('¿Cuántos sets se necesitan ganar para ganar un partido de singles varonil en un Grand Slam de tenis?', 'texto', 1),
('¿Cuántos pilotos participan actualmente en una carrera de Fórmula 1?', 'texto', 1),
('¿Cuál ha sido la pit stop más rápida en una carrera de Fórmula 1?', 'texto', 1),
('¿Cuántas Champions League ha ganado el Real Madrid?', 'texto', 1),
('¿En qué club se hizo famoso Cristiano Ronaldo antes de jugar por el Real Madrid?', 'texto', 1),

('¿Cuál tenista tiene más Grand Slams?', 'texto', 1),
('¿Cuál es el circuito más antiguo de Fórmula 1?', 'texto', 1),
('¿Quién de estos jugadores es Erling Haaland?', 'texto', 1),
('¿Cuál de estos pilotos pertenece actualmente a la escudería Visa Cash App Racing Bulls?', 'texto', 1),
('¿Cuál de estos pilotos ha estado en más escuderías durante su trayectoria en Fórmula 1?', 'texto', 1);

