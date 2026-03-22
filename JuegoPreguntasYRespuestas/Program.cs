using System;
using System.Windows.Forms;
// ESTA ES LA LÍNEA CLAVE QUE FALTA:
using JuegoPreguntasYRespuestas.Presentacion; 

namespace JuegoPreguntasYRespuestas
{
    static class Program
    {
        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            // Ahora sí encontrará Form1 gracias al using de arriba
            Application.Run(new Form1()); 
        }
    }
}