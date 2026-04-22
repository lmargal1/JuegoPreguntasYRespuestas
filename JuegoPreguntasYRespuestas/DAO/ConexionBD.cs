using MySql.Data.MySqlClient;

namespace JuegoPreguntasYRespuestas.DAO 
{
    public class ConexionBD
    {

        private const string CadenaConexion = "Server = localhost; Database = juegoDB; User ID = root; Password = root;";

        public MySqlConnection ObtenerConexion()
        {
            return new MySqlConnection(CadenaConexion);
        }
    }
}