using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JuegoPreguntasYRespuestas
{
    internal static class JuegoServicio
    {
        private static string tipoTexto = "texto";
        private static string tipoImagen = "imagen";
        public static List<Pregunta> desordenarPreguntas(List<Pregunta> preguntas)
        {
            if (preguntas == null)
                return preguntas;

            List<Pregunta> lista = new List<Pregunta>(preguntas);
            Random rng = new Random();

            for (int i=lista.Count-1; i>0;i--)
            {
                int j = rng.Next(0, i+1);
                Pregunta temp = lista[i];
                lista[i] = lista[j];
                lista[j] = temp;
            }

            return lista;
        }

        public static string esTipo(Pregunta pregunta)
        {
            if (pregunta.Tipo == tipoTexto) 
                return tipoTexto;
            return tipoImagen;
        }
    }
}
