using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;


namespace JuegoPreguntasYRespuestas.DAO
{
    public class JuegoDao
    {
        private readonly ConexionBD _conexionBd = new ConexionBD();

        public List<Categoria> ObtenerCategorias()
        {
            var categorias = new List<Categoria>();
            using (var conexion = _conexionBd.ObtenerConexion())
            {
                conexion.Open();
                const string query = "SELECT * FROM Categorias";
                var cmd = new MySqlCommand(query, conexion);
                
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        categorias.Add(new Categoria {
                            IdCategoria = reader.GetInt32("idCategoria"),
                            NombreCategoria = reader.GetString("nombreCategoria")
                        });
                    }
                }
            }
            return categorias;
        }

        public List<Pregunta> ObtenerPreguntasPorCategoria(int idCategoria)
        {
            var preguntas = new List<Pregunta>();
            using (var conexion = _conexionBd.ObtenerConexion())
            {
                conexion.Open();
                
                const string query = "SELECT * FROM Preguntas WHERE idCategoria = @idCategoria ORDER BY RAND()"; 
                var cmd = new MySqlCommand(query, conexion);
                cmd.Parameters.AddWithValue("@idCategoria", idCategoria);
                
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        preguntas.Add(new Pregunta {
                            IdPregunta = reader.GetInt32("idPregunta"),
                            TextoPregunta = reader.GetString("textoPregunta"),
                            Tipo = reader.GetString("tipo"),
                            IdCategoria = reader.GetInt32("idCategoria")
                        });
                    }
                }
            }
            return preguntas;
        }

        public List<Opcion> ObtenerOpcionesPorPregunta(int idPregunta)
        {
            var opciones = new List<Opcion>();
            using (var conexion = _conexionBd.ObtenerConexion())
            {
                conexion.Open();
                
                const string query = "SELECT * FROM Opciones WHERE idPregunta = @idPregunta";
                var cmd = new MySqlCommand(query, conexion);
                cmd.Parameters.AddWithValue("@idPregunta", idPregunta);
                
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        opciones.Add(new Opcion {
                            IdOpcion = reader.GetInt32("idOpcion"),
                            IdPregunta = reader.GetInt32("idPregunta"),
                            TextoOpcion = reader.GetString("textoOpcion"),
                            
                            // Validamos si el campo de imagen está nulo en la BD para que no crashee
                            RutaImagen = reader.IsDBNull(reader.GetOrdinal("rutaImagen")) ? null : reader.GetString("rutaImagen"),
                            EsCorrecta = reader.GetBoolean("esCorrecta")
                        });
                    }
                }
            }
            return opciones;
        }

        //Guardar partida
        public int GuardarPartida(int idCategoria, int correctas, int incorrectas)
        {
            try 
            {
                conexion.Open();

                string query = "INSERT INTO Partidas (idCategoria, correctas, incorrectas) VALUES (@idCategoria, @correctas, @incorrectas); SELECT LAST_INSERT_ID();";
                MySqlCommand cmd = new MySqlCommand(query, conexion);
                cmd.Parameters.AddWithValue("@idCategoria", idCategoria);
                cmd.Parameters.AddWithValue("@correctas", correctas);
                cmd.Parameters.AddWithValue("@incorrectas", incorrectas);

                int idPartida = Convert.ToInt32(cmd.ExecuteScalar());
                return idPartida;
            }
        }

        //Guardar respuesta 
        public void GuardarRespuesta(int idPartida, int idPregunta, bool esCorrecta)
        {
            using (MySqlConnection conexion = conexionBD.ObtenerConexion())
            {
                conexion.Open();
                string query = "INSERT INTO RespuestaJugador(idPartida, idPregunta, esCorrecta) VALUES (@idPartida, @idPregunta, @esCorrecta)";
                MySqlCommand cmd = new MySqlCommand(query, conexion);
                cmd.Parameters.AddWithValue("@idPartida", idPartida);
                cmd.Parameters.AddWithValue("@idPregunta", idPregunta);
                cmd.Parameters.AddWithValue("@esCorrecta", esCorrecta);
                cmd.ExecuteNonQuery();
            }
        }

        //Obtener preguntas de TODAS las categorías al azar
        public List<Pregunta> ObtenerTodasLasPreguntas()
        {
            var preguntas = new List<Pregunta>();
            using (var conexion = _conexionBd.ObtenerConexion())
            {
                conexion.Open();
                
                const string query = "SELECT * FROM Preguntas ORDER BY RAND() LIMIT 15"; 
                var cmd = new MySqlCommand(query, conexion);
                
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        preguntas.Add(new Pregunta {
                            IdPregunta = reader.GetInt32("idPregunta"),
                            TextoPregunta = reader.GetString("textoPregunta"),
                            Tipo = reader.GetString("tipo"),
                            IdCategoria = reader.GetInt32("idCategoria")
                        });
                    }
                }
            }
            return preguntas;
        }

        public List<string> ObtenerHistorial()
        {
            var historial = new List<string>();
            try {
                using (var conexion = _conexionBd.ObtenerConexion())
                {
                    conexion.Open();
                    
                    const string query = @"SELECT IFNULL(c.nombreCategoria, 'Aleatorio') as Cat, p.correctas, p.incorrectas 
                                     FROM Partidas p LEFT JOIN Categorias c ON p.idCategoria = c.idCategoria 
                                     ORDER BY p.idPartida DESC LIMIT 10";
                    var cmd = new MySqlCommand(query, conexion);
                    
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            historial.Add($"{reader["Cat"].ToString().PadRight(12)} | ✅{reader["correctas"]} | ❌{reader["incorrectas"]}");
                        }
                    }
                }
            } catch (Exception ex) { Console.WriteLine(@"Error leyendo Historial: " + ex.Message); }
            return historial;
        }
    }
}