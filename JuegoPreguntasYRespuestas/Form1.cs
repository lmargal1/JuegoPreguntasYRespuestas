using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using JuegoPreguntasYRespuestas.Data;
using MySql.Data.MySqlClient;

namespace JuegoPreguntasYRespuestas
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            JuegoDAO dao = new JuegoDAO();
            List<Categoria> categorias = dao.ObtenerCategorias();
            foreach (Categoria c in categorias)
                MessageBox.Show(c.NombreCategoria);


            /*
            //PRUEBA DE CONEXIÓN CON LA BASE DE DATOS
            ConexionBD conexion = new ConexionBD();

            try
            {
                using (MySqlConnection conn = conexion.ObtenerConexion())
                {
                    conn.Open();
                    MessageBox.Show("Conexión exitosa a la base de datos.");
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Error al conectar a la base de datos: " + ex.Message);
            }
            */
        }
    }
}
