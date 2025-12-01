using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ejercicios.Backend.Models;

namespace Ejercicios.Backend.Controllers
{
    /// <summary>
    /// Controlador para procesamiento de item con formato específico (ItemName$$##Price$$##Quantity)
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ItemController : ControllerBase
    {
        private readonly ILogger<ItemController> _logger;

        public ItemController(ILogger<ItemController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Procesa un item individual con formato específico y extrae sus componentes
        /// </summary>
        /// <param name="request"></param>
        /// <returns>Item procesado con nombre, precio, cantidad y salida formateada</returns>
        [HttpPost("procesar")]
        public ActionResult<ItemResponse> ProcesarItem([FromBody] ItemRequest request)
        {
            try
            {
                _logger.LogInformation("Iniciando procesamiento de item individual");
                
                // Validamos que el texto introducido no es vacío
                if (request == null || string.IsNullOrWhiteSpace(request.RawInput))
                {
                    _logger.LogWarning("Solicitud de procesamiento de item rechazada: entrada vacía o nula");
                    return BadRequest("Se requiere una cadena de entrada válida.");
                }

                _logger.LogDebug("Procesando item: {RawInput}", request.RawInput);

                // Crear el objeto ItemSeparator
                var itemSeparator = new ItemSeparator(request.RawInput);

                // Crear la respuesta
                var response = new ItemResponse
                {
                    Name = itemSeparator.GetName(),
                    Price = itemSeparator.GetPrice(),
                    Quantity = itemSeparator.GetQuantity(),
                    FormattedOutput = itemSeparator.ToString(),
                    Success = true,
                    ErrorMessage = ""
                };

                _logger.LogInformation("Item procesado exitosamente: Nombre={Name}, Precio={Price}, Cantidad={Quantity}", 
                    response.Name, response.Price, response.Quantity);

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError("Error de formato al procesar item '{RawInput}': {Error}", 
                    request?.RawInput ?? "null", ex.Message);
                
                return BadRequest(new ItemResponse
                {
                    Success = false,
                    ErrorMessage = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al procesar item '{RawInput}'", 
                    request?.RawInput ?? "null");
                
                return StatusCode(500, new ItemResponse
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Procesa múltiples items y calcula estadísticas agregadas
        /// </summary>
        /// <param name="request"></param>
        /// <returns>Lista de items procesados con resumen y valor total</returns>
        [HttpPost("procesar-multiples")]
        public ActionResult<MultipleItemsResponse> ProcesarMultiplesItems([FromBody] MultipleItemsRequest request)
        {
            try
            {
                _logger.LogInformation("Iniciando procesamiento múltiple de items");
                
                if (request == null || request.RawInputs == null || !request.RawInputs.Any())
                {
                    _logger.LogWarning("Solicitud de procesamiento múltiple rechazada: sin cadenas de entrada válidas");
                    return BadRequest("Se requiere al menos una cadena de entrada válida.");
                }

                _logger.LogDebug("Procesando {CantidadItems} items", request.RawInputs.Count);

                var response = new MultipleItemsResponse();
                double totalValue = 0;
                int itemsExitosos = 0;
                int itemsFallidos = 0;

                foreach (var rawInput in request.RawInputs)
                {
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(rawInput))
                        {
                            _logger.LogDebug("Procesando item individual: {RawInput}", rawInput);
                            
                            var itemSeparator = new ItemSeparator(rawInput);
                            var itemResponse = new ItemResponse
                            {
                                Name = itemSeparator.GetName(),
                                Price = itemSeparator.GetPrice(),
                                Quantity = itemSeparator.GetQuantity(),
                                FormattedOutput = itemSeparator.ToString(),
                                Success = true,
                                ErrorMessage = ""
                            };

                            response.Items.Add(itemResponse);
                            totalValue += itemResponse.Price * itemResponse.Quantity;
                            itemsExitosos++;
                            
                            _logger.LogDebug("Item procesado exitosamente: {Name} - ${Price} x {Quantity}", 
                                itemResponse.Name, itemResponse.Price, itemResponse.Quantity);
                        }
                    }
                    catch (ArgumentException ex)
                    {
                        _logger.LogWarning("Error de formato en item '{RawInput}': {Error}", rawInput, ex.Message);
                        
                        response.Items.Add(new ItemResponse
                        {
                            Success = false,
                            ErrorMessage = $"Error en '{rawInput}': {ex.Message}"
                        });
                        itemsFallidos++;
                    }
                }

                response.TotalItems = response.Items.Count(i => i.Success);
                response.TotalValue = totalValue;
                response.Summary = $"Procesados {response.TotalItems} items exitosamente. Valor total: ${totalValue:F2}";

                _logger.LogInformation("Procesamiento múltiple completado: {ItemsExitosos} exitosos, {ItemsFallidos} fallidos, Valor total: ${ValorTotal:F2}", 
                    itemsExitosos, itemsFallidos, totalValue);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado durante procesamiento múltiple");
                return StatusCode(500, new MultipleItemsResponse());
            }
        }

        /// <summary>
        /// Proporciona un ejemplo de formato válido
        /// </summary>
        /// <returns>String ejemplo con formato correcto</returns>
        [HttpGet("ejemplo")]
        public ActionResult<string> ObtenerEjemplo()
        {
            _logger.LogDebug("Proporcionando ejemplo de formato válido");
            return Ok("Bread$$##12.5$$##10");
        }
    }
}