using Microsoft.AspNetCore.Mvc;
using Ejercicios.Backend.Models;

namespace Ejercicios.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ItemController : ControllerBase
    {
        [HttpPost("procesar")]
        public ActionResult<ItemResponse> ProcesarItem([FromBody] ItemRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.RawInput))
                {
                    return BadRequest("Se requiere una cadena de entrada válida.");
                }

                Console.WriteLine($"Procesando item: {request.RawInput}");

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

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Error de formato: {ex.Message}");
                return BadRequest(new ItemResponse
                {
                    Success = false,
                    ErrorMessage = ex.Message
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inesperado: {ex.Message}");
                return StatusCode(500, new ItemResponse
                {
                    Success = false,
                    ErrorMessage = "Error interno del servidor"
                });
            }
        }

        [HttpPost("procesar-multiples")]
        public ActionResult<MultipleItemsResponse> ProcesarMultiplesItems([FromBody] MultipleItemsRequest request)
        {
            try
            {
                if (request == null || request.RawInputs == null || !request.RawInputs.Any())
                {
                    return BadRequest("Se requiere al menos una cadena de entrada válida.");
                }

                var response = new MultipleItemsResponse();
                double totalValue = 0;

                foreach (var rawInput in request.RawInputs)
                {
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(rawInput))
                        {
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
                        }
                    }
                    catch (ArgumentException ex)
                    {
                        response.Items.Add(new ItemResponse
                        {
                            Success = false,
                            ErrorMessage = $"Error en '{rawInput}': {ex.Message}"
                        });
                    }
                }

                response.TotalItems = response.Items.Count(i => i.Success);
                response.TotalValue = totalValue;
                response.Summary = $"Procesados {response.TotalItems} items exitosamente. Valor total: ${totalValue:F2}";

                return Ok(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inesperado: {ex.Message}");
                return StatusCode(500, new MultipleItemsResponse());
            }
        }

        [HttpGet("ejemplo")]
        public ActionResult<string> ObtenerEjemplo()
        {
            return Ok("Bread$$##12.5$$##10");
        }
    }
}