using Microsoft.AspNetCore.Mvc;
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
                // Validamos que la entrada es un número válido y no vacío
                if (request == null)
                {
                    return BadRequest("Los datos de entrada son requeridos.");
                }

                if (request.Numero < 1)
                {
                    return BadRequest("El número debe ser mayor que 0.");
                }

                if (request.Numero > 1000000)
                {
                    return BadRequest("El número debe ser menor que 1000000 para evitar cáculos muy largos.");
                }

                Console.WriteLine($"Verificando si {request.Numero} es Kaprekar");

                var resultado = KaprekarCalculator.CalcularKaprekar(request.Numero, request.MetodoCalculo);

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al verificar Kaprekar: {ex.Message}");
                return StatusCode(500, $"Error al verificar Kaprekar: {ex.Message}");
            }
        }

        /// <summary>
        /// Proporciona una lista de números de Kaprekar conocidos para testing
        /// </summary>
        /// <returns>Lista de número que son conocidos como números de Kaprekar</returns>
        [HttpGet("ejemplos")]
        public ActionResult<List<int>> ObtenerEjemplos()
        {
            var ejemplos = new List<int> { 1, 9, 45, 55, 99, 297, 703, 999};
            return Ok(ejemplos);
        }

        /// <summary>
        /// Proporciona información sobre los números de Kaprekar y los métodos disponibles de cálculo
        /// </summary>
        /// <returns>Objeto con definición, ejemplos y descripción de métodos de cálculo</returns>
        [HttpGet("info")]
        public ActionResult<object> ObtenerInformacion()
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
                    MATEMATICO = "Métodos basado en propiedas matemáticas de los números de Kaprekar"
                }
            };

            return Ok(info);
        }
    }
}