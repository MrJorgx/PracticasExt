using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<FechaController> _logger;

        public FechaController(ILogger<FechaController> logger)
        {
            _logger = logger;
        }

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
                _logger.LogInformation("Iniciando procesamiento de fechas");

                if (request == null)
                {
                    _logger.LogWarning("Solicitud de procesamiento de fechas recibida sin datos");
                    return BadRequest("Los datos de entrada son requeridos.");
                }

                _logger.LogInformation("Procesando fechas: Fecha1='{Fecha1Text}', Fecha2='{Fecha2Text}'", 
                    request.Fecha1Text, request.Fecha2Text);

                // Validamos las fechas
                if (!ValidarYConvertirFechas(request, out DateTime fecha1, out DateTime fecha2, out string mensajeError))
                {
                    _logger.LogWarning("Validación de fechas fallida: {MensajeError}", mensajeError);
                    return BadRequest(mensajeError);
                }

                _logger.LogDebug("Fechas validadas correctamente: {Fecha1} y {Fecha2}", fecha1, fecha2);

                var result = new FechaResult();

                // Asignar fechas validadas
                result.Fecha1 = fecha1;
                result.Fecha2 = fecha2;

                // 1. Calcular diferencia de dias
                TimeSpan diferencia = fecha1 - fecha2;
                result.DiferenciaDias = (int)diferencia.TotalDays;
                _logger.LogDebug("Diferencia de días calculada: {DiferenciaDias} días", result.DiferenciaDias);

                // 2. Calcular inicio y fin de año de cada fecha
                result.InicioAno1 = new DateTime(fecha1.Year, 1, 1);
                result.FinAno1 = new DateTime(fecha1.Year, 12, 31);
                result.InicioAno2 = new DateTime(fecha2.Year, 1, 1);
                result.FinAno2 = new DateTime(fecha2.Year, 12, 31);
                _logger.LogDebug("Rangos de años calculados: Año1={Ano1} ({InicioAno1} - {FinAno1}), Año2={Ano2} ({InicioAno2} - {FinAno2})", 
                    fecha1.Year, result.InicioAno1, result.FinAno1, fecha2.Year, result.InicioAno2, result.FinAno2);

                // 3. Calcular numero de dias del año
                result.DiasDelAno1 = DateTime.IsLeapYear(fecha1.Year) ? 366 : 365;
                result.DiasDelAno2 = DateTime.IsLeapYear(fecha2.Year) ? 366 : 365;
                _logger.LogDebug("Días del año calculados: Año {Ano1}={DiasAno1} días (bisiesto: {EsBisiesto1}), Año {Ano2}={DiasAno2} días (bisiesto: {EsBisiesto2})", 
                    fecha1.Year, result.DiasDelAno1, DateTime.IsLeapYear(fecha1.Year),
                    fecha2.Year, result.DiasDelAno2, DateTime.IsLeapYear(fecha2.Year));

                // 4. Calcular numero de semana del año
                result.NumeroSemana1 = ObtenerNumeroSemana(fecha1);
                result.NumeroSemana2 = ObtenerNumeroSemana(fecha2);
                _logger.LogDebug("Números de semana calculados: Fecha1=semana {Semana1}, Fecha2=semana {Semana2}", 
                    result.NumeroSemana1, result.NumeroSemana2);

                // 5. Obtener dias de la semana
                result.DiaSemana1 = fecha1.ToString("dddd", new CultureInfo("es-ES"));
                result.DiaSemana2 = fecha2.ToString("dddd", new CultureInfo("es-ES"));
                _logger.LogDebug("Días de la semana: Fecha1={DiaSemana1}, Fecha2={DiaSemana2}", 
                    result.DiaSemana1, result.DiaSemana2);

                // 6. Determinar si es año bisiesto o no
                result.EsBisiesto1 = DateTime.IsLeapYear(fecha1.Year);
                result.EsBisiesto2 = DateTime.IsLeapYear(fecha2.Year);

                _logger.LogInformation("Procesamiento de fechas completado exitosamente. Diferencia: {DiferenciaDias} días, Semanas: {Semana1}/{Semana2}, Días semana: {DiaSemana1}/{DiaSemana2}", 
                    result.DiferenciaDias, result.NumeroSemana1, result.NumeroSemana2, result.DiaSemana1, result.DiaSemana2);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar las fechas: Fecha1='{Fecha1Text}', Fecha2='{Fecha2Text}'", 
                    request?.Fecha1Text, request?.Fecha2Text);
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
            _logger.LogDebug("Iniciando validación de fechas");

            fecha1 = default;
            fecha2 = default;
            mensajeError = "";

            if (string.IsNullOrWhiteSpace(request.Fecha1Text) || string.IsNullOrWhiteSpace(request.Fecha2Text))
            {
                mensajeError = "Por favor, introduzca ambas fechas.";
                _logger.LogWarning("Validación fallida: una o ambas fechas están vacías. Fecha1Text='{Fecha1Text}', Fecha2Text='{Fecha2Text}'", 
                    request.Fecha1Text, request.Fecha2Text);
                return false;
            }

            if (!DateTime.TryParseExact(request.Fecha1Text, "yyyy/MM/dd", null, DateTimeStyles.None, out fecha1))
            {
                mensajeError = $"La primera fecha '{request.Fecha1Text}' no tiene el formato correcto. Usa yyyy/MM/dd";
                _logger.LogWarning("Validación fallida: formato incorrecto en primera fecha - '{Fecha1Text}'", request.Fecha1Text);
                return false;
            }

            if (!DateTime.TryParseExact(request.Fecha2Text, "yyyy/MM/dd", null, DateTimeStyles.None, out fecha2))
            {
                mensajeError = $"La segunda fecha '{request.Fecha2Text}' no tiene el formato correcto. Usa yyyy/MM/dd";
                _logger.LogWarning("Validación fallida: formato incorrecto en segunda fecha - '{Fecha2Text}'", request.Fecha2Text);
                return false;
            }

            _logger.LogDebug("Validación completada exitosamente: {Fecha1} y {Fecha2}", fecha1, fecha2);
            return true;
        }

        /// <summary>
        /// Obtiene el número de la semana del año basado en la cultura ISO 8601
        /// </summary>
        /// <param name="fecha"></param>
        /// <returns>Número de semana del año (1-53)</returns>
        private int ObtenerNumeroSemana(DateTime fecha)
        {
            _logger.LogTrace("Calculando número de semana para fecha {Fecha}", fecha);

            // Usamos el calendario ISO 8601
            Calendar calendario = CultureInfo.InvariantCulture.Calendar;
            CalendarWeekRule reglas = CalendarWeekRule.FirstFourDayWeek;
            DayOfWeek primerDiaSemana = DayOfWeek.Monday;

            int numeroSemana = calendario.GetWeekOfYear(fecha, reglas, primerDiaSemana);
            
            _logger.LogTrace("Número de semana calculado: {NumeroSemana} para fecha {Fecha}", numeroSemana, fecha);
            
            return numeroSemana;
        }
    }
}