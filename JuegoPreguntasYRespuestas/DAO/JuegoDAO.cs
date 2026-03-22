using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MySql.Data.MySqlClient;

namespace JuegoPreguntasYRespuestas.Data
{
    public class JuegoDAO
    {
        private ConexionBD conexionBD = new ConexionBD();

        //Obtener categorías
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
                    Categoria categoria = new Categoria();
                    categoria.IdCategoria = reader.GetInt32("idCategoria");
                    categoria.NombreCategoria = reader.GetString("nombreCategoria");
                    categorias.Add(categoria);
                }
            }
            return categorias;
        }

        //Obtener preguntas por categoría
        public List<Pregunta> ObtenerPreguntasPorCategoria(int idCategoria)
        {
            List<Pregunta> preguntas = new List<Pregunta>();

            using (MySqlConnection conexion = conexionBD.ObtenerConexion())
            {
                conexion.Open();

                //Para mostrar aleatoriamente las preguntas
                string query = "SELECT * FROM Preguntas WHERE idCategoria = @idCategoria ORDER BY RAND()"; 
                MySqlCommand cmd = new MySqlCommand(query, conexion);
                cmd.Parameters.AddWithValue("@idCategoria", idCategoria);
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    Pregunta pregunta = new Pregunta();
                    pregunta.IdPregunta = reader.GetInt32("idPregunta");
                    pregunta.TextoPregunta = reader.GetString("textoPregunta");
                    pregunta.Tipo = reader.GetString("tipo");
                    pregunta.IdCategoria = reader.GetInt32("idCategoria");
                    preguntas.Add(pregunta);
                }
            }
            return preguntas;
        }

        //Obtener opciones por pregunta
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
                    Opcion opcion = new Opcion();
                    opcion.IdOpcion = reader.GetInt32("idOpcion");
                    opcion.IdPregunta = reader.GetInt32("idPregunta");
                    opcion.TextoOpcion = reader.GetString("textoOpcion");

                    if (reader.IsDBNull(reader.GetOrdinal("rutaImagen")))
                        opcion.RutaImagen = null;
                    else
                        opcion.RutaImagen = reader.GetString("rutaImagen");

                    opcion.EsCorrecta = reader.GetBoolean("esCorrecta");
                    opciones.Add(opcion);
                }
            }
            return opciones;
        }

        //Guardar partida
        public void GuardarPartida(int idCategoria, int correctas, int incorrectas)
        {
            using (MySqlConnection conexion = conexionBD.ObtenerConexion())
            {
                conexion.Open();

                string query = "INSERT INTO Partidas (idCategoria, correctas, incorrectas) VALUES (@idCategoria, @correctas, @incorrectas)";
                MySqlCommand cmd = new MySqlCommand(query, conexion);
                cmd.Parameters.AddWithValue("@idCategoria", idCategoria);
                cmd.Parameters.AddWithValue("@correctas", correctas);
                cmd.Parameters.AddWithValue("@incorrectas", incorrectas);
                cmd.ExecuteNonQuery();
            }

        }

        // --- NUEVO MÉTODO: Obtener preguntas de TODAS las categorías al azar ---
        public List<Pregunta> ObtenerTodasLasPreguntas()
        {
            List<Pregunta> preguntas = new List<Pregunta>();

            using (MySqlConnection conexion = conexionBD.ObtenerConexion())
            {
                conexion.Open();

                // Traemos 15 preguntas aleatorias de cualquier categoría
                string query = "SELECT * FROM Preguntas ORDER BY RAND() LIMIT 15"; 
                MySqlCommand cmd = new MySqlCommand(query, conexion);
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    Pregunta pregunta = new Pregunta();
                    pregunta.IdPregunta = reader.GetInt32("idPregunta");
                    pregunta.TextoPregunta = reader.GetString("textoPregunta");
                    pregunta.Tipo = reader.GetString("tipo");
                    pregunta.IdCategoria = reader.GetInt32("idCategoria");
                    preguntas.Add(pregunta);
                }
            }
            return preguntas;
        }
    }
}