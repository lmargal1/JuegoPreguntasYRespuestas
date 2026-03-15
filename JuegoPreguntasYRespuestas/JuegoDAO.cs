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
    }
}
