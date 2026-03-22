-- Crea base de datos
CREATE DATABASE IF NOT EXISTS juegoDB;
USE juegoDB;

DROP TABLE IF EXISTS Opciones;
DROP TABLE IF EXISTS Preguntas;
DROP TABLE IF EXISTS Categorias;
DROP TABLE IF EXISTS Partidas;

-- TABLAS
-- Crea tabla Categorias
CREATE TABLE Categorias
(
	idCategoria INT PRIMARY KEY AUTO_INCREMENT NOT NULL,
	nombreCategoria VARCHAR(100) NOT NULL
);

-- Crea tabla Preguntas
CREATE TABLE Preguntas
(
	idPregunta INT PRIMARY KEY AUTO_INCREMENT NOT NULL,
	textoPregunta TEXT NOT NULL,
	tipo ENUM('texto', 'imagen') NOT NULL,
	idCategoria INT NOT NULL,
	FOREIGN KEY (idCategoria) REFERENCES Categorias(idCategoria)
);

-- Crea tabla Opciones
CREATE TABLE Opciones
(
	idOpcion INT PRIMARY KEY AUTO_INCREMENT NOT NULL,
	idPregunta INT NOT NULL,
	textoOpcion TEXT NOT NULL,
	rutaImagen TEXT,
	esCorrecta BOOLEAN NOT NULL,
	FOREIGN KEY (idPregunta) REFERENCES Preguntas(idPregunta)
);

-- Crea tabla Partidas
CREATE TABLE Partidas
(
	idPartida INT PRIMARY KEY AUTO_INCREMENT NOT NULL,
	idCategoria INT NOT NULL,
	correctas INT NOT NULL,
	incorrectas INT NOT NULL,
	FOREIGN KEY (idCategoria) REFERENCES Categorias (idCategoria)
);


-- CATEGORÍAS
-- Inserta datos en tabla Categorias
INSERT INTO Categorias(nombreCategoria) VALUES
('Deportes'), ('Música'), ('Animales'), ('Curiosidades'), ('Películas');


-- PREGUNTAS
-- Deportes -> idCategoria = 1
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

-- Música -> idCategoria = 2
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

-- Animales -> idCategoria = 3
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

-- Curiosidades -> idCategoria = 4
INSERT INTO Preguntas(textoPregunta, tipo, idCategoria) VALUES
('¿Qué significan las siglas del restaurante TOKS?', 'texto', 4),
('¿Cuál es el único continente sin hormigas?', 'texto', 4),
('¿Cuál es el alimento más robado del mundo?', 'texto', 4),
('¿Cuál es el animal nacional de Escocia?', 'texto', 4),
('¿Cuál es la única letra que no aparece en el nombre de ningún estado de Estados Unidos?', 'texto', 4),
('¿Cuál de estos es el primer producto vendido en internet?', 'imagen', 4),
('¿Cuál de estos productos se vendía como medicina en la década de 1830?', 'imagen', 4),
('¿Cuál de estos logos tiene ese color debido al daltonismo de su creador?', 'imagen', 4),
('¿Cuál es el emoji más utilizado en el mundo?', 'imagen', 4),
('¿En cuál de estos planetas llueven diamantes?', 'imagen', 4);

-- Películas -> idCategoria = 5
INSERT INTO Preguntas(textoPregunta, tipo, idCategoria) VALUES
('¿Quién es el protagonista de la película "Volver al futuro"?', 'texto', 5),
('¿Cómo se llama el arma característica de los Jedi en Star Wars?', 'texto', 5),
('¿En qué episodio de Star Wars nace el personaje Darth Vader?', 'texto', 5),
('¿Quién escribió la novela en la que se basa "La máquina del tiempo"?', 'texto', 5),
('¿En qué fecha Marty McFly y Doc Brown llegaron al futuro?', 'texto', 5),
('¿Cuál es la película más costosa de la historia?', 'imagen', 5),
('¿La esposa de cuál actor ha aparecido en más de 20 películas junto a su esposo?', 'imagen', 5),
('¿Cuál de estos actores ha participado en más películas?', 'imagen', 5),
('¿Cuál de estos personajes no ha sido doblado al español por el actor mexicano Omar Chaparro?', 'imagen', 5),
('¿Cuál fue la primera película en ganar el Oscar a Mejor Película Animada?', 'imagen', 5);


