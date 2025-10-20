using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace Ejercicios.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FechaController : ControllerBase
    {
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

        private int ObtenerNumeroSemana(DateTime fecha)
        {
            // Usamos el calendario ISO 8601
            Calendar calendario = CultureInfo.InvariantCulture.Calendar;
            CalendarWeekRule reglas = CalendarWeekRule.FirstFourDayWeek;
            DayOfWeek primerDiaSemana = DayOfWeek.Monday;

            return calendario.GetWeekOfYear(fecha, reglas, primerDiaSemana);
        }
    }

    public class FechaRequest
    {
        public string Fecha1Text { get; set; } = "";
        public string Fecha2Text { get; set; } = "";
    }

    public class FechaResult
    {
        public DateTime Fecha1 { get; set; }
        public DateTime Fecha2 { get; set; }
        public int DiferenciaDias { get; set; }
        public DateTime InicioAno1 { get; set; }
        public DateTime FinAno1 { get; set; }
        public DateTime InicioAno2 { get; set; }
        public DateTime FinAno2 { get; set; }
        public int DiasDelAno1 { get; set; }
        public int DiasDelAno2 { get; set; }
        public int NumeroSemana1 { get; set; }
        public int NumeroSemana2 { get; set; }
        public string DiaSemana1 { get; set; } = "";
        public string DiaSemana2 { get; set; } = "";
        public bool EsBisiesto1 { get; set; }
        public bool EsBisiesto2 { get; set; }
    }
}