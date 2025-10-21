namespace Ejercicios.Backend.Models
{
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