-- OPCIONES
-- Preguntas 1 a 5 (Deportes - Texto)
INSERT INTO Opciones (idPregunta, textoOpcion, rutaImagen, esCorrecta) VALUES
(1, '3', NULL, true), (1, '2', NULL, false), (1, '4', NULL, false), (1, '5', NULL, false),
(2, '18', NULL, false), (2, '20', NULL, false), (2, '22', NULL, true), (2, '24', NULL, false),
(3, '2.5 segundos', NULL, false), (3, '1.8 segundos', NULL, true), (3, '1.6 segundos', NULL, false), (3, '3.1 segundos', NULL, false),
(4, '17', NULL, true), (4, '16', NULL, false), (4, '15', NULL, false), (4, '14', NULL, false),
(5, 'Bayern Munich', NULL, false), (5, 'Manchester United', NULL, true), (5, 'Juventus', NULL, false), (5, 'Manchester City', NULL, false);

-- Preguntas 6 a 10 (Deportes - Imagen)
INSERT INTO Opciones (idPregunta, textoOpcion, rutaImagen, esCorrecta) VALUES
(6, 'Imagen A', 'Presentacion/Imagenes/deportes/novak.jpg', true), (6, 'Imagen B', 'Presentacion/Imagenes/deportes/serena.jpg', false), (6, 'Imagen C', 'Presentacion/Imagenes/deportes/federer.jpg', false), (6, 'Imagen D', 'Presentacion/Imagenes/deportes/nadal.jpg', false),
(7, 'Imagen A', 'Presentacion/Imagenes/deportes/spa.jpg', false), (7, 'Imagen B', 'Presentacion/Imagenes/deportes/silverstone.jpg', false), (7, 'Imagen C', 'Presentacion/Imagenes/deportes/monaco.jpg', true), (7, 'Imagen D', 'Presentacion/Imagenes/deportes/monza.jpg', false),
(8, 'Imagen A', 'Presentacion/Imagenes/deportes/haaland.jpg', true), (8, 'Imagen B', 'Presentacion/Imagenes/deportes/mbappe.jpg', false), (8, 'Imagen C', 'Presentacion/Imagenes/deportes/yamal.jpg', false), (8, 'Imagen D', 'Presentacion/Imagenes/deportes/bellingham.jpg', false),
(9, 'Imagen A', 'Presentacion/Imagenes/deportes/kimi.jpeg', false), (9, 'Imagen B', 'Presentacion/Imagenes/deportes/sainz.jpeg', false), (9, 'Imagen C', 'Presentacion/Imagenes/deportes/bottas.jpeg', false), (9, 'Imagen D', 'Presentacion/Imagenes/deportes/lindblad.jpeg', true),
(10, 'Imagen A', 'Presentacion/Imagenes/deportes/sainz.jpeg', false), (10, 'Imagen B', 'Presentacion/Imagenes/deportes/hamilton.jpeg', false), (10, 'Imagen C', 'Presentacion/Imagenes/deportes/alonso.jpg', true), (10, 'Imagen D', 'Presentacion/Imagenes/deportes/maxi.jpeg', false);

-- Preguntas 11 a 15 (Música - Texto)
INSERT INTO Opciones (idPregunta, textoOpcion, rutaImagen, esCorrecta) VALUES
(11, '56', NULL, true), (11, '32', NULL, false), (11, '40', NULL, false), (11, '48', NULL, false),
(12, 'México', NULL, true), (12, 'Argentina', NULL, false), (12, 'España', NULL, false), (12, 'Colombia', NULL, false),
(13, 'DTMF', NULL, false), (13, 'DtMf', NULL, false), (13, 'DTmF', NULL, false), (13, 'DtMF', NULL, true),
(14, 'Mozart', NULL, false), (14, 'Beethoven', NULL, true), (14, 'Bach', NULL, false), (14, 'Chopin', NULL, false),
(15, 'Frederick Mercury', NULL, false), (15, 'Michael Bulsara', NULL, false), (15, 'Farrokh Bulsara', NULL, true), (15, 'Arthur Mercury', NULL, false);

-- Preguntas 16 a 20 (Música - Imagen)
INSERT INTO Opciones (idPregunta, textoOpcion, rutaImagen, esCorrecta) VALUES
(16, 'Imagen A', 'Presentacion/Imagenes/musica/namjoon.jpg', false), (16, 'Imagen B', 'Presentacion/Imagenes/musica/jin.jpg', false), (16, 'Imagen C', 'Presentacion/Imagenes/musica/yoongi.jpg', false), (16, 'Imagen D', 'Presentacion/Imagenes/musica/sanha.png', true),
(17, 'Imagen A', 'Presentacion/Imagenes/musica/astros.jpeg', false), (17, 'Imagen B', 'Presentacion/Imagenes/musica/esencia.jpeg', true), (17, 'Imagen C', 'Presentacion/Imagenes/musica/aurora.jpeg', false), (17, 'Imagen D', 'Presentacion/Imagenes/musica/ddc.jpeg', false),
(18, 'Imagen A', 'Presentacion/Imagenes/musica/rick.jpeg', true), (18, 'Imagen B', 'Presentacion/Imagenes/musica/uxi.jpeg', false), (18, 'Imagen C', 'Presentacion/Imagenes/musica/mike.jpeg', false), (18, 'Imagen D', 'Presentacion/Imagenes/musica/blnko.jpeg', false),
(19, 'Imagen A', 'Presentacion/Imagenes/musica/liluzi.jpg', false), (19, 'Imagen B', 'Presentacion/Imagenes/musica/travis.jpg', false), (19, 'Imagen C', 'Presentacion/Imagenes/musica/juiceWRLD.jpg', true), (19, 'Imagen D', 'Presentacion/Imagenes/musica/xxxtentacion.jpg', false),
(20, 'Imagen A', 'Presentacion/Imagenes/musica/elvis.jpg', false), (20, 'Imagen B', 'Presentacion/Imagenes/musica/mercury.jpg', false), (20, 'Imagen C', 'Presentacion/Imagenes/musica/sinatra.jpg', false), (20, 'Imagen D', 'Presentacion/Imagenes/musica/mj.jpg', true);

-- Preguntas 21 a 25 (Animales - Texto)
INSERT INTO Opciones (idPregunta, textoOpcion, rutaImagen, esCorrecta) VALUES
(21, 'Parvada', NULL, false), (21, 'Bandada', NULL, false), (21, 'Colonia', NULL, false), (21, 'Parlamento', NULL, true),
(22, 'León', NULL, false), (22, 'Guepardo', NULL, true), (22, 'Antílope', NULL, false), (22, 'Avestruz', NULL, false),
(23, 'Delfines', NULL, true), (23, 'Ballenas', NULL, false), (23, 'Lobos', NULL, false), (23, 'Canarios', NULL, false),
(24, 'Elefante africano', NULL, false), (24, 'Patagotitan', NULL, false), (24, 'Ballena Azul', NULL, true), (24, 'Mamut', NULL, false),
(25, 'Chimpancé', NULL, false), (25, 'Hombre', NULL, true), (25, 'Delfín', NULL, false), (25, 'Elefante', NULL, false);

-- Preguntas 26 a 30 (Animales - Imagen)
INSERT INTO Opciones (idPregunta, textoOpcion, rutaImagen, esCorrecta) VALUES
(26, 'Imagen A', 'Presentacion/Imagenes/animales/marsupial.jpg', true), (26, 'Presentacion/Imagenes/animales/oso.jpg', false), (26, 'Imagen C', 'Presentacion/Imagenes/animales/perezoso.jpg', false), (26, 'Imagen D', 'Presentacion/Imagenes/animales/ratita.jpg', false),
(27, 'Imagen A', 'Presentacion/Imagenes/animales/tigre.jpg', true), (27, 'Imagen B', 'Presentacion/Imagenes/animales/civeta.jpg', false), (27, 'Imagen C', 'Presentacion/Imagenes/animales/fossa.jpg', false), (27, 'Imagen D', 'Presentacion/Imagenes/animales/lingsang.jpg', false),
(28, 'Imagen A', 'Presentacion/Imagenes/animales/tortuga.png', false), (28, 'Imagen B', 'Presentacion/Imagenes/animales/tiburon.png', false), (28, 'Imagen C', 'Presentacion/Imagenes/animales/dugongo.jpg', true), (28, 'Imagen D', 'Presentacion/Imagenes/animales/pezLuna.jpg', false),
(29, 'Imagen A', 'Presentacion/Imagenes/animales/varano.jpg', false), (29, 'Imagen B', 'Presentacion/Imagenes/animales/monitorAgua.jpg', false), (29, 'Imagen C', 'Presentacion/Imagenes/animales/cocodrilo.jpg', false), (29, 'Imagen D', 'Presentacion/Imagenes/animales/komodo.jpg', false),
(30, 'Imagen A', 'Presentacion/Imagenes/animales/codorniz.jpg', false), (30, 'Imagen B', 'Presentacion/Imagenes/animales/cigoñino.jpg', false), (30, 'Imagen C', 'Presentacion/Imagenes/animales/perdigon.jpg', false), (30, 'Imagen D', 'Presentacion/Imagenes/animales/corvato.jpg', false);

-- Preguntas 31 a 35 (Curiosidades - Texto)
INSERT INTO Opciones (idPregunta, textoOpcion, rutaImagen, esCorrecta) VALUES
(31, 'Skot (apellido del fundador al revés', NULL, false), (31, 'Today Okay Service', NULL, true), (31, 'The Original Kitchen Service', NULL, false), (31, 'No significa nada', NULL, false),
(32, 'África', NULL, false), (32, 'Oceanía', NULL, false), (32, 'Asia', NULL, false), (32, 'Antártida', NULL, true),
(33, 'Queso', NULL, true), (33, 'Pan', NULL, false), (33, 'Chocolate', NULL, false), (33, 'Café', NULL, false),
(34, 'Dragón', NULL, false), (34, 'Águila', NULL, false), (34, 'Unicornio', NULL, true), (34, 'Caballo', NULL, false),
(35, 'J', NULL, false), (35, 'K', NULL, false), (35, 'Z', NULL, false), (35, 'Q', NULL, true);

-- Preguntas 36 a 40 (Curiosidades - Imagen)
INSERT INTO Opciones (idPregunta, textoOpcion, rutaImagen, esCorrecta) VALUES
(36, 'Imagen A', 'Presentacion/Imagenes/curiosidades/cd.png', true), (36, 'Imagen B', 'Presentacion/Imagenes/curiosidades/crema.png', false), (36, 'Imagen C', 'Presentacion/Imagenes/curiosidades/libro.png', false), (36, 'Imagen D', 'Presentacion/Imagenes/curiosidades/pizza.png', false),
(37, 'Imagen A', 'Presentacion/Imagenes/curiosidades/mayonesa.jpg', false), (37, 'Imagen B', 'Presentacion/Imagenes/curiosidades/catsup.png', true), (37, 'Imagen C', 'Presentacion/Imagenes/curiosidades/cannabis.jpeg', false), (37, 'Imagen D', 'Presentacion/Imagenes/curiosidades/chocolate.jpg', false),
(38, 'Imagen A', 'Presentacion/Imagenes/curiosidades/twitter.png', false), (38, 'Imagen B', 'Presentacion/Imagenes/curiosidades/vine.jpg', false), (38, 'Imagen C', 'Presentacion/Imagenes/curiosidades/facebook.jpg', true), (38, 'Imagen D', 'Presentacion/Imagenes/curiosidades/pinterest.png', false),
(39, 'Imagen A', 'Presentacion/Imagenes/curiosidades/feliz.png', false), (39, 'Imagen B', 'Presentacion/Imagenes/curiosidades/fuego.png', false), (39, 'Imagen C', 'Presentacion/Imagenes/curiosidades/risa.png', false), (39, 'Imagen D', 'Presentacion/Imagenes/curiosidades/corazon.png', true),
(40, 'Imagen A', 'Presentacion/Imagenes/curiosidades/mercurio.jpg', false), (40, 'Imagen B', 'Presentacion/Imagenes/curiosidades/jupiter.jpg', false), (40, 'Imagen C', 'Presentacion/Imagenes/curiosidades/marte.jpg', false), (40, 'Imagen D', 'Presentacion/Imagenes/curiosidades/neptuno.jpg', true);

-- Preguntas 41 a 45 (Películas - Texto)
INSERT INTO Opciones (idPregunta, textoOpcion, rutaImagen, esCorrecta) VALUES
(41, 'Marty McFly', NULL, true), (41, 'Doc Brown', NULL, false), (41, 'Biff Tannen', NULL, false), (41, 'Martin McFly', NULL, false),
(42, 'Sable obscuro', NULL, false), (42, 'Sable de luz', NULL, true), (42, 'Espada Jedi', NULL, false), (42, 'Espada láser', NULL, false),
(43, 'Episodio I', NULL, false), (43, 'Episodio II', NULL, false), (43, 'Episodio III', NULL, true), (43, 'Episodio IV', NULL, false),
(44, 'Jules Verne', NULL, false), (44, 'Isaac Asimov', NULL, false), (44, 'Arthur C. Clarke', NULL, false), (44, 'H. G. Wells', NULL, true),
(45, '15 de octubre de 2015', NULL, false), (45, '5 de mayo de 2000', NULL, false), (45, '21 de octubre de 2015', NULL, true), (45, '15 de agosto de 2012', NULL, false);

-- Preguntas 46 a 50 (Películas - Imagen)
INSERT INTO Opciones (idPregunta, textoOpcion, rutaImagen, esCorrecta) VALUES
(46, 'Imagen A', 'Presentacion/Imagenes/peliculas/jurassic.jpg', false), (46, 'Imagen B', 'Presentacion/Imagenes/peliculas/sw2.png', false), (46, 'Imagen C', 'Presentacion/Imagenes/peliculas/piratas.png', false), (46, 'Imagen D', 'Presentacion/Imagenes/peliculas/epVII.jpg', true),
(47, 'Imagen A', 'Presentacion/Imagenes/peliculas/henry.png', false), (47, 'Imagen B', 'Presentacion/Imagenes/peliculas/liam.png', false), (47, 'Imagen C', 'Presentacion/Imagenes/peliculas/adam.png', true), (47, 'Imagen D', 'Presentacion/Imagenes/peliculas/mbj.png', false),
(48, 'Imagen A', 'Presentacion/Imagenes/peliculas/chan.jpg', false), (48, 'Imagen B', 'Presentacion/Imagenes/peliculas/danny.jpeg', true), (48, 'Imagen C', 'Presentacion/Imagenes/peliculas/lee.png', false), (48, 'Imagen D', 'Presentacion/Imagenes/peliculas/samuel.jpg', false),
(49, 'Imagen A', 'Presentacion/Imagenes/peliculas/shrek.png', true), (49, 'Imagen B', 'Presentacion/Imagenes/peliculas/sindrome.jpg', false), (49, 'Imagen C', 'Presentacion/Imagenes/peliculas/poo.png', false), (49, 'Imagen D', 'Presentacion/Imagenes/peliculas/abuelita.png', false),
(50, 'Imagen A', 'Presentacion/Imagenes/peliculas/monsters.png', false), (50, 'Imagen B', 'Presentacion/Imagenes/peliculas/shrekPic.jpg', true), (50, 'Imagen C', 'Presentacion/Imagenes/peliculas/toyStory.jpg', false), (50, 'Imagen D', 'Presentacion/Imagenes/peliculas/wg.png', false);