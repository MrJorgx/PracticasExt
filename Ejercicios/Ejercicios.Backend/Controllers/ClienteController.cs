using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ejercicios.Backend.Data;
using Ejercicios.Backend.Models;
using System.Text.RegularExpressions;

namespace Ejercicios.Backend.Controllers
{
    /// <summary>
    /// Controlador para operaciones CRUD de clientes
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ClienteController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ClienteController> _logger;

        public ClienteController(AppDbContext context, ILogger<ClienteController> logger)
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
        /// Valida que el NIE tenga el formato correcto (1 letra (X, Y o Z), 7 dígitos y 1 letra)
        /// </summary>
        /// <param name="nie"></param>
        /// <returns>True si el formato es válido, False en caso contrario</returns>
        private bool ValidarFormatoNie(string nie)
        {
            if (string.IsNullOrWhiteSpace(nie))
                return false;

            nie = nie.Trim().ToUpper();
            var patron = @"^[XYZ][0-9]{7}[A-Z]$";
            return Regex.IsMatch(nie, patron);
        }

        /// <summary>
        /// Crea un nuevo cliente en la base de datos
        /// </summary>
        /// <param name="request"></param>
        /// <returns>Datos del cliente creado con información adicional</returns>
        [HttpPost]
        public async Task<ActionResult<ClienteResponse>> CrearCliente([FromBody] ClienteRequest request)
        {
            try
            {
                _logger.LogInformation("Iniciando creación de cliente");

                if (request == null)
                {
                    _logger.LogWarning("Solicitud de creación de cliente recibida sin datos");
                    return BadRequest("Los datos de entrada son requeridos");
                }

                // Normalizar DNI
                request.Dni = request.Dni?.Trim().ToUpper() ?? "";

                _logger.LogInformation("Creando cliente: DNI={DNI}, Tipo={TipoCliente}", 
                    request.Dni, request.TipoCliente);

                // Validar formato DNI
                if (!ValidarFormatoDni(request.Dni))
                {
                    _logger.LogWarning("Validación fallida: formato de DNI incorrecto - {DNI}", request.Dni);
                    return BadRequest("El DNI debe tener 8 dígitos y una letra.");
                }
                
                // Validar que no exista un cliente con el mismo DNI
                if (await _context.Clientes.AnyAsync(c => c.Dni == request.Dni))
                {
                    _logger.LogWarning("Intento de crear cliente con DNI duplicado: {DNI}", request.Dni);
                    return BadRequest($"Ya existe un cliente con DNI {request.Dni}");
                }

                // Validar tipo de cliente
                if (!Enum.TryParse<TipoCliente>(request.TipoCliente, out var tipoCliente))
                {
                    _logger.LogWarning("Tipo de cliente inválido recibido: {TipoCliente}", request.TipoCliente);
                    return BadRequest("Tipo de cliente inválido. Debe ser REGISTRADO o SOCIO");
                }

                // Validar cuota máxima para clientes REGISTRADO
                if (tipoCliente == TipoCliente.REGISTRADO && request.CuotaMaxima == null)
                {
                    _logger.LogWarning("Cliente REGISTRADO sin cuota máxima: {DNI}", request.Dni);
                    return BadRequest("Los clientes REGISTRADO deben tener una cuota máxima");
                }

                // Validar que clientes SOCIO no tienen cuota
                if (tipoCliente == TipoCliente.SOCIO && request.CuotaMaxima != null)
                {
                    _logger.LogWarning("Cliente SOCIO con cuota máxima: {DNI}", request.Dni);
                    return BadRequest("Los clientes SOCIO no deben tener cuota máxima");
                }

                var cliente = new Cliente
                {
                    Dni = request.Dni,
                    Nombre = request.Nombre,
                    Apellidos = request.Apellidos,
                    TipoCliente = tipoCliente,
                    CuotaMaxima = request.CuotaMaxima,
                    FechaAlta = DateTime.UtcNow
                };

                _context.Clientes.Add(cliente);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Cliente creado exitosamente: {DNI} - {Nombre} {Apellidos}, Tipo: {TipoCliente}", 
                    cliente.Dni, cliente.Nombre, cliente.Apellidos, cliente.TipoCliente);
                
                var response = new ClienteResponse
                {
                    Dni = cliente.Dni,
                    Nombre = cliente.Nombre,
                    Apellidos = cliente.Apellidos,
                    TipoCliente = cliente.TipoCliente.ToString(),
                    CuotaMaxima = cliente.CuotaMaxima,
                    FechaAlta = cliente.FechaAlta.ToLocalTime(),
                    TotalRecibos = 0
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear cliente: DNI={DNI}, Nombre={Nombre}", 
                    request?.Dni, request?.Nombre);
                return StatusCode(500, $"Error al crear cliente: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtiene un cliente específico por su DNI
        /// </summary>
        /// <param name="dni"></param>
        /// <returns>Datos completos del cliente incluyendo el total de recibos</returns>
        [HttpGet("{dni}")]
        public async Task<ActionResult<ClienteResponse>> ObtenerCliente(string dni)
        {
            try
            {
                _logger.LogInformation("Buscando cliente por DNI: {DNI}", dni);

                // Normalizar DNI
                dni = dni?.Trim().ToUpper() ?? "";

                // Validar formato DNI
                if (!ValidarFormatoDni(dni))
                {
                    _logger.LogWarning("Formato de DNI inválido en búsqueda: {DNI}", dni);
                    return BadRequest("El DNI debe tener 8 dígitos y una letra.");
                }

                var cliente = await _context.Clientes
                    .Include(c => c.Recibos)
                    .FirstOrDefaultAsync(c => c.Dni == dni);

                if (cliente == null)
                {
                    _logger.LogWarning("Cliente no encontrado: {DNI}", dni);
                    return NotFound($"No se encontró cliente con DNI {dni}");
                }

                _logger.LogDebug("Cliente encontrado: {DNI} - {Nombre} {Apellidos}, Total recibos: {TotalRecibos}", 
                    cliente.Dni, cliente.Nombre, cliente.Apellidos, cliente.Recibos.Count);

                var response = new ClienteResponse
                {
                    Dni = cliente.Dni,
                    Nombre = cliente.Nombre,
                    Apellidos = cliente.Apellidos,
                    TipoCliente = cliente.TipoCliente.ToString(),
                    CuotaMaxima = cliente.CuotaMaxima,
                    FechaAlta = cliente.FechaAlta,
                    TotalRecibos = cliente.Recibos.Count
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener cliente con DNI: {DNI}", dni);
                return StatusCode(500, $"Error al obtener cliente: {ex.Message}");
            }
        }

        /// <summary>
        /// Actualiza los datos de un cliente existente (el DNI no se puede cambiar)
        /// </summary>
        /// <param name="dni"></param>
        /// <param name="request"></param>
        /// <returns>Datos actualizados del cliente</returns>
        [HttpPut("{dni}")]
        public async Task<ActionResult<ClienteResponse>> ActualizarCliente(string dni, [FromBody] ClienteUpdateRequest request)
        {
            try
            {
                _logger.LogInformation("Iniciando actualización de cliente: {DNI}", dni);

                if (request == null)
                {
                    _logger.LogWarning("Solicitud de actualización de cliente recibida sin datos");
                    return BadRequest("Los datos de entrada son requeridos");
                }

                // Normalizar DNI
                dni = dni?.Trim().ToUpper() ?? "";

                // Validar formato DNI
                if (!ValidarFormatoDni(dni))
                {
                    _logger.LogWarning("Formato de DNI inválido en actualización: {DNI}", dni);
                    return BadRequest("El DNI debe tener 8 dígitos y una letra.");
                }

                var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Dni == dni);

                if (cliente == null)
                {
                    _logger.LogWarning("Cliente no encontrado para actualización: {DNI}", dni);
                    return NotFound($"No se encontró cliente con DNI {dni}");
                }

                // Validar tipo de cliente
                if (!Enum.TryParse<TipoCliente>(request.TipoCliente, out var tipoCliente))
                {
                    _logger.LogWarning("Tipo de cliente inválido en actualización: {TipoCliente}", request.TipoCliente);
                    return BadRequest("Tipo de cliente inválido. Debe ser REGISTRADO o SOCIO");
                }

                // Validar cuota máxima
                if (tipoCliente == TipoCliente.REGISTRADO && request.CuotaMaxima == null)
                {
                    _logger.LogWarning("Actualización: Cliente REGISTRADO sin cuota máxima: {DNI}", dni);
                    return BadRequest("Los clientes REGISTRADO deben tener una cuota máxima");
                }

                if (tipoCliente == TipoCliente.SOCIO && request.CuotaMaxima != null)
                {
                    _logger.LogWarning("Actualización: Cliente SOCIO con cuota máxima: {DNI}", dni);
                    return BadRequest("Los clientes SOCIO no deben tener cuota máxima");
                }

                var datosAnteriores = $"{cliente.Nombre} {cliente.Apellidos} ({cliente.TipoCliente})";

                cliente.Nombre = request.Nombre;
                cliente.Apellidos = request.Apellidos;
                cliente.TipoCliente = tipoCliente;
                cliente.CuotaMaxima = request.CuotaMaxima;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Cliente actualizado exitosamente: {DNI}. Anterior: {DatosAnteriores}, Nuevo: {Nombre} {Apellidos} ({TipoCliente})", 
                    dni, datosAnteriores, cliente.Nombre, cliente.Apellidos, cliente.TipoCliente);

                var response = new ClienteResponse
                {
                    Dni = cliente.Dni,
                    Nombre = cliente.Nombre,
                    Apellidos = cliente.Apellidos,
                    TipoCliente = cliente.TipoCliente.ToString(),
                    CuotaMaxima = cliente.CuotaMaxima,
                    FechaAlta = cliente.FechaAlta,
                    TotalRecibos = await _context.Recibos.CountAsync(r => r.DniCliente == dni)
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar cliente: {DNI}", dni);
                return StatusCode(500, $"Error al actualizar cliente: {ex.Message}");
            }
        }

        /// <summary>
        /// Elimina un cliente de la base de datos
        /// </summary>
        /// <param name="dni"></param>
        /// <returns>Mensaje de confirmación de la eliminación</returns>
        [HttpDelete("{dni}")]
        public async Task<IActionResult> EliminarCliente(string dni)
        {
            try
            {
                _logger.LogInformation("Iniciando eliminación de cliente: {DNI}", dni);

                // Normalizar DNI
                dni = dni?.Trim().ToUpper() ?? "";

                // Validar formato DNI
                if (!ValidarFormatoDni(dni))
                {
                    _logger.LogWarning("Formato de DNI inválido en eliminación: {DNI}", dni);
                    return BadRequest("El DNI debe tener 8 dígitos y una letra.");
                }

                var cliente = await _context.Clientes
                    .Include(c => c.Recibos)
                    .FirstOrDefaultAsync(c => c.Dni == dni);

                if (cliente == null)
                {
                    _logger.LogWarning("Cliente no encontrado para eliminación: {DNI}", dni);
                    return NotFound($"No se encontró cliente con DNI {dni}");
                }

                var nombreCompleto = $"{cliente.Nombre} {cliente.Apellidos}";
                var totalRecibos = cliente.Recibos.Count;

                _context.Clientes.Remove(cliente);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Cliente eliminado exitosamente: {DNI} - {NombreCompleto}, Recibos eliminados: {TotalRecibos}", 
                    dni, nombreCompleto, totalRecibos);

                return Ok($"Cliente con DNI {dni} eliminado correctamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar cliente: {DNI}", dni);
                return StatusCode(500, $"Error al eliminar cliente: {ex.Message}");
            }
        }

        /// <summary>
        /// Lista todos los clientes con opciones de ordenamiento
        /// </summary>
        /// <param name="ordenarPor"></param>
        /// <param name="descendente"></param>
        /// <returns>Lista de todos los clientes con su información y total de recibos</returns>
        [HttpGet]
        public async Task<ActionResult<List<ClienteResponse>>> ListarClientes(
            [FromQuery] string ordenarPor = "dni", 
            [FromQuery] bool descendente = false)
        {
            try
            {
                _logger.LogInformation("Listando clientes. Ordenar por: {OrdenarPor}, Descendente: {Descendente}", 
                    ordenarPor, descendente);

                var query = _context.Clientes.Include(c => c.Recibos).AsQueryable();

                query = ordenarPor.ToLower() switch
                {
                    "fechaalta" => descendente ? query.OrderByDescending(c => c.FechaAlta) : query.OrderBy(c => c.FechaAlta),
                    "dni" => descendente ? query.OrderByDescending(c => c.Dni) : query.OrderBy(c => c.Dni),
                    _ => query.OrderBy(c => c.Dni)
                };

                var clientes = await query.ToListAsync();

                _logger.LogDebug("Consulta de clientes ejecutada. Total encontrados: {TotalClientes}", clientes.Count);

                var response = clientes.Select(cliente => new ClienteResponse
                {
                    Dni = cliente.Dni,
                    Nombre = cliente.Nombre,
                    Apellidos = cliente.Apellidos,
                    TipoCliente = cliente.TipoCliente.ToString(),
                    CuotaMaxima = cliente.CuotaMaxima,
                    FechaAlta = cliente.FechaAlta,
                    TotalRecibos = cliente.Recibos.Count
                }).ToList();

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar clientes");
                return StatusCode(500, $"Error al listar clientes: {ex.Message}");
            }
        }
    }
}