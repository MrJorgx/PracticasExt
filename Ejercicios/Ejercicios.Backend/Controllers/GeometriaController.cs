using Microsoft.AspNetCore.Mvc;
using Ejercicios.Backend.Models;

namespace Ejercicios.Backend.Controllers
{
    /// <summary>
    /// Controlador para generación aleatoria de formas geométricas
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class GeometriaController : ControllerBase
    {
        private readonly Random _random = new Random();
        private readonly ILogger<GeometriaController> _logger;

        public GeometriaController(ILogger<GeometriaController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Genera formas geométricas aleatorias (círculos, cuadrados y triángulos) con propiedades aleatorias
        /// </summary>
        /// <param name="request"></param>
        /// <returns>Colección de formas generadas organizadas por tipo con métricas calculadas</returns>
        [HttpPost("generar")]
        public ActionResult<FormasGeneradasResponse> GenerarFormas([FromBody] GenerarFormasRequest request)
        {
            try
            {
                _logger.LogInformation("Iniciando generación de formas geométricas");
                
                if (request == null)
                {
                    _logger.LogWarning("Solicitud de generación de formas rechazada: datos de entrada nulos");
                    return BadRequest("Los datos de entrada son requeridos.");
                }

                _logger.LogInformation("Generando formas: {NumeroCirculos} círculos, {NumeroCuadrados} cuadrados, {NumeroTriangulos} triángulos", 
                    request.NumeroCirculos, request.NumeroCuadrados, request.NumeroTriangulos);

                // Validar que al menos se genere una forma
                if (request.NumeroCirculos + request.NumeroCuadrados + request.NumeroTriangulos == 0)
                {
                    _logger.LogWarning("Generación rechazada: no se solicitó generar ninguna forma");
                    return BadRequest("Debe generar al menos una forma geométrica.");
                }

                // Validar números no negativos
                if (request.NumeroCirculos < 0 || request.NumeroCuadrados < 0 || request.NumeroTriangulos < 0)
                {
                    _logger.LogWarning("Generación rechazada: números negativos - Círculos: {Circulos}, Cuadrados: {Cuadrados}, Triángulos: {Triangulos}", 
                        request.NumeroCirculos, request.NumeroCuadrados, request.NumeroTriangulos);
                    return BadRequest("Los números de formas no pueden ser negativos.");
                }

                var todasLasFormas = new List<FormaGeometrica>();
                int ordenCreacion = 1;

                _logger.LogDebug("Iniciando generación de círculos");
                // Generar círculos
                for (int i = 0; i < request.NumeroCirculos; i++)
                {
                    var radio = GenerarNumeroPositivo(1.0, 20.0);
                    var color = GenerarColorAleatorio();
                    var centroX = _random.Next(0, 500);
                    var centroY = _random.Next(0, 500);
                    
                    var circulo = new Circulo
                    {
                        Id = ordenCreacion,
                        Radio = radio,
                        Color = color,
                        CentroX = centroX,
                        CentroY = centroY,
                        OrdenCreacion = ordenCreacion++
                    };
                    todasLasFormas.Add(circulo);
                    
                    _logger.LogTrace("Círculo {Id} generado: Radio={Radio}, Color={Color}, Centro=({CentroX},{CentroY})", 
                        circulo.Id, radio, color, centroX, centroY);
                }

                _logger.LogDebug("Iniciando generación de cuadrados");
                // Generar cuadrados
                for (int i = 0; i < request.NumeroCuadrados; i++)
                {
                    var lado = GenerarNumeroPositivo(1.0, 25.0);
                    var color = GenerarColorAleatorio();
                    var centroX = _random.Next(0, 500);
                    var centroY = _random.Next(0, 500);
                    
                    var cuadrado = new Cuadrado
                    {
                        Id = ordenCreacion,
                        Lado = lado,
                        Color = color,
                        CentroX = centroX,
                        CentroY = centroY,
                        OrdenCreacion = ordenCreacion++
                    };
                    todasLasFormas.Add(cuadrado);
                    
                    _logger.LogTrace("Cuadrado {Id} generado: Lado={Lado}, Color={Color}, Centro=({CentroX},{CentroY})", 
                        cuadrado.Id, lado, color, centroX, centroY);
                }

                _logger.LogDebug("Iniciando generación de triángulos");
                // Generar triángulos
                for (int i = 0; i < request.NumeroTriangulos; i++)
                {
                    var baseTriangulo = GenerarNumeroPositivo(2.0, 30.0);
                    var altura = GenerarNumeroPositivo(2.0, 25.0);
                    var color = GenerarColorAleatorio();
                    var centroX = _random.Next(0, 500);
                    var centroY = _random.Next(0, 500);
                    
                    var triangulo = new Triangulo
                    {
                        Id = ordenCreacion,
                        Base = baseTriangulo,
                        Altura = altura,
                        Color = color,
                        CentroX = centroX,
                        CentroY = centroY,
                        OrdenCreacion = ordenCreacion++
                    };
                    todasLasFormas.Add(triangulo);
                    
                    _logger.LogTrace("Triángulo {Id} generado: Base={Base}, Altura={Altura}, Color={Color}, Centro=({CentroX},{CentroY})", 
                        triangulo.Id, baseTriangulo, altura, color, centroX, centroY);
                }

                _logger.LogDebug("Mezclando orden aleatorio de {TotalFormas} formas generadas", todasLasFormas.Count);
                // Mezclar aleatoriamente el orden de creación para simular generación aleatoria
                var formasAleatorias = todasLasFormas.OrderBy(x => _random.Next()).ToList();
                for (int i = 0; i < formasAleatorias.Count; i++)
                {
                    formasAleatorias[i].OrdenCreacion = i + 1;
                }

                _logger.LogDebug("Convirtiendo formas a DTOs para respuesta");
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

                _logger.LogInformation("Generación completada exitosamente: {TotalFormas} formas, Área total: {AreaTotal:F2}. Distribución: {Circulos} círculos, {Cuadrados} cuadrados, {Triangulos} triángulos", 
                    response.TotalFormas, areaTotal, circulosDto.Count, cuadradosDto.Count, triangulosDto.Count);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al generar formas geométricas: Círculos={Circulos}, Cuadrados={Cuadrados}, Triángulos={Triangulos}", 
                    request?.NumeroCirculos, request?.NumeroCuadrados, request?.NumeroTriangulos);
                return StatusCode(500, $"Error al generar formas: {ex.Message}");
            }
        }

        /// <summary>
        /// Genera un número decimal aleatorio positivo dentro de un rango específico
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns>Número decimal aleatorio redondeado a 2 decimales</returns>
        private double GenerarNumeroPositivo(double min, double max)
        {
            var numero = Math.Round(_random.NextDouble() * (max - min) + min, 2);
            _logger.LogTrace("Número aleatorio generado: {Numero} (rango: {Min}-{Max})", numero, min, max);
            return numero;
        }

        /// <summary>
        /// Selecciona un color aleatorio de la enumeración disponible
        /// </summary>
        /// <returns>Color aleatorio del enum Color</returns>
        private Color GenerarColorAleatorio()
        {
            var colores = Enum.GetValues<Color>();
            var colorSeleccionado = colores[_random.Next(colores.Length)];
            _logger.LogTrace("Color aleatorio seleccionado: {Color}", colorSeleccionado);
            return colorSeleccionado;
        }

        /// <summary>
        /// Convierte una forma geométrica del modelo interno a DTO para la respuesta API
        /// </summary>
        /// <param name="forma"></param>
        /// <returns>DTO con toda la información de la forma para el frontend</returns>
        private FormaGeometricaResponse ConvertirADto(FormaGeometrica forma)
        {
            var dto = new FormaGeometricaResponse
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
            
            _logger.LogTrace("Forma {Id} convertida a DTO: {Tipo}, Área={Area:F2}", forma.Id, dto.Tipo, dto.Area);
            return dto;
        }
    }
}