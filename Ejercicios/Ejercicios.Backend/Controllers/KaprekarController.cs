using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ejercicios.Backend.Models;

namespace Ejercicios.Backend.Controllers
{
    /// <summary>
    /// Controlador para verificación de números de Kaprekar con diferentes algoritmos
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class KaprekarController : ControllerBase
    {
        private readonly ILogger<KaprekarController> _logger;

        public KaprekarController(ILogger<KaprekarController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Proporciona información sobre los números de Kaprekar y los métodos disponibles de cálculo
        /// </summary>
        /// <returns>Objeto con definición, ejemplos y descripción de métodos de cálculo</returns>
        [HttpGet("info")]
        public ActionResult<object> ObtenerInformacion()
        {
            _logger.LogInformation("Solicitando información sobre números de Kaprekar");

            try
            {
                var info = new
                {
                    Definicion = "Un número Kaprekar es un número que, cuando se eleva al cuadrado y se divide en dos partes, la suma de estas partes es igual al número original.",
                    Ejemplos = new[]
                    {
                        "9: 9² = 81 -> 8 + 1 = 9",
                        "45: 45² = 2025 -> 20 + 25 = 45",
                        "297: 297² = 88209 -> 88 + 209 = 297"
                    },
                    Metodos = new
                    {
                        OPTIMIZADO = "Método eficiente que prueba solo las divisiones necesarias",
                        FUERZA_BRUTA = "Método que prueba todas las combinaciones posibles (más operaciones)",
                        MATEMATICO = "Métodos basado en propiedades matemáticas de los números de Kaprekar"
                    }
                };

                _logger.LogDebug("Información de Kaprekar devuelta exitosamente");
                return Ok(info);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener información de números de Kaprekar");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Verifica si un número es un número de Kaprekar usando el método especificado
        /// </summary>
        /// <param name="request"></param>
        /// <returns>Resultado detallado de la verificación con pasos, explicación y métricas</returns>
        [HttpPost("verificar")]
        public ActionResult<KaprekarResponse> VerificarKaprekar([FromBody] KaprekarRequest request)
        {
            try
            {
                _logger.LogInformation("Iniciando verificación de número de Kaprekar");

                // Validamos que la entrada es un número válido y no vacío
                if (request == null)
                {
                    _logger.LogWarning("Solicitud de verificación de Kaprekar recibida sin datos");
                    return BadRequest("Los datos de entrada son requeridos.");
                }

                if (request.Numero < 1)
                {
                    _logger.LogWarning("Número inválido recibido: {Numero} (debe ser mayor que 0)", request.Numero);
                    return BadRequest("El número debe ser mayor que 0.");
                }

                if (request.Numero > 1000000)
                {
                    _logger.LogWarning("Número demasiado grande recibido: {Numero} (debe ser menor que 1000000)", request.Numero);
                    return BadRequest("El número debe ser menor que 1000000 para evitar cálculos muy largos.");
                }

                _logger.LogInformation("Verificando si {Numero} es Kaprekar usando método {Metodo}", 
                    request.Numero, request.MetodoCalculo);

                var resultado = KaprekarCalculator.CalcularKaprekar(request.Numero, request.MetodoCalculo);

                _logger.LogInformation("Verificación completada para {Numero}. Resultado: {EsKaprekar}, Operaciones: {Operaciones}", 
                    request.Numero, resultado.EsKaprekar, resultado.NumeroOperaciones);

                if (resultado.EsKaprekar)
                {
                    _logger.LogDebug("El número {Numero} ES Kaprekar. Descomposición: {Descomposicion}", 
                        request.Numero, resultado.Descomposicion);
                }
                else
                {
                    _logger.LogDebug("El número {Numero} NO es Kaprekar", request.Numero);
                }

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar si {Numero} es Kaprekar usando método {Metodo}", 
                    request?.Numero, request?.MetodoCalculo);
                return StatusCode(500, $"Error al verificar Kaprekar: {ex.Message}");
            }
        }

        /// <summary>
        /// Proporciona una lista de números de Kaprekar conocidos para testing
        /// </summary>
        /// <returns>Lista de números que son conocidos como números de Kaprekar</returns>
        [HttpGet("ejemplos")]
        public ActionResult<List<int>> ObtenerEjemplos()
        {
            _logger.LogInformation("Solicitando ejemplos de números de Kaprekar");

            try
            {
                var ejemplos = new List<int> { 1, 9, 45, 55, 99, 297, 703, 999 };
                
                _logger.LogDebug("Devolviendo {Cantidad} ejemplos de números de Kaprekar", ejemplos.Count);
                
                return Ok(ejemplos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ejemplos de números de Kaprekar");
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}