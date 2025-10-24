namespace Ejercicios.Backend.Models
{
    // Enum para los colores disponibles
    public enum Color
    {
        Rojo,
        Azul,
        Verde,
        Amarillo,
        Naranja,
        Morado,
        Rosa,
        Cyan,
        Magenta,
        Lima
    }

    // Clase base para las formas geométricas
    public abstract class FormaGeometrica
    {
        public int Id { get; set; }
        public Color Color { get; set; }
        public int CentroX { get; set; }
        public int CentroY { get; set; }
        public int OrdenCreacion { get; set; }

        public abstract double CalcularArea();
        public abstract string GetTipo();
        public abstract string GetPropiedades();

        public string GetColorHex()
        {
            return Color switch
            {
                Color.Rojo => "#FF0000",
                Color.Azul => "#0000FF",
                Color.Verde => "#008000",
                Color.Amarillo => "#FFFF00",
                Color.Naranja => "#FFA500",
                Color.Morado => "#800080",
                Color.Rosa => "#FFC0CB",
                Color.Cyan => "#00FFFF",
                Color.Magenta => "#FF00FF",
                Color.Lima => "#00FF00",
                _ => "#000000"
            };
        }
    }

    // Clase Círculo
    public class Circulo : FormaGeometrica
    {
        public double Radio { get; set; }

        public override double CalcularArea()
        {
            return Math.PI * Radio * Radio;
        }

        public override string GetTipo()
        {
            return "Círculo";
        }

        public override string GetPropiedades()
        {
            return $"Radio: {Radio:F2}";
        }
    }

    // Clase Cuadrado
    public class Cuadrado : FormaGeometrica
    {
        public double Lado { get; set; }

        public override double CalcularArea()
        {
            return Lado * Lado;
        }

        public override string GetTipo()
        {
            return "Cuadrado";
        }

        public override string GetPropiedades()
        {
            return $"Lado: {Lado:F2}";
        }
    }

    // Clase Triángulo
    public class Triangulo : FormaGeometrica
    {
        public double Base { get; set; }
        public double Altura { get; set; }

        public override double CalcularArea()
        {
            return (Base * Altura) / 2;
        }

        public override string GetTipo()
        {
            return "Triángulo";
        }

        public override string GetPropiedades()
        {
            return $"Base: {Base:F2}, Altura: {Altura:F2}";
        }
    }

    // DTOs para la API
    public class GenerarFormasRequest
    {
        public int NumeroCirculos { get; set; }
        public int NumeroTriangulos { get; set; }
        public int NumeroCuadrados { get; set; }
    }

    public class FormaGeometricaResponse
    {
        public int Id { get; set; }
        public string Tipo { get; set; } = "";
        public string Propiedades { get; set; } = "";
        public double Area { get; set; }
        public string Color { get; set; } = "";
        public string ColorHex { get; set; } = "";
        public int CentroX { get; set; }
        public int CentroY { get; set; }
        public int OrdenCreacion { get; set; }
    }

    public class FormasGeneradasResponse
    {
        public List<FormaGeometricaResponse> TodasLasFormas { get; set; } = new();
        public List<FormaGeometricaResponse> Circulos { get; set; } = new();
        public List<FormaGeometricaResponse> Cuadrados { get; set; } = new();
        public List<FormaGeometricaResponse> Triangulos { get; set; } = new();
        public int TotalFormas { get; set; }
        public double AreaTotal { get; set; }
        public string Resumen { get; set; } = "";
    }
}