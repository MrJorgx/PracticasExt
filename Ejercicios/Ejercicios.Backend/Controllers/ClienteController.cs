using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ejercicios.Backend.Data;
using Ejercicios.Backend.Models;
using System.Text.RegularExpressions;

namespace Ejercicios.Backend.Controllers
{
    /// <summary>
    /// Controlador para operaciones CRUDde clientes
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ClienteController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ClienteController(AppDbContext context)
        {
            _context = context;
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
        /// Crea un nuevo cliente en la base de datos
        /// </summary>
        /// <param name="request"></param>
        /// <returns>Datos del cliente creado con información adicional</returns>
        [HttpPost]
        public async Task<ActionResult<ClienteResponse>> CrearCliente([FromBody] ClienteRequest request)
        {
            try
            {
                // Normalizar DNI
                request.Dni = request.Dni?.Trim().ToUpper() ?? "";

                // Log para debugging
                Console.WriteLine($"Creando cliente: DNI={request.Dni}, Tipo={request.TipoCliente}");

                // Validar formato DNI
                if (!ValidarFormatoDni(request.Dni))
                {
                    return BadRequest("El DNI debe tener 8 dígitos y una letra.");
                }
                
                // Validar que no exista un cliente con el mismo DNI
                if (await _context.Clientes.AnyAsync(c => c.Dni == request.Dni))
                {
                    return BadRequest($"Ya existe un cliente con DNI {request.Dni}");
                }

                // Validar tipo de cliente
                if (!Enum.TryParse<TipoCliente>(request.TipoCliente, out var tipoCliente))
                {
                    return BadRequest("Tipo de cliente inválido. Debe ser REGISTRADO o SOCIO");
                }

                // Validar cuota máxima para clientes REGISTRADO
                if (tipoCliente == TipoCliente.REGISTRADO && request.CuotaMaxima == null)
                {
                    return BadRequest("Los clientes REGISTRADO deben tener una cuota máxima");
                }

                // Validar que clientes SOCIO no tienen cuota
                if (tipoCliente == TipoCliente.SOCIO && request.CuotaMaxima != null)
                {
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
                Console.WriteLine("Cliente guardado exitosamente.");
                
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
                Console.WriteLine($"Error completo al crear cliente: {ex}");
                Console.WriteLine($"Inner exception: {ex.InnerException?.Message}");
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
            // Pedimos el cliente en base al DNI
            try
            {
                // Normalizar DNI
                dni = dni?.Trim().ToUpper() ?? "";

                // Validar formato DNI
                if (!ValidarFormatoDni(dni))
                {
                    return BadRequest("El DNI debe tener 8 dígitos y una letra.");
                }

                var cliente = await _context.Clientes
                    .Include(c => c.Recibos)
                    .FirstOrDefaultAsync(c => c.Dni == dni);

                if (cliente == null)
                {
                    return NotFound($"No se encontró cliente con DNI {dni}");
                }

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
            // Actualizar cliente (datos)
            try
            {
                // Normalizar DNI
                dni = dni?.Trim().ToUpper() ?? "";

                // Validar formato DNI
                if (!ValidarFormatoDni(dni))
                {
                    return BadRequest("El DNI debe tener 8 dígitos y una letra.");
                }

                var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Dni == dni);

                if (cliente == null)
                {
                    return NotFound($"No se encontró cliente con DNI {dni}");
                }

                // Validar tipo de cliente
                if (!Enum.TryParse<TipoCliente>(request.TipoCliente, out var tipoCliente))
                {
                    return BadRequest("Tipo de cliente inválido. Debe ser REGISTRADO o SOCIO");
                }

                // Validar cuota máxima
                if (tipoCliente == TipoCliente.REGISTRADO && request.CuotaMaxima == null)
                {
                    return BadRequest("Los clientes REGISTRADO deben tener una cuota máxima");
                }

                if (tipoCliente == TipoCliente.SOCIO && request.CuotaMaxima != null)
                {
                    return BadRequest("Los clientes SOCIO no deben tener cuota máxima");
                }

                cliente.Nombre = request.Nombre;
                cliente.Apellidos = request.Apellidos;
                cliente.TipoCliente = tipoCliente;
                cliente.CuotaMaxima = request.CuotaMaxima;

                await _context.SaveChangesAsync();

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
            // Eliminar cliente
            try
            {
                // Normalizar DNI
                dni = dni?.Trim().ToUpper() ?? "";

                // Validar formato DNI
                if (!ValidarFormatoDni(dni))
                {
                    return BadRequest("El DNI debe tener 8 dígitos y una letra.");
                }

                var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Dni == dni);

                if (cliente == null)
                {
                    return NotFound($"No se encontró cliente con DNI {dni}");
                }

                _context.Clientes.Remove(cliente);
                await _context.SaveChangesAsync();

                return Ok($"Cliente con DNI {dni} eliminado correctamente");
            }
            catch (Exception ex)
            {
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
                var query = _context.Clientes.Include(c => c.Recibos).AsQueryable();

                query = ordenarPor.ToLower() switch
                {
                    "fechaalta" => descendente ? query.OrderByDescending(c => c.FechaAlta) : query.OrderBy(c => c.FechaAlta),
                    "dni" => descendente ? query.OrderByDescending(c => c.Dni) : query.OrderBy(c => c.Dni),
                    _ => query.OrderBy(c => c.Dni)
                };

                var clientes = await query.ToListAsync();

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
                return StatusCode(500, $"Error al listar clientes: {ex.Message}");
            }
        }
    }
}