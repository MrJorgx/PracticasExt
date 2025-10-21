namespace Ejercicios.Backend.Models
{
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