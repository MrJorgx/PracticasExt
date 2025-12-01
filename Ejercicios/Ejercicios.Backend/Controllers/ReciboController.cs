using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ejercicios.Backend.Data;
using Ejercicios.Backend.Models;
using System.Text.RegularExpressions;

namespace Ejercicios.Backend.Controllers
{
    /// <summary>
    /// Controlador para operaciones CRUD de recibos
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ReciboController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ReciboController> _logger;

        public ReciboController(AppDbContext context, ILogger<ReciboController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Valida que el DNI tenga el formato correcto (8 dígitos y 1 letra)
        /// </summary>
        /// <param name="dni"></param>
        /// <returns>True si el formato es válido, False en caso contrario</returns>
        private bool ValidarFormatoDni(string dni)
        {
            if (string.IsNullOrWhiteSpace(dni))
                return false;

            dni = dni.Trim().ToUpper();
            var patron = @"^[0-9]{8}[A-Z]$";
            return Regex.IsMatch(dni, patron);
        }

        /// <summary>
        /// Crea un nuevo recibo en la base de datos con validaciones de cliente y cuota máxima
        /// </summary>
        /// <param name="request"></param>
        /// <returns>Datos del recibo creado con información del cliente asociado</returns>
        [HttpPost]
        public async Task<ActionResult<ReciboResponse>> CrearRecibo([FromBody] ReciboRequest request)
        {
            try
            {
                _logger.LogInformation("Iniciando creación de recibo");

                if (request == null)
                {
                    _logger.LogWarning("Solicitud de creación de recibo recibida sin datos");
                    return BadRequest("Los datos de entrada son requeridos");
                }

                // Normalizar DNI
                request.DniCliente = request.DniCliente?.Trim().ToUpper() ?? "";

                _logger.LogInformation("Creando recibo: Número={NumeroRecibo}, DNI Cliente={DniCliente}, Importe={Importe}", 
                    request.NumeroRecibo, request.DniCliente, request.Importe);

                // Validar formato DNI
                if (!ValidarFormatoDni(request.DniCliente))
                {
                    _logger.LogWarning("Formato de DNI inválido en creación de recibo: {DniCliente}", request.DniCliente);
                    return BadRequest("El DNI debe tener 8 dígitos y una letra.");
                }

                // Validar que no exista un recibo con el mismo número
                if (await _context.Recibos.AnyAsync(r => r.NumeroRecibo == request.NumeroRecibo))
                {
                    _logger.LogWarning("Intento de crear recibo con número duplicado: {NumeroRecibo}", request.NumeroRecibo);
                    return BadRequest($"Ya existe un recibo con número {request.NumeroRecibo}");
                }

                // Validar que existe el cliente
                var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Dni == request.DniCliente);
                if (cliente == null)
                {
                    _logger.LogWarning("Intento de crear recibo para cliente inexistente: {DniCliente}", request.DniCliente);
                    return BadRequest($"No existe cliente con DNI {request.DniCliente}");
                }

                // Validar cuota máxima para clientes REGISTRADO
                if (cliente.TipoCliente == TipoCliente.REGISTRADO)
                {
                    if (request.Importe > cliente.CuotaMaxima)
                    {
                        _logger.LogWarning("Importe excede cuota máxima: Cliente={DniCliente}, Importe={Importe}, CuotaMaxima={CuotaMaxima}", 
                            request.DniCliente, request.Importe, cliente.CuotaMaxima);
                        return BadRequest($"El importe {request.Importe} supera la cuota máxima permitida de {cliente.CuotaMaxima}");
                    }
                }

                var recibo = new Recibo
                {
                    NumeroRecibo = request.NumeroRecibo,
                    DniCliente = request.DniCliente,
                    Importe = request.Importe,
                    FechaEmision = request.FechaEmision?.ToUniversalTime() ?? DateTime.UtcNow
                };

                _context.Recibos.Add(recibo);
                await _context.SaveChangesAsync();

                _logger.LogDebug("Recibo guardado en base de datos: {NumeroRecibo}", request.NumeroRecibo);

                // Recargar el recibo con el cliente incluido
                var reciboConCliente = await _context.Recibos
                    .Include(r => r.Cliente)
                    .FirstOrDefaultAsync(r => r.NumeroRecibo == recibo.NumeroRecibo);

                if (reciboConCliente == null || reciboConCliente.Cliente == null)
                {
                    _logger.LogError("Error al recuperar recibo creado: {NumeroRecibo}", request.NumeroRecibo);
                    return StatusCode(500, "Error al recuperar el recibo creado");
                }

                var response = new ReciboResponse
                {
                    NumeroRecibo = reciboConCliente.NumeroRecibo,
                    DniCliente = reciboConCliente.DniCliente,
                    NombreCliente = $"{reciboConCliente.Cliente.Nombre} {reciboConCliente.Cliente.Apellidos}",
                    Importe = reciboConCliente.Importe,
                    FechaEmision = reciboConCliente.FechaEmision
                };

                _logger.LogInformation("Recibo creado exitosamente: {NumeroRecibo} para cliente {DniCliente} - {NombreCliente}, Importe: {Importe}", 
                    response.NumeroRecibo, response.DniCliente, response.NombreCliente, response.Importe);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear recibo: Número={NumeroRecibo}, DNI={DniCliente}", 
                    request?.NumeroRecibo, request?.DniCliente);
                return StatusCode(500, $"Error al crear recibo: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtiene todos los recibos de un cliente específico
        /// </summary>
        /// <param name="dni"></param>
        /// <param name="ordenarPorFecha"></param>
        /// <param name="descendente"></param>
        /// <returns>Lista de recibos del cliente con información completa</returns>
        [HttpGet("cliente/{dni}")]
        public async Task<ActionResult<List<ReciboResponse>>> ObtenerRecibosPorCliente(
            string dni, 
            [FromQuery] bool ordenarPorFecha = true, 
            [FromQuery] bool descendente = false)
        {
            try
            {
                _logger.LogInformation("Buscando recibos para cliente: {DNI}, OrdenarPorFecha: {OrdenarPorFecha}, Descendente: {Descendente}", 
                    dni, ordenarPorFecha, descendente);

                dni = dni?.Trim().ToUpper() ?? "";

                if (!ValidarFormatoDni(dni))
                {
                    _logger.LogWarning("Formato de DNI inválido en búsqueda de recibos: {DNI}", dni);
                    return BadRequest("El DNI debe tener 8 dígitos y una letra.");
                }
                
                // Validar que existe el cliente
                if (!await _context.Clientes.AnyAsync(c => c.Dni == dni))
                {
                    _logger.LogWarning("Cliente no encontrado para búsqueda de recibos: {DNI}", dni);
                    return NotFound($"No existe cliente con DNI {dni}");
                }

                var query = _context.Recibos
                    .Include(r => r.Cliente)
                    .Where(r => r.DniCliente == dni);

                if (ordenarPorFecha)
                {
                    query = descendente ? query.OrderByDescending(r => r.FechaEmision) : query.OrderBy(r => r.FechaEmision);
                }
                else
                {
                    query = descendente ? query.OrderByDescending(r => r.NumeroRecibo) : query.OrderBy(r => r.NumeroRecibo);
                }

                var recibos = await query.ToListAsync();

                _logger.LogDebug("Recibos encontrados para cliente {DNI}: {TotalRecibos}", dni, recibos.Count);

                var response = recibos.Select(recibo => new ReciboResponse
                {
                    NumeroRecibo = recibo.NumeroRecibo,
                    DniCliente = recibo.DniCliente,
                    NombreCliente = $"{recibo.Cliente.Nombre} {recibo.Cliente.Apellidos}",
                    Importe = recibo.Importe,
                    FechaEmision = recibo.FechaEmision
                }).ToList();

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener recibos del cliente: {DNI}", dni);
                return StatusCode(500, $"Error al obtener recibos del cliente: {ex.Message}");
            }
        }

        /// <summary>
        /// Actualiza el importe y fecha de emisión de un recibo existente
        /// </summary>
        /// <param name="numeroRecibo"></param>
        /// <param name="request"></param>
        /// <returns>Datos actualizados del recibo</returns>
        [HttpPut("{numeroRecibo}")]
        public async Task<ActionResult<ReciboResponse>> ActualizarRecibo(string numeroRecibo, [FromBody] ReciboUpdateRequest request)
        {
            try
            {
                _logger.LogInformation("Iniciando actualización de recibo: {NumeroRecibo}", numeroRecibo);

                if (request == null)
                {
                    _logger.LogWarning("Solicitud de actualización de recibo recibida sin datos");
                    return BadRequest("Los datos de entrada son requeridos");
                }

                var recibo = await _context.Recibos
                    .Include(r => r.Cliente)
                    .FirstOrDefaultAsync(r => r.NumeroRecibo == numeroRecibo);

                if (recibo == null)
                {
                    _logger.LogWarning("Recibo no encontrado para actualización: {NumeroRecibo}", numeroRecibo);
                    return NotFound($"No se encontró recibo con número {numeroRecibo}");
                }

                var importeAnterior = recibo.Importe;

                // Validar cuota máxima para clientes REGISTRADO
                if (recibo.Cliente.TipoCliente == TipoCliente.REGISTRADO)
                {
                    if (request.Importe > recibo.Cliente.CuotaMaxima)
                    {
                        _logger.LogWarning("Actualización: Importe excede cuota máxima: Recibo={NumeroRecibo}, Cliente={DniCliente}, Importe={Importe}, CuotaMaxima={CuotaMaxima}", 
                            numeroRecibo, recibo.DniCliente, request.Importe, recibo.Cliente.CuotaMaxima);
                        return BadRequest($"El importe {request.Importe} supera la cuota máxima permitida de {recibo.Cliente.CuotaMaxima}");
                    }
                }

                recibo.Importe = request.Importe;
                recibo.FechaEmision = request.FechaEmision ?? recibo.FechaEmision;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Recibo actualizado exitosamente: {NumeroRecibo}, Cliente: {DniCliente}, Importe anterior: {ImporteAnterior}, Importe nuevo: {ImporteNuevo}", 
                    numeroRecibo, recibo.DniCliente, importeAnterior, recibo.Importe);

                var response = new ReciboResponse
                {
                    NumeroRecibo = recibo.NumeroRecibo,
                    DniCliente = recibo.DniCliente,
                    NombreCliente = $"{recibo.Cliente.Nombre} {recibo.Cliente.Apellidos}",
                    Importe = recibo.Importe,
                    FechaEmision = recibo.FechaEmision
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar recibo: {NumeroRecibo}", numeroRecibo);
                return StatusCode(500, $"Error al actualizar recibo: {ex.Message}");
            }
        }

        /// <summary>
        /// Elimina un recibo de la base de datos 
        /// </summary>
        /// <param name="numeroRecibo"></param>
        /// <returns>Mensaje de confirmación de la eliminación</returns>
        [HttpDelete("{numeroRecibo}")]
        public async Task<IActionResult> EliminarRecibo(string numeroRecibo)
        {
            try
            {
                _logger.LogInformation("Iniciando eliminación de recibo: {NumeroRecibo}", numeroRecibo);

                var recibo = await _context.Recibos
                    .Include(r => r.Cliente)
                    .FirstOrDefaultAsync(r => r.NumeroRecibo == numeroRecibo);

                if (recibo == null)
                {
                    _logger.LogWarning("Recibo no encontrado para eliminación: {NumeroRecibo}", numeroRecibo);
                    return NotFound($"No se encontró recibo con número {numeroRecibo}");
                }

                var infoRecibo = $"Cliente: {recibo.DniCliente}, Importe: {recibo.Importe}";

                _context.Recibos.Remove(recibo);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Recibo eliminado exitosamente: {NumeroRecibo} - {InfoRecibo}", 
                    numeroRecibo, infoRecibo);

                return Ok($"Recibo {numeroRecibo} eliminado correctamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar recibo: {NumeroRecibo}", numeroRecibo);
                return StatusCode(500, $"Error al eliminar recibo: {ex.Message}");
            }
        }

        /// <summary>
        /// Lista todos los recibos
        /// </summary>
        /// <param name="ordenarPor"></param>
        /// <param name="descendente"></param>
        /// <returns>Lista de todos los recibos con información completa de clientes</returns>
        [HttpGet]
        public async Task<ActionResult<List<ReciboResponse>>> ListarTodosLosRecibos(
            [FromQuery] string ordenarPor = "fecha", 
            [FromQuery] bool descendente = false)
        {
            try
            {
                _logger.LogInformation("Listando todos los recibos. Ordenar por: {OrdenarPor}, Descendente: {Descendente}", 
                    ordenarPor, descendente);

                var query = _context.Recibos.Include(r => r.Cliente).AsQueryable();

                query = ordenarPor.ToLower() switch
                {
                    "cliente" => descendente ? query.OrderByDescending(r => r.Cliente.Apellidos).ThenByDescending(r => r.Cliente.Nombre) 
                                             : query.OrderBy(r => r.Cliente.Apellidos).ThenBy(r => r.Cliente.Nombre),
                    "fecha" => descendente ? query.OrderByDescending(r => r.FechaEmision) : query.OrderBy(r => r.FechaEmision),
                    "numero" => descendente ? query.OrderByDescending(r => r.NumeroRecibo) : query.OrderBy(r => r.NumeroRecibo),
                    _ => query.OrderBy(r => r.FechaEmision)
                };

                var recibos = await query.ToListAsync();

                _logger.LogDebug("Consulta de recibos ejecutada. Total encontrados: {TotalRecibos}", recibos.Count);

                var response = recibos.Select(recibo => new ReciboResponse
                {
                    NumeroRecibo = recibo.NumeroRecibo,
                    DniCliente = recibo.DniCliente,
                    NombreCliente = $"{recibo.Cliente.Nombre} {recibo.Cliente.Apellidos}",
                    Importe = recibo.Importe,
                    FechaEmision = recibo.FechaEmision
                }).ToList();

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar recibos");
                return StatusCode(500, $"Error al listar recibos: {ex.Message}");
            }
        }
    }
}