namespace JuegoServidor.Modelo
{
    public class Pregunta
    {
        public int IdPregunta { get; set; }
        public string TextoPregunta { get; set; }
        public string Tipo {  get; set; }
        public int IdCategoria { get; set; }
    }
}
