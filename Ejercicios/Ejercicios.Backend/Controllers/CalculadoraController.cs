using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ejercicios.Backend.Models;

namespace Ejercicios.Backend.Controllers
{
    /// <summary>
    /// Controlador para operaciones matemáticas
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CalculadoraController : ControllerBase
    {
        private readonly ILogger<CalculadoraController> _logger;

        public CalculadoraController(ILogger<CalculadoraController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Realiza operaciones matemáticas (suma, resta, multiplicación, división, módulo y comparación) con dos números
        /// </summary>
        /// <param name="request"></param>
        /// <returns>Resultado de todas las operaciones matemáticas</returns>
        [HttpPost("calcular")]
        public ActionResult<OperacionResult> CalcularOperaciones([FromBody] OperacionRequest request)
        {
            try
            {
                _logger.LogInformation("Iniciando cálculo de operaciones matemáticas");

                if (request == null)
                {
                    _logger.LogWarning("Solicitud de cálculo recibida sin datos");
                    return BadRequest("Los datos de entrada son requeridos");
                }

                _logger.LogInformation("Calculando operaciones matemáticas: {Cantidad1} y {Cantidad2} con {NumDecimales} decimales", 
                    request.Cantidad1, request.Cantidad2, request.NumDecimales);

                // Validar número de decimales
                if (request.NumDecimales < 0 || request.NumDecimales > 10)
                {
                    _logger.LogWarning("Número de decimales inválido: {NumDecimales} (debe estar entre 0 y 10)", request.NumDecimales);
                    return BadRequest("El número de decimales debe estar entre 0 y 10");
                }

                // Realizar las operaciones
                double resultadoSuma = request.Cantidad1 + request.Cantidad2;
                double resultadoResta = request.Cantidad1 - request.Cantidad2;
                double resultadoMultiplicacion = request.Cantidad1 * request.Cantidad2;
                double resultadoDivision = request.Cantidad2 != 0 ? request.Cantidad1 / request.Cantidad2 : 0;
                double resultadoModulo = request.Cantidad2 != 0 ? request.Cantidad1 % request.Cantidad2 : 0;

                _logger.LogDebug("Operaciones calculadas - Suma: {Suma}, Resta: {Resta}, Multiplicación: {Multiplicacion}", 
                    resultadoSuma, resultadoResta, resultadoMultiplicacion);

                // Aplicar redondeo según el número de decimales
                var result = new OperacionResult
                {
                    Suma = Math.Round(resultadoSuma, request.NumDecimales).ToString($"F{request.NumDecimales}"),
                    Resta = Math.Round(resultadoResta, request.NumDecimales).ToString($"F{request.NumDecimales}"),
                    Multiplicacion = Math.Round(resultadoMultiplicacion, request.NumDecimales).ToString($"F{request.NumDecimales}")
                };

                // Manejar división por cero
                if (request.Cantidad2 != 0)
                {
                    result.Division = Math.Round(resultadoDivision, request.NumDecimales).ToString($"F{request.NumDecimales}");
                    result.Modulo = Math.Round(resultadoModulo, request.NumDecimales).ToString($"F{request.NumDecimales}");
                    
                    _logger.LogDebug("División y módulo calculados - División: {Division}, Módulo: {Modulo}", 
                        resultadoDivision, resultadoModulo);
                }
                else
                {
                    result.Division = "Error: División por cero";
                    result.Modulo = "Error: División por cero";
                    
                    _logger.LogWarning("Intento de división por cero detectado con Cantidad1: {Cantidad1} y Cantidad2: {Cantidad2}", 
                        request.Cantidad1, request.Cantidad2);
                }

                // Comparar los números
                if (request.Cantidad1 < request.Cantidad2)
                {
                    result.Comparacion = "es menor que";
                }
                else if (request.Cantidad1 > request.Cantidad2)
                {
                    result.Comparacion = "es mayor que";
                }
                else
                {
                    result.Comparacion = "es igual a";
                }

                _logger.LogInformation("Cálculo de operaciones completado exitosamente. Comparación: {Cantidad1} {Comparacion} {Cantidad2}", 
                    request.Cantidad1, result.Comparacion, request.Cantidad2);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al realizar operaciones matemáticas con Cantidad1: {Cantidad1}, Cantidad2: {Cantidad2}", 
                    request?.Cantidad1, request?.Cantidad2);

                return StatusCode(500, new OperacionResult
                {
                    Suma = $"Error: {ex.Message}",
                    Resta = $"Error: {ex.Message}",
                    Multiplicacion = $"Error: {ex.Message}",
                    Division = $"Error: {ex.Message}",
                    Modulo = $"Error: {ex.Message}",
                    Comparacion = "Error en comparación"
                });
            }
        }
    }
}