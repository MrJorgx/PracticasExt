using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text;

namespace Ejercicios.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class TextoController : ControllerBase
    {
        [HttpPost("procesar")]

        public ActionResult<TextoResult> ProcesarTexto([FromBody] TextoRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Texto))
                {
                    return BadRequest("Se requiere texto de entrada.");
                }

                var result = new TextoResult();

                // 1. Numero caracteres
                result.NumeroCaracteres = request.Texto.Length;

                // 2. Texto en mayusculas
                result.TextoMayusculas = request.Texto.ToUpper();

                // 3. Texto en minusculas
                result.TextoMinusculas = request.Texto.ToLower();

                // 4. Encontrar palabras repetidas
                result.PalabrasRepetidas = EncontrarPalabrasRepetidas(request.Texto);

                // 5. Reemplazar "Proconsi" por "Isnocorp"
                result.TextoReemplazado = request.Texto.Replace("Proncosi", "Isnocorp");

                // 6. Concatenar 1000 veces y medir el tiempo
                var(tiempo, longitud) = ConcatenarTexto1000Veces(request.Texto);
                result.TiempoConcatenacion = tiempo;
                result.LongitudTextoConcatenado = longitud;

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new TextoResult
                {
                    TextoMayusculas = $"Error: {ex.Message}",
                    TextoMinusculas = $"Error: {ex.Message}",
                    TextoReemplazado = $"Error: {ex.Message}",
                    NumeroCaracteres = 0,
                    PalabrasRepetidas = new Dictionary<string, int>(),
                    TiempoConcatenacion = 0,
                    LongitudTextoConcatenado = 0
                });
            }
        }

        private Dictionary<string, int> EncontrarPalabrasRepetidas(string texto)
        {
            var palabrasRepetidas = new Dictionary<string, int>();

            // Separar el texto en palabras, eliminar signos y convertir
            char[] separadores = { ' ', '.', ',', ';', ':', '!', '?', '\n', '\r', '\t', '(', ')', '[', ']', '{', '}', '"', '\'' };
            string[] palabras = texto.ToLower().Split(separadores, StringSplitOptions.RemoveEmptyEntries);

            // Contar ocurrencias de cada palabra
            var contadorPalabras = new Dictionary<string, int>();

            foreach (string palabra in palabras)
            {
                if (!string.IsNullOrWhiteSpace(palabra))
                {
                    if (contadorPalabras.ContainsKey(palabra))
                    {
                        contadorPalabras[palabra]++;
                    }
                    else
                    {
                        contadorPalabras[palabra] = 1;
                    }
                }
            }

            // Filtrar solo palabras repetidas
            foreach (var kvp in contadorPalabras)
            {
                if (kvp.Value > 1)
                {
                    palabrasRepetidas[kvp.Key] = kvp.Value;
                }
            }

            return palabrasRepetidas;
        }

        private (double tiempo, int longitud) ConcatenarTexto1000Veces(string texto)
        {
            // Medir tiempo de concatenacion
            Stopwatch stopwatch = Stopwatch.StartNew();

            // Usar StringBuilder para la concatenacion
            StringBuilder sb = new StringBuilder();

            for (int i=0; i<1000; i++)
            {
                sb.Append(texto);
            }

            stopwatch.Stop();

            return (stopwatch.Elapsed.TotalMilliseconds, sb.Length);
        }
    }

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