using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using Ejercicios.Backend.Models;

namespace Ejercicios.Backend.Controllers
{
    /// <summary>
    /// Controlador para operaciones con fechas y cálculos relacionados
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class FechaController : ControllerBase
    {
        /// <summary>
        /// Procesa dos fechas y calcula diversas operaciones y propiedades relacionadas
        /// </summary>
        /// <param name="request"></param>
        /// <returns>Resultado con diferencias, propiedades de años, números de semana y días de la semana</returns>
        [HttpPost("procesar")]
        public ActionResult<FechaResult> ProcesarFechas([FromBody] FechaRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Los datos de entrada son requeridos.");
                }

                // Validamos las fechas
                if (!ValidarYConvertirFechas(request, out DateTime fecha1, out DateTime fecha2, out string mensajeError))
                {
                    return BadRequest(mensajeError);
                }

                var result = new FechaResult();

                // Asignar fechas validadas
                result.Fecha1 = fecha1;
                result.Fecha2 = fecha2;

                // 1. Calcular diferencia de dias
                TimeSpan diferencia = fecha1 - fecha2;
                result.DiferenciaDias = (int)diferencia.TotalDays;

                // 2. Calcular inicio y fin de año de cada fecha
                result.InicioAno1 = new DateTime(fecha1.Year, 1, 1);
                result.FinAno1 = new DateTime(fecha1.Year, 12, 31);
                result.InicioAno2 = new DateTime(fecha2.Year, 1, 1);
                result.FinAno2 = new DateTime(fecha2.Year, 12, 31);

                // 3. Calcular numero de dias del año
                result.DiasDelAno1 = DateTime.IsLeapYear(fecha1.Year) ? 366 : 365;
                result.DiasDelAno2 = DateTime.IsLeapYear(fecha2.Year) ? 366 : 365;

                // 4. Calcular numero de semana del año
                result.NumeroSemana1 = ObtenerNumeroSemana(fecha1);
                result.NumeroSemana2 = ObtenerNumeroSemana(fecha2);

                // 5. Obtener dias de la semana
                result.DiaSemana1 = fecha1.ToString("dddd", new CultureInfo("es-ES"));
                result.DiaSemana2 = fecha2.ToString("dddd", new CultureInfo("es-ES"));

                // 6. Determinar si es año bisiesto o no
                result.EsBisiesto1 = DateTime.IsLeapYear(fecha1.Year);
                result.EsBisiesto2 = DateTime.IsLeapYear(fecha2.Year);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al procesar las fechas: {ex.Message}");
            }
        }

        /// <summary>
        /// Valida que las fechas no sean vacías y tengan el formato yyyy/MM/dd
        /// </summary>
        /// <param name="request"></param>
        /// <param name="fecha1"></param>
        /// <param name="fecha2"></param>
        /// <param name="mensajeError"></param>
        /// <returns>True si ambas fechas son validas, False si hay errores</returns>
        private bool ValidarYConvertirFechas(FechaRequest request, out DateTime fecha1, out DateTime fecha2, out string mensajeError)
        {
            fecha1 = default;
            fecha2 = default;
            mensajeError = "";

            if (string.IsNullOrWhiteSpace(request.Fecha1Text) || string.IsNullOrWhiteSpace(request.Fecha2Text))
            {
                mensajeError = "Por favor, introduzca ambas fechas.";
                return false;
            }

            if (!DateTime.TryParseExact(request.Fecha1Text, "yyyy/MM/dd", null, DateTimeStyles.None, out fecha1))
            {
                mensajeError = $"La primera fecha '{request.Fecha1Text}' no tiene el formato correcto. Usa yyyy/MM/dd";
                return false;
            }

            if (!DateTime.TryParseExact(request.Fecha2Text, "yyyy/MM/dd", null, DateTimeStyles.None, out fecha2))
            {
                mensajeError = $"La segunda fecha '{request.Fecha2Text}' no tiene el formato correcto. Usa yyyy/MM/dd";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Obtiene el número de la semana del año basado en la cultura ISO 8601
        /// </summary>
        /// <param name="fecha"></param>
        /// <returns>Número de semana del año (1-53)</returns>
        private int ObtenerNumeroSemana(DateTime fecha)
        {
            // Usamos el calendario ISO 8601
            Calendar calendario = CultureInfo.InvariantCulture.Calendar;
            CalendarWeekRule reglas = CalendarWeekRule.FirstFourDayWeek;
            DayOfWeek primerDiaSemana = DayOfWeek.Monday;

            return calendario.GetWeekOfYear(fecha, reglas, primerDiaSemana);
        }
    }
}