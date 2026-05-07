namespace JuegoServidor.Modelo
{
    public class Opcion
    {
        public int IdOpcion { get; set; }
        public int IdPregunta { get; set; }
        public string TextoOpcion { get; set; }
        public string RutaImagen { get; set; }
        public bool EsCorrecta { get; set; }
    }
}
