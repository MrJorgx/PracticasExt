namespace Ejercicios.Backend.Models
{
    /// <summary>
    /// Modelo de entrada para operaciones matemáticas
    /// </summary>
    public class OperacionRequest
    {
        /// <summary>
        /// Primer número para las operaciones
        /// </summary>
        public double Cantidad1 { get; set; }

        /// <summary>
        /// Segundo número para las operaciones
        /// </summary>
        public double Cantidad2 { get; set; }

        /// <summary>
        /// Número de decimales para formatear los resultados
        /// </summary>
        public int NumDecimales { get; set; }
    }

    /// <summary>
    /// Modelo de respuesta con los resultados de todas las operaciones matemáticas
    /// </summary>
    public class OperacionResult
    {
        /// <summary>
        /// Resultado de la suma formateada como string
        /// </summary>
        public string Suma { get; set; } = "";

        /// <summary>
        /// Resultado de la resta formateada como string
        /// </summary>
        public string Resta { get; set; } = "";

        /// <summary>
        /// Resultado de la multiplicación formateada como string
        /// </summary>
        public string Multiplicacion { get; set; } = "";

        /// <summary>
        /// Resultado de la división formateada como string (maneja división por 0)
        /// </summary>
        public string Division { get; set; } = "";

        /// <summary>
        /// Resultado del módulo formateado como string (maneja división por 0)
        /// </summary>
        public string Modulo { get; set; } = "";

        /// <summary>
        /// Resultado de la comparación entre los números (mayor, menor, igual)
        /// </summary>
        public string Comparacion { get; set; } = "";
    }
}