using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;
using Ejercicios.Backend.Models;

namespace Ejercicios.Backend.Controllers
{
    /// <summary>
    /// Controlador para operaciones de procesamiento de texto
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class TextoController : ControllerBase
    {
        private readonly ILogger<TextoController> _logger;

        public TextoController(ILogger<TextoController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Procesa un texto realizando diversas transformaciones y análisis
        /// </summary>
        /// <param name="request"></param>
        /// <returns>Resultado con las transformaciones, análisis de palabras repetidas y métricas de tiempo</returns>
        [HttpPost("procesar")]
        public ActionResult<TextoResult> ProcesarTexto([FromBody] TextoRequest request)
        {
            try
            {
                _logger.LogInformation("Iniciando procesamiento de texto");

                if (request == null || string.IsNullOrWhiteSpace(request.Texto))
                {
                    _logger.LogWarning("Solicitud de procesamiento de texto recibida sin datos o texto vacío");
                    return BadRequest("Se requiere texto de entrada.");
                }

                _logger.LogInformation("Procesando texto de {Longitud} caracteres", request.Texto.Length);

                var result = new TextoResult();

                // 1. Numero caracteres
                result.NumeroCaracteres = request.Texto.Length;
                _logger.LogDebug("Número de caracteres calculado: {NumeroCaracteres}", result.NumeroCaracteres);

                // 2. Texto en mayusculas
                result.TextoMayusculas = request.Texto.ToUpper();
                _logger.LogDebug("Conversión a mayúsculas completada");

                // 3. Texto en minusculas
                result.TextoMinusculas = request.Texto.ToLower();
                _logger.LogDebug("Conversión a minúsculas completada");

                // 4. Encontrar palabras repetidas
                result.PalabrasRepetidas = EncontrarPalabrasRepetidas(request.Texto);
                _logger.LogDebug("Análisis de palabras repetidas completado. Encontradas {CantidadRepetidas} palabras repetidas", 
                    result.PalabrasRepetidas.Count);

                // 5. Reemplazar "Proconsi" por "Isnocorp"
                result.TextoReemplazado = request.Texto.Replace("Proconsi", "Isnocorp");
                var textoOriginal = request.Texto;
                var ocurrenciasProconsi = 0;
                var indice = 0;
                while ((indice = textoOriginal.IndexOf("Proconsi", indice)) != -1)
                {
                    ocurrenciasProconsi++;
                    indice += "Proconsi".Length;
                }
                
                if (ocurrenciasProconsi > 0)
                {
                    _logger.LogDebug("Reemplazo completado: {Ocurrencias} ocurrencias de 'Proconsi' reemplazadas por 'Isnocorp'", 
                        ocurrenciasProconsi);
                }
                else
                {
                    _logger.LogDebug("No se encontraron ocurrencias de 'Proconsi' en el texto");
                }

                // 6. Concatenar 1000 veces y medir el tiempo
                _logger.LogDebug("Iniciando concatenación de texto 1000 veces");
                var (tiempo, longitud) = ConcatenarTexto1000Veces(request.Texto);
                result.TiempoConcatenacion = tiempo;
                result.LongitudTextoConcatenado = longitud;
                _logger.LogDebug("Concatenación completada en {Tiempo}ms. Longitud final: {Longitud} caracteres", 
                    tiempo, longitud);

                _logger.LogInformation("Procesamiento de texto completado exitosamente. Caracteres: {Caracteres}, Palabras repetidas: {PalabrasRepetidas}, Tiempo concatenación: {Tiempo}ms", 
                    result.NumeroCaracteres, result.PalabrasRepetidas.Count, tiempo);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar texto de longitud {Longitud}", 
                    request?.Texto?.Length ?? 0);

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

        /// <summary>
        /// Encuentra palabras repetidas más de una vez en el texto
        /// </summary>
        /// <param name="texto"></param>
        /// <returns>Diccionario con las palabras repetidas y número de ocurrencias</returns>
        private Dictionary<string, int> EncontrarPalabrasRepetidas(string texto)
        {
            _logger.LogDebug("Iniciando búsqueda de palabras repetidas");

            var palabrasRepetidas = new Dictionary<string, int>();

            // Separar el texto en palabras, eliminar signos y convertir
            char[] separadores = { ' ', '.', ',', ';', ':', '!', '?', '\n', '\r', '\t', '(', ')', '[', ']', '{', '}', '"', '\'' };
            string[] palabras = texto.ToLower().Split(separadores, StringSplitOptions.RemoveEmptyEntries);

            _logger.LogDebug("Texto dividido en {CantidadPalabras} palabras", palabras.Length);

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

            _logger.LogDebug("Análisis de palabras completado. Palabras únicas: {PalabrasUnicas}, Palabras repetidas: {PalabrasRepetidas}", 
                contadorPalabras.Count, palabrasRepetidas.Count);

            return palabrasRepetidas;
        }

        /// <summary>
        /// Concatena el texto 1000 veces y mide el tiempo de ejecución
        /// </summary>
        /// <param name="texto"></param>
        /// <returns>Tupla con el tiempo en milisegundos y longitud del texto concatenado</returns>
        private (double tiempo, int longitud) ConcatenarTexto1000Veces(string texto)
        {
            _logger.LogDebug("Iniciando concatenación de texto 1000 veces usando StringBuilder");

            // Medir tiempo de concatenacion
            Stopwatch stopwatch = Stopwatch.StartNew();

            // Usar StringBuilder para la concatenacion
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < 1000; i++)
            {
                sb.Append(texto);
            }

            stopwatch.Stop();

            var tiempoTotal = stopwatch.Elapsed.TotalMilliseconds;
            var longitudFinal = sb.Length;

            _logger.LogDebug("Concatenación completada. Tiempo: {Tiempo}ms, Longitud original: {LongitudOriginal}, Longitud final: {LongitudFinal}", 
                tiempoTotal, texto.Length, longitudFinal);

            return (tiempoTotal, longitudFinal);
        }
    }
}