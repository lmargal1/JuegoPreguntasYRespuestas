using MySql.Data.MySqlClient;

namespace JuegoPreguntasYRespuestas.DAO 
{
    /// <summary>
    /// Gestiona la conexión a la base de datos MySQL.
    /// Cambia los datos aquí si tu contraseña de root es distinta.
    /// </summary>
    public class ConexionBD
    {
        // Cambia "root" si en otra computadora la contraseña es diferente.
        private const string CadenaConexion = "Server = localhost; Database = juegoDB; User ID = root; Password = root;";

        public MySqlConnection ObtenerConexion()
        {
            return new MySqlConnection(CadenaConexion);
        }
    }
}