namespace Ejercicios.Backend.Models
{
    /// <summary>
    /// Modelo de entrada para operaciones de procesamiento de texto
    /// </summary>
    public class TextoRequest
    {
        /// <summary>
        /// Texto a procesar
        /// </summary>
        public string Texto { get; set; } = "";
    }

    /// <summary>
    /// Modelo de respuesta con todos los resultados del procesamiento del texto
    /// </summary>
    public class TextoResult
    {
        /// <summary>
        /// Número total de caracteres en el texto original
        /// </summary>
        public int NumeroCaracteres { get; set; }

        /// <summary>
        /// Texto convertido a mayúsculas
        /// </summary>
        public string TextoMayusculas { get; set; } = "";

        /// <summary>
        /// Texto convertido a minúsculas
        /// </summary>
        public string TextoMinusculas { get; set; } = "";

        /// <summary>
        /// Diccionario con palabras que aparecen más de una vez y su número de ocurrencias
        /// </summary>
        public Dictionary<string, int> PalabrasRepetidas { get; set; } = new Dictionary<string, int>();

        /// <summary>
        /// Texto con todas las ocurrencias de "Proconsi" reemplazadas por "Isnocorp"
        /// </summary>
        public string TextoReemplazado { get; set; } = "";

        /// <summary>
        /// Tiempo en milisegundos que tardó en concatenar el texto 1000 veces
        /// </summary>
        public double TiempoConcatenacion { get; set; }

        /// <summary>
        /// Longitud del texto después de concatenarlo 1000 veces
        /// </summary>
        public int LongitudTextoConcatenado { get; set; }
    }
}