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
                        categorias.Add(new Categoria
                        {
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
                        preguntas.Add(new Pregunta
                        {
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
                        opciones.Add(new Opcion
                        {
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

        // Guardar partida -> devuelve el idPartida generado
        public int GuardarPartida(string nombreJugador, int? idCategoria, int correctas, int incorrectas)
        {
            try
            {
                using (var conexion = _conexionBd.ObtenerConexion())
                {
                    conexion.Open();

                    const string query = @"
                INSERT INTO Partidas (nombreJugador, idCategoria, correctas, incorrectas, fechaPartida)
                VALUES (@nombreJugador, @idCategoria, @correctas, @incorrectas, NOW());
                SELECT LAST_INSERT_ID();";

                    var cmd = new MySqlCommand(query, conexion);
                    cmd.Parameters.AddWithValue("@nombreJugador", nombreJugador);
                    cmd.Parameters.AddWithValue("@idCategoria", (object)idCategoria ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@correctas", correctas);
                    cmd.Parameters.AddWithValue("@incorrectas", incorrectas);

                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error guardando Partida: " + ex.Message);
                return -1;
            }
        }

        // Guardar respuesta individual de una partida.
        // idOpcionElegida = null cuando el jugador no contestó
        public void GuardarRespuesta(int idPartida, int idPregunta, int? idOpcionElegida, bool esCorrecta)
        {
            try
            {
                using (var conexion = _conexionBd.ObtenerConexion())
                {
                    conexion.Open();

                    const string query = @"
                INSERT INTO RespuestasPartida (idPartida, idPregunta, idOpcionElegida, esCorrecta)
                VALUES (@idPartida, @idPregunta, @idOpcionElegida, @esCorrecta)";

                    var cmd = new MySqlCommand(query, conexion);
                    cmd.Parameters.AddWithValue("@idPartida", idPartida);
                    cmd.Parameters.AddWithValue("@idPregunta", idPregunta);
                    cmd.Parameters.AddWithValue("@idOpcionElegida", (object)idOpcionElegida ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@esCorrecta", esCorrecta);

                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error guardando Respuesta: " + ex.Message);
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
                        preguntas.Add(new Pregunta
                        {
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

public List<Tuple<string, List<string>>> ObtenerHistorialDesplegable() {
    var historial = new List<Tuple<string, List<string>>>();
    var partidasInfo = new List<Tuple<int, string>>();

    try {
        using (var conexion = _conexionBd.ObtenerConexion()) {
            conexion.Open();
            
            string q1 = "SELECT idPartida, nombreJugador, correctas, incorrectas FROM Partidas ORDER BY idPartida DESC LIMIT 10";
            using (var cmd = new MySql.Data.MySqlClient.MySqlCommand(q1, conexion))
            using (var reader = cmd.ExecuteReader()) {
                while (reader.Read()) {
                    int id = reader.GetInt32("idPartida");
                    string nombre = reader.GetString("nombreJugador");
                    int corr = reader.GetInt32("correctas");
                    int incorr = reader.GetInt32("incorrectas");
                    string titulo = $"Partida #{id} | Jugador: {nombre} | Aciertos: {corr}/{corr+incorr}";
                    partidasInfo.Add(new Tuple<int, string>(id, titulo));
                }
            }

            foreach (var p in partidasInfo) {
                var respuestas = new List<string>();
                string q2 = "SELECT pr.textoPregunta, rp.esCorrecta FROM RespuestasPartida rp JOIN Preguntas pr ON rp.idPregunta = pr.idPregunta WHERE rp.idPartida = @id";
                using (var cmd2 = new MySql.Data.MySqlClient.MySqlCommand(q2, conexion)) {
                    cmd2.Parameters.AddWithValue("@id", p.Item1);
                    using (var r2 = cmd2.ExecuteReader()) {
                        while (r2.Read()) {
                            string preg = r2.GetString("textoPregunta");
                            if (preg.Length > 55) preg = preg.Substring(0, 52) + "..."; 
                            bool corr = r2.GetBoolean("esCorrecta");
                            respuestas.Add($"{(corr ? "✅" : "❌")} {preg}");
                        }
                    }
                }
                historial.Add(new Tuple<string, List<string>>(p.Item2, respuestas));
            }
        }
    } catch (Exception ex) { Console.WriteLine("Error BD Historial: " + ex.Message); }
    
    return historial;
}
    }
}