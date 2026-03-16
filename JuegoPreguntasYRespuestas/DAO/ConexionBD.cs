using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MySql.Data.MySqlClient;

namespace JuegoPreguntasYRespuestas.Data
{
    public class ConexionBD
    {
        private string connectionString = "Server = localhost; Database = juegoDB; User ID = root; Password = Bach.1638;";

        public MySqlConnection ObtenerConexion()
        {
            return new MySqlConnection(connectionString);
        }
    }
}
