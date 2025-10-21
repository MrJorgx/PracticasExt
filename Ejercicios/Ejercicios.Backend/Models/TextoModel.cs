namespace Ejercicios.Backend.Models
{
    public class TextoRequest
    {
        public string Texto { get; set; } = "";
    }

    public class TextoResult
    {
        public int NumeroCaracteres { get; set; }
        public string TextoMayusculas { get; set; } = "";
        public string TextoMinusculas { get; set; } = "";
        public Dictionary<string, int> PalabrasRepetidas { get; set; } = new Dictionary<string, int>();
        public string TextoReemplazado { get; set; } = "";
        public double TiempoConcatenacion { get; set; }
        public int LongitudTextoConcatenado { get; set; }
    }
}