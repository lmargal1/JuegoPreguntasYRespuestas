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

        public void GuardarPartida(int idCat, int corr, int incorr)
        {
            try 
            {
                using (var conexion = _conexionBd.ObtenerConexion())
                {
                    conexion.Open();
                    
                    const string query = "INSERT INTO Partidas (idCategoria, correctas, incorrectas) VALUES (@id, @c, @i)";
                    var cmd = new MySqlCommand(query, conexion);
                    
                    cmd.Parameters.AddWithValue("@id", idCat == 0 ? (object)DBNull.Value : idCat);
                    cmd.Parameters.AddWithValue("@c", corr);
                    cmd.Parameters.AddWithValue("@i", incorr);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex) { Console.WriteLine(@"Error DB al guardar: " + ex.Message); }
        }

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