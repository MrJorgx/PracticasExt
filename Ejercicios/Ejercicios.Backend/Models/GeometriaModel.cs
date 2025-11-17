namespace Ejercicios.Backend.Models
{
    /// <summary>
    /// Enumeración de colores disponibles para las formas geométricas 
    /// </summary>
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

    /// <summary>
    /// Clase base abstracta para todas las formas geométricas
    /// </summary>
    public abstract class FormaGeometrica
    {
        /// <summary>
        /// Identificador único de la forma
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Color de la forma
        /// </summary>
        public Color Color { get; set; }

        /// <summary>
        /// Coordenada X del centro de la forma
        /// </summary>
        public int CentroX { get; set; }

        /// <summary>
        /// Coordenada Y del centro de la forma
        /// </summary>
        public int CentroY { get; set; }

        /// <summary>
        /// Número que indica el orden en que se creó la forma
        /// </summary>
        public int OrdenCreacion { get; set; }

        /// <summary>
        /// Calcula el área de la forma geométrica
        /// </summary>
        /// <returns>Área de la forma en unidades cuadradas</returns>
        public abstract double CalcularArea();

        /// <summary>
        /// Obtiene el tipo de forma como string
        /// </summary>
        /// <returns>Nombre del tipo de forma</returns>
        public abstract string GetTipo();

        /// <summary>
        /// Obtiene una descripción de las propiedades específicas de la forma
        /// </summary>
        /// <returns>Descripción formateada de las propiedades</returns>
        public abstract string GetPropiedades();

        /// <summary>
        /// Convierte el color de la enumeración a código hexadecimal
        /// </summary>
        /// <returns>Código hexadecimal del color</returns>
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

    /// <summary>
    /// Representa un círculo con radio específico
    /// </summary>
    public class Circulo : FormaGeometrica
    {
        /// <summary>
        /// Radio del círculo
        /// </summary>
        public double Radio { get; set; }

        /// <summary>
        /// Calcula el área del círculo usando la fórmula PI * r²
        /// </summary>
        /// <returns>Área del círculo</returns>
        public override double CalcularArea()
        {
            return Math.PI * Radio * Radio;
        }

        /// <summary>
        /// Obtiene el tipo de forma
        /// </summary>
        /// <returns>La cadena "Círculo"</returns>
        public override string GetTipo()
        {
            return "Círculo";
        }

        /// <summary>
        /// Obtiene las propiedades específicas del círculo
        /// </summary>
        /// <returns>Descripción del radio formateada</returns>
        public override string GetPropiedades()
        {
            return $"Radio: {Radio:F2}";
        }
    }

    /// <summary>
    /// Representa un cuadrado con lado específico
    /// </summary>
    public class Cuadrado : FormaGeometrica
    {
        /// <summary>
        /// Longitud del lado del cuadrado
        /// </summary>
        public double Lado { get; set; }

        /// <summary>
        /// Calcula el área del cuadrado usando la fórmula lado²
        /// </summary>
        /// <returns>Área del cuadrado</returns>
        public override double CalcularArea()
        {
            return Lado * Lado;
        }

        /// <summary>
        /// Obtiene el tipo de forma
        /// </summary>
        /// <returns>La cadena "Cuadrado"</returns>
        public override string GetTipo()
        {
            return "Cuadrado";
        }

        /// <summary>
        /// Obtiene las propiedades específicas del cuadrado
        /// </summary>
        /// <returns>Descripción del lado formateada</returns>
        public override string GetPropiedades()
        {
            return $"Lado: {Lado:F2}";
        }
    }

    /// <summary>
    /// Representa un triángulo con base y altura específicas
    /// </summary>
    public class Triangulo : FormaGeometrica
    {
        /// <summary>
        /// Base del triángulo
        /// </summary>
        public double Base { get; set; }

        /// <summary>
        /// Altura del triángulo
        /// </summary>
        public double Altura { get; set; }

        /// <summary>
        /// Calcula el área del triángulo usando la formula (base * altura) / 2
        /// </summary>
        /// <returns>Área del triángulo</returns>
        public override double CalcularArea()
        {
            return (Base * Altura) / 2;
        }

        /// <summary>
        /// Obtiene el tipo de forma
        /// </summary>
        /// <returns>La cadena "Triángulo"</returns>
        public override string GetTipo()
        {
            return "Triángulo";
        }

        /// <summary>
        /// Obtiene las propiedades específicas del triángulo
        /// </summary>
        /// <returns>Descripción de base y altura formateadas</returns>
        public override string GetPropiedades()
        {
            return $"Base: {Base:F2}, Altura: {Altura:F2}";
        }
    }

    /// <summary>
    /// DTO para las solicitudes de generación de formas geométricas
    /// </summary>
    public class GenerarFormasRequest
    {
        /// <summary>
        /// Número de círculos a generar
        /// </summary>
        public int NumeroCirculos { get; set; }

        /// <summary>
        /// Número de triángulos a generar
        /// </summary>
        public int NumeroTriangulos { get; set; }

        /// <summary>
        /// Número de cuadrados a generar
        /// </summary>
        public int NumeroCuadrados { get; set; }
    }

    /// <summary>
    /// DTO para la respuesta de una forma geométrica individual
    /// </summary>
    public class FormaGeometricaResponse
    {
        /// <summary>
        /// Identificador único de la forma
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Tipo de forma (círculo, cuadrado, triángulo)
        /// </summary>
        public string Tipo { get; set; } = "";

        /// <summary>
        /// Descripción de las propiedades de la forma
        /// </summary>
        public string Propiedades { get; set; } = "";

        /// <summary>
        /// Área calculada de la forma
        /// </summary>
        public double Area { get; set; }

        /// <summary>
        /// Nombre del color de la forma
        /// </summary>
        public string Color { get; set; } = "";

        /// <summary>
        /// Código hexadecimal del color
        /// </summary>
        public string ColorHex { get; set; } = "";

        /// <summary>
        /// Coordenada X del centro
        /// </summary>
        public int CentroX { get; set; }

        /// <summary>
        /// Coordenada Y del centro
        /// </summary>
        public int CentroY { get; set; }

        /// <summary>
        /// Orden de creación de la forma
        /// </summary>
        public int OrdenCreacion { get; set; }
    }

    /// <summary>
    /// DTO para la respuesta completa de generación de formas con agrupaciones y métricas
    /// </summary>
    public class FormasGeneradasResponse
    {
        /// <summary>
        /// Lista de todas las formas generadas en orden de creación
        /// </summary>
        public List<FormaGeometricaResponse> TodasLasFormas { get; set; } = new();

        /// <summary>
        /// Lista filtrada solo de círculos generados
        /// </summary>
        public List<FormaGeometricaResponse> Circulos { get; set; } = new();

        /// <summary>
        /// Lista filtrada solo de cuadrados generados
        /// </summary>
        public List<FormaGeometricaResponse> Cuadrados { get; set; } = new();

        /// <summary>
        /// Lista filtrada solo de triángulos generados
        /// </summary>
        public List<FormaGeometricaResponse> Triangulos { get; set; } = new();

        /// <summary>
        /// Número total de formas generadas
        /// </summary>
        public int TotalFormas { get; set; }

        /// <summary>
        /// Área total sumando todas las formas
        /// </summary>
        public double AreaTotal { get; set; }

        /// <summary>
        /// Resumen textual de la generación
        /// </summary>
        public string Resumen { get; set; } = "";
    }
}