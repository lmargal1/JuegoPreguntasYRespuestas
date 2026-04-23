using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using JuegoPreguntasYRespuestas.Modelo;

namespace JuegoPreguntasYRespuestas
{
    internal static class JuegoServicio
    {
        private static string tipoTexto = "texto";
        private static string tipoImagen = "imagen";
        private static List<Pregunta> preguntasJuego = new List<Pregunta>();
        private static List<Respuesta> respuestas = new List<Respuesta>();
        public static int preguntaActual { get; set; }
        public static int correctas { get; set; }
        public static int incorrectas { get; set; }
        public static int totalPreguntas { get; set; }

        public static void iniciaJuego(List<Pregunta> preguntas)
        {
            preguntasJuego = desordenarPreguntas(preguntas);
            preguntaActual = 0;
            correctas = 0;
            incorrectas = 0;
            totalPreguntas = preguntasJuego.Count;
            respuestas = new List<Respuesta>();
        }

        public static Pregunta obtenerPreguntaActual()
        {
            return preguntasJuego[preguntaActual];
        }

        public static bool siguientePregunta()
        {
            preguntaActual++;
            return preguntaActual < preguntasJuego.Count;
        }

        public static void registraRespuesta(int idPregunta, bool esCorrecta)
        {
            if (esCorrecta == true)
                correctas++;
            else
                incorrectas++;

            respuestas.Add(new Respuesta
            {
                IDPregunta = idPregunta,
                EsCorrecta = esCorrecta
            });
        }

        public static List<Respuesta> obtenerRespuestas()
        {
            return respuestas;
        }

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

        public static bool validaRespuesta(int idSeleccionada, List<Opcion> opciones)
        {
            bool esCorrecta = false;
            int idPregunta = 0;

            foreach (Opcion opcion in opciones)
            {
                if(opcion.IdOpcion == idSeleccionada)
                {
                    esCorrecta = opcion.EsCorrecta;
                    idPregunta = opcion.IdPregunta;
                    break;
                }
            }

            registraRespuesta(idPregunta, esCorrecta);
            return esCorrecta;
        }

        public static int calcularPorcentajeCorrecto()
        {
            int res=0;
            if (totalPreguntas == 0)
                return res;

            res=correctas * 100 / totalPreguntas;

            return res;
        }
        public static int calcularPorcentajeIncorrecto()
        {
            int res = 0;
            if (totalPreguntas == 0)
                return res;

            res = incorrectas * 100 / totalPreguntas;

            return res;
        }
    }
}