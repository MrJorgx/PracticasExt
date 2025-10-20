using Microsoft.AspNetCore.Mvc;

namespace Ejercicios.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CalculadoraController : ControllerBase
    {
        [HttpPost("calcular")]
        public ActionResult<OperacionResult> CalcularOperaciones([FromBody] OperacionRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Los datos de entrada son requeridos");
                }

                // Realizar las operaciones
                double resultadoSuma = request.Cantidad1 + request.Cantidad2;
                double resultadoResta = request.Cantidad1 - request.Cantidad2;
                double resultadoMultiplicacion = request.Cantidad1 * request.Cantidad2;
                double resultadoDivision = request.Cantidad2 != 0 ? request.Cantidad1 / request.Cantidad2 : 0;
                double resultadoModulo = request.Cantidad2 != 0 ? request.Cantidad1 % request.Cantidad2 : 0;

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
                }
                else
                {
                    result.Division = "Error: División por cero";
                    result.Modulo = "Error: División por cero";
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

                return Ok(result);
            }
            catch (Exception ex)
            {
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

    public class OperacionRequest
    {
        public double Cantidad1 { get; set; }
        public double Cantidad2 { get; set; }
        public int NumDecimales { get; set; }
    }

    public class OperacionResult
    {
        public string Suma { get; set; } = "";
        public string Resta { get; set; } = "";
        public string Multiplicacion { get; set; } = "";
        public string Division { get; set; } = "";
        public string Modulo { get; set; } = "";
        public string Comparacion { get; set; } = "";
    }
}