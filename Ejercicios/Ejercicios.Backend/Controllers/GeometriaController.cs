using Microsoft.AspNetCore.Mvc;
using Ejercicios.Backend.Models;

namespace Ejercicios.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GeometriaController : ControllerBase
    {
        private readonly Random _random = new Random();

        [HttpPost("generar")]
        public ActionResult<FormasGeneradasResponse> GenerarFormas([FromBody] GenerarFormasRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Los datos de entrada son requeridos.");
                }

                Console.WriteLine($"Generando formas: {request.NumeroCirculos} círculos, {request.NumeroCuadrados} cuadrados, {request.NumeroTriangulos} triángulos");

                // Validar que al menos se genere una forma
                if (request.NumeroCirculos + request.NumeroCuadrados + request.NumeroTriangulos == 0)
                {
                    return BadRequest("Debe generar al menos una forma geométrica.");
                }

                // Validar números no negativos
                if (request.NumeroCirculos < 0 || request.NumeroCuadrados < 0 || request.NumeroTriangulos < 0)
                {
                    return BadRequest("Los números de formas no pueden ser negativos.");
                }

                var todasLasFormas = new List<FormaGeometrica>();
                int ordenCreacion = 1;

                // Generar círculos
                for (int i = 0; i < request.NumeroCirculos; i++)
                {
                    var circulo = new Circulo
                    {
                        Id = ordenCreacion,
                        Radio = GenerarNumeroPositivo(1.0, 20.0),
                        Color = GenerarColorAleatorio(),
                        CentroX = _random.Next(0, 500),
                        CentroY = _random.Next(0, 500),
                        OrdenCreacion = ordenCreacion++
                    };
                    todasLasFormas.Add(circulo);
                }

                // Generar cuadrados
                for (int i = 0; i < request.NumeroCuadrados; i++)
                {
                    var cuadrado = new Cuadrado
                    {
                        Id = ordenCreacion,
                        Lado = GenerarNumeroPositivo(1.0, 25.0),
                        Color = GenerarColorAleatorio(),
                        CentroX = _random.Next(0, 500),
                        CentroY = _random.Next(0, 500),
                        OrdenCreacion = ordenCreacion++
                    };
                    todasLasFormas.Add(cuadrado);
                }

                // Generar triángulos
                for (int i = 0; i < request.NumeroTriangulos; i++)
                {
                    var triangulo = new Triangulo
                    {
                        Id = ordenCreacion,
                        Base = GenerarNumeroPositivo(2.0, 30.0),
                        Altura = GenerarNumeroPositivo(2.0, 25.0),
                        Color = GenerarColorAleatorio(),
                        CentroX = _random.Next(0, 500),
                        CentroY = _random.Next(0, 500),
                        OrdenCreacion = ordenCreacion++
                    };
                    todasLasFormas.Add(triangulo);
                }

                // Mezclar aleatoriamente el orden de creación para simular generación aleatoria
                var formasAleatorias = todasLasFormas.OrderBy(x => _random.Next()).ToList();
                for (int i = 0; i < formasAleatorias.Count; i++)
                {
                    formasAleatorias[i].OrdenCreacion = i + 1;
                }

                // Convertir a DTOs
                var todasLasFormasDto = formasAleatorias
                    .OrderBy(f => f.OrdenCreacion)
                    .Select(ConvertirADto)
                    .ToList();

                var circulosDto = formasAleatorias
                    .OfType<Circulo>()
                    .Select(ConvertirADto)
                    .ToList();

                var cuadradosDto = formasAleatorias
                    .OfType<Cuadrado>()
                    .Select(ConvertirADto)
                    .ToList();

                var triangulosDto = formasAleatorias
                    .OfType<Triangulo>()
                    .Select(ConvertirADto)
                    .ToList();

                var areaTotal = todasLasFormas.Sum(f => f.CalcularArea());

                var response = new FormasGeneradasResponse
                {
                    TodasLasFormas = todasLasFormasDto,
                    Circulos = circulosDto,
                    Cuadrados = cuadradosDto,
                    Triangulos = triangulosDto,
                    TotalFormas = todasLasFormas.Count,
                    AreaTotal = areaTotal,
                    Resumen = $"Generadas {todasLasFormas.Count} formas: {request.NumeroCirculos} círculos, {request.NumeroCuadrados} cuadrados, {request.NumeroTriangulos} triángulos. Área total: {areaTotal:F2}"
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al generar formas: {ex.Message}");
                return StatusCode(500, $"Error al generar formas: {ex.Message}");
            }
        }

        private double GenerarNumeroPositivo(double min, double max)
        {
            return Math.Round(_random.NextDouble() * (max - min) + min, 2);
        }

        private Color GenerarColorAleatorio()
        {
            var colores = Enum.GetValues<Color>();
            return colores[_random.Next(colores.Length)];
        }

        private FormaGeometricaResponse ConvertirADto(FormaGeometrica forma)
        {
            return new FormaGeometricaResponse
            {
                Id = forma.Id,
                Tipo = forma.GetTipo(),
                Propiedades = forma.GetPropiedades(),
                Area = Math.Round(forma.CalcularArea(), 2),
                Color = forma.Color.ToString(),
                ColorHex = forma.GetColorHex(),
                CentroX = forma.CentroX,
                CentroY = forma.CentroY,
                OrdenCreacion = forma.OrdenCreacion
            };
        }
    }
}