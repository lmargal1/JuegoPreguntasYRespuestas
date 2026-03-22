using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace JuegoPreguntasYRespuestas.Data
{
    public class JuegoDAO
    {
        private ConexionBD conexionBD = new ConexionBD();

        public List<Categoria> ObtenerCategorias()
        {
            List<Categoria> categorias = new List<Categoria>();
            using (MySqlConnection conexion = conexionBD.ObtenerConexion())
            {
                conexion.Open();
                string query = "SELECT * FROM Categorias";
                MySqlCommand cmd = new MySqlCommand(query, conexion);
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    Categoria cat = new Categoria {
                        IdCategoria = reader.GetInt32("idCategoria"),
                        NombreCategoria = reader.GetString("nombreCategoria")
                    };
                    categorias.Add(cat);
                }
            }
            return categorias;
        }

        public List<Pregunta> ObtenerPreguntasPorCategoria(int idCategoria)
        {
            List<Pregunta> preguntas = new List<Pregunta>();
            using (MySqlConnection conexion = conexionBD.ObtenerConexion())
            {
                conexion.Open();
                string query = "SELECT * FROM Preguntas WHERE idCategoria = @idCategoria ORDER BY RAND()"; 
                MySqlCommand cmd = new MySqlCommand(query, conexion);
                cmd.Parameters.AddWithValue("@idCategoria", idCategoria);
                MySqlDataReader reader = cmd.ExecuteReader();
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
            return preguntas;
        }

        public List<Opcion> ObtenerOpcionesPorPregunta(int idPregunta)
        {
            List<Opcion> opciones = new List<Opcion>();
            using (MySqlConnection conexion = conexionBD.ObtenerConexion())
            {
                conexion.Open();
                string query = "SELECT * FROM Opciones WHERE idPregunta = @idPregunta";
                MySqlCommand cmd = new MySqlCommand(query, conexion);
                cmd.Parameters.AddWithValue("@idPregunta", idPregunta);
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    opciones.Add(new Opcion {
                        IdOpcion = reader.GetInt32("idOpcion"),
                        IdPregunta = reader.GetInt32("idPregunta"),
                        TextoOpcion = reader.GetString("textoOpcion"),
                        RutaImagen = reader.IsDBNull(reader.GetOrdinal("rutaImagen")) ? null : reader.GetString("rutaImagen"),
                        EsCorrecta = reader.GetBoolean("esCorrecta")
                    });
                }
            }
            return opciones;
        }

        public void GuardarPartida(int idCat, int corr, int incorr)
        {
            try 
            {
                using (MySqlConnection conexion = conexionBD.ObtenerConexion())
                {
                    conexion.Open();
                    // Usamos @idCat para que coincida con el parámetro
                    string query = "INSERT INTO Partidas (idCategoria, correctas, incorrectas) VALUES (@idCat, @corr, @incorr)";
                    MySqlCommand cmd = new MySqlCommand(query, conexion);
                    
                    // Si idCat es 0 (Aleatorio), mandamos NULL a la base de datos
                    cmd.Parameters.AddWithValue("@idCat", idCat == 0 ? (object)DBNull.Value : idCat);
                    cmd.Parameters.AddWithValue("@corr", corr);
                    cmd.Parameters.AddWithValue("@incorr", incorr);
                    
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                // Si falla el guardado, lo imprimimos en consola pero NO cerramos el juego
                Console.WriteLine("Error al guardar partida: " + ex.Message);
            }
        }

        public List<Pregunta> ObtenerTodasLasPreguntas()
        {
            List<Pregunta> preguntas = new List<Pregunta>();
            using (MySqlConnection conexion = conexionBD.ObtenerConexion())
            {
                conexion.Open();
                string query = "SELECT * FROM Preguntas ORDER BY RAND() LIMIT 15"; 
                MySqlCommand cmd = new MySqlCommand(query, conexion); // <--- ESTA LÍNEA FALTABA
                MySqlDataReader reader = cmd.ExecuteReader();      // <--- ESTA TAMBIÉN
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
            return preguntas;
        }

        public List<string> ObtenerHistorial()
        {
            List<string> historial = new List<string>();
            using (MySqlConnection conexion = conexionBD.ObtenerConexion())
            {
                conexion.Open();
                string query = @"SELECT IFNULL(c.nombreCategoria, 'Aleatorio') as Cat, p.correctas, p.incorrectas 
                                 FROM Partidas p LEFT JOIN Categorias c ON p.idCategoria = c.idCategoria 
                                 ORDER BY p.idPartida DESC LIMIT 10";
                MySqlCommand cmd = new MySqlCommand(query, conexion);
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    historial.Add($"{reader["Cat"].ToString().PadRight(12)} | ✅{reader["correctas"]} | ❌{reader["incorrectas"]}");
                }
            }
            return historial;
        }
    }
}