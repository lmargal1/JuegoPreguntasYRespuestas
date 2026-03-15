using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JuegoPreguntasYRespuestas
{
    internal class Pregunta
    {
        public int IdPregunta { get; set; }
        public string TextoPregunta { get; set; }
        public string Tipo {  get; set; }
        public int IdCategoria { get; set; }
    }
}
