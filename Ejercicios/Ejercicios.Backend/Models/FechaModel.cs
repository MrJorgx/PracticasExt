namespace Ejercicios.Backend.Models
{
    /// <summary>
    /// Modelo de entrada para operaciones con fechas
    /// </summary>
    public class FechaRequest
    {
        /// <summary>
        /// Primera fecha para las operaciones
        /// </summary>
        public string Fecha1Text { get; set; } = "";

        /// <summary>
        /// Segunda fecha para las operaciones
        /// </summary>
        public string Fecha2Text { get; set; } = "";
    }

    /// <summary>
    /// Modelo de respuesta con los resultados de todas las operaciones con fechas
    /// </summary>
    public class FechaResult
    {
        /// <summary>
        /// Primera fecha formateada a DateTime
        /// </summary>
        public DateTime Fecha1 { get; set; }

        /// <summary>
        /// Segunda fecha formateada a DateTime
        /// </summary>
        public DateTime Fecha2 { get; set; }

        /// <summary>
        /// Diferencia de días entre las dos fechas
        /// </summary>
        public int DiferenciaDias { get; set; }

        /// <summary>
        /// Fecha de inicio del año de la primera fecha
        /// </summary>
        public DateTime InicioAno1 { get; set; }

        /// <summary>
        /// Fecha de fin del año de la primera fecha
        /// </summary>
        public DateTime FinAno1 { get; set; }

        /// <summary>
        /// Fecha de inicio del año de la segunda fecha
        /// </summary>
        public DateTime InicioAno2 { get; set; }

        /// <summary>
        /// Fecha de fin del año de la segunda fecha
        /// </summary>
        public DateTime FinAno2 { get; set; }

        /// <summary>
        /// Número de días del año de la primera fecha
        /// </summary>
        public int DiasDelAno1 { get; set; }

        /// <summary>
        /// Número de días del año de la segunda fecha
        /// </summary>
        public int DiasDelAno2 { get; set; }

        /// <summary>
        /// Número de la semana de la primera fecha
        /// </summary>
        public int NumeroSemana1 { get; set; }

        /// <summary>
        /// Número de la semana de la segunda fecha
        /// </summary>
        public int NumeroSemana2 { get; set; }

        /// <summary>
        /// Día de la semana en que cae la primera fecha
        /// </summary>
        public string DiaSemana1 { get; set; } = "";

        /// <summary>
        /// Día de la semana en que cae la segunda fecha
        /// </summary>
        public string DiaSemana2 { get; set; } = "";

        /// <summary>
        /// Resultado de la operación para comprobar si el año de la primera fecha es bisiesto (si, no)
        /// </summary>
        public bool EsBisiesto1 { get; set; }

        /// <summary>
        /// Resultado de la operación para comprobar si el año de la segunda fecha es bisiesto (si, no)
        /// </summary>
        public bool EsBisiesto2 { get; set; }
    }
}