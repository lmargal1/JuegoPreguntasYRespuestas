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


/*
SET SQL_SAFE_UPDATES = 0;
DELETE FROM preguntas;
SET SQL_SAFE_UPDATES = 1;
*/

//PREGUNTAS
//Inserta datos en tabla Preguntas
//Deportes -> idCategoria == 1
INSERT INTO Preguntas(textoPregunta, tipo, idCategoria) VALUES
('¿Cuántos sets se necesitan ganar para ganar un partido de singles varonil en un Grand Slam de tenis?', 'texto', 1),
('¿Cuántos pilotos participan actualmente en una carrera de Fórmula 1?', 'texto', 1),
('¿Cuál ha sido la pit stop más rápida en una carrera de Fórmula 1?', 'texto', 1),
('¿Cuántas Champions League ha ganado el Real Madrid?', 'texto', 1),
('¿En qué club se hizo famoso Cristiano Ronaldo antes de jugar por el Real Madrid?', 'texto', 1),

('¿Cuál tenista tiene más Grand Slams?', 'imagen', 1),
('¿Cuál es el circuito más antiguo de Fórmula 1?', 'imagen', 1),
('¿Quién de estos jugadores es Erling Haaland?', 'imagen', 1),
('¿Cuál de estos pilotos pertenece actualmente a la escudería Visa Cash App Racing Bulls?', 'imagen', 1),
('¿Cuál de estos pilotos ha estado en más escuderías durante su trayectoria en Fórmula 1?', 'imagen', 1);


//Música -> idCategoria == 2
INSERT INTO Preguntas(textoPregunta, tipo, idCategoria) VALUES
('¿Cuántas veces se dice la palabra "baby" en la canción con el mismo nombre, de Justin Bieber?', 'texto', 2),
('¿De qué país es originario el cantante Alex Soto?', 'texto', 2),
('¿Cuál es el título de la canción de Bad Bunny que menciona la frase "Debí tirar más fotos"?', 'texto', 2),
('¿Cuál compositor nació en Bonn, Alemania?', 'texto', 2),
('¿Cuál era el nombre real de Freddie Mercury?', 'texto', 2),

('¿Cuál de estas personas no es integrante de BTS?', 'imagen', 2),
('¿Cuál de estas es la portada del álbum "Esencia" de Humbe?', 'imagen', 2),
('¿Cuál de estas personas es el baterista de San Venus?', 'imagen', 2),
('¿Cuál de estas personas es el cantante Juice WRLD?', 'imagen', 2),
('¿Cuál es el cantante más famoso de todos los tiempos?', 'imagen', 2);


//Animales -> idCategoria == 3
INSERT INTO Preguntas(textoPregunta, tipo, idCategoria) VALUES
('¿Cómo se llama un grupo de búhos?', 'texto', 3),
('¿Cuál es el animal terrestre más rápido del mundo?', 'texto', 3),
('¿Cuál de estas especies se llaman entre sí por "nombres" únicos que consisten en silbidos específicos?', 'texto', 3),
('¿Cuál es el animal más grande que ha existido?', 'texto', 3),
('¿Cuál es el mamífero más inteligente?', 'texto', 3),

('¿Cuál de estos animales es un marsupial?', 'imagen', 3),
('¿Cuál de estos animales pertenece a la familia de los felinos?', 'imagen', 3),
('¿Cuál de estos animales es un mamífero marino?', 'imagen', 3),
('¿Cuál de estos es un dragón de komodo?', 'imagen', 3),
('¿Cuál de estos es un corvato?', 'imagen', 3);


//Curiosidades -> idCategoria == 4
INSERT INTO Preguntas(textoPregunta, tipo, idCategoria) VALUES
('¿Qué significan las siglas del restaurante TOKS?', 'texto', 4),
('¿Cuál es el único continente sin hormigas?', 'texto', 4),
('¿Cuál es el alimento más robado del mundo?', 'texto', 4),
('¿Cuál es el animal nacional de Escocia?', 'texto', 4),
('¿Cuál es la única letra que no aparece en el nombre de ningún estado de Estados Unidos?', 'texto', 4),

('¿Cuál de estos es el primer producto vendido en internet?', 'imagen', 4),
('¿Cuál de estos productos se vendía como medicina en la década de 1830?', 'imagen', 4),
('¿Cuál de estos logos tiene ese (o esos) color(es) debido al daltonismo de su creador?', 'imagen', 4),
('¿Cuál es el emoji más utilizado en el mundo?', 'imagen', 4),
('¿En cuál de estos planetas llueven diamantes?', 'imagen', 4);
