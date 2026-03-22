using System;
using System.Windows.Forms;
using JuegoPreguntasYRespuestas.Presentacion; 

namespace JuegoPreguntasYRespuestas
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1()); 
        }
    }
}