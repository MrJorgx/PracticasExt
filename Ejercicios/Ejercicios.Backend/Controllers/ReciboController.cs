using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ejercicios.Backend.Data;
using Ejercicios.Backend.Models;

namespace Ejercicios.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReciboController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReciboController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult<ReciboResponse>> CrearRecibo([FromBody] ReciboRequest request)
        {
            try
            {
                // Validar que no exista un recibo con el mismo número
                if (await _context.Recibos.AnyAsync(r => r.NumeroRecibo == request.NumeroRecibo))
                {
                    return BadRequest($"Ya existe un recibo con número {request.NumeroRecibo}");
                }

                // Validar que existe el cliente
                var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Dni == request.DniCliente);
                if (cliente == null)
                {
                    return BadRequest($"No existe cliente con DNI {request.DniCliente}");
                }

                // Validar cuota máxima para clientes REGISTRADO
                if (cliente.TipoCliente == TipoCliente.REGISTRADO)
                {
                    if (request.Importe > cliente.CuotaMaxima)
                    {
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

                // Recargar el recibo con el cliente incluido
                var reciboConCliente = await _context.Recibos
                    .Include(r => r.Cliente)
                    .FirstOrDefaultAsync(r => r.NumeroRecibo == recibo.NumeroRecibo);

                if (reciboConCliente == null || reciboConCliente.Cliente == null)
                {
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

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al crear recibo: {ex.Message}");
            }
        }

        [HttpGet("cliente/{dni}")]
        public async Task<ActionResult<List<ReciboResponse>>> ObtenerRecibosPorCliente(
            string dni, 
            [FromQuery] bool ordenarPorFecha = true, 
            [FromQuery] bool descendente = false)
        {
            try
            {
                // Validar que existe el cliente
                if (!await _context.Clientes.AnyAsync(c => c.Dni == dni))
                {
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
                return StatusCode(500, $"Error al obtener recibos del cliente: {ex.Message}");
            }
        }

        [HttpPut("{numeroRecibo}")]
        public async Task<ActionResult<ReciboResponse>> ActualizarRecibo(string numeroRecibo, [FromBody] ReciboUpdateRequest request)
        {
            try
            {
                var recibo = await _context.Recibos
                    .Include(r => r.Cliente)
                    .FirstOrDefaultAsync(r => r.NumeroRecibo == numeroRecibo);

                if (recibo == null)
                {
                    return NotFound($"No se encontró recibo con número {numeroRecibo}");
                }

                // Validar cuota máxima para clientes REGISTRADO
                if (recibo.Cliente.TipoCliente == TipoCliente.REGISTRADO)
                {
                    if (request.Importe > recibo.Cliente.CuotaMaxima)
                    {
                        return BadRequest($"El importe {request.Importe} supera la cuota máxima permitida de {recibo.Cliente.CuotaMaxima}");
                    }
                }

                recibo.Importe = request.Importe;
                recibo.FechaEmision = request.FechaEmision ?? recibo.FechaEmision;

                await _context.SaveChangesAsync();

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
                return StatusCode(500, $"Error al actualizar recibo: {ex.Message}");
            }
        }

        [HttpDelete("{numeroRecibo}")]
        public async Task<IActionResult> EliminarRecibo(string numeroRecibo)
        {
            try
            {
                var recibo = await _context.Recibos.FirstOrDefaultAsync(r => r.NumeroRecibo == numeroRecibo);

                if (recibo == null)
                {
                    return NotFound($"No se encontró recibo con número {numeroRecibo}");
                }

                _context.Recibos.Remove(recibo);
                await _context.SaveChangesAsync();

                return Ok($"Recibo {numeroRecibo} eliminado correctamente");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al eliminar recibo: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<ActionResult<List<ReciboResponse>>> ListarTodosLosRecibos(
            [FromQuery] string ordenarPor = "fecha", 
            [FromQuery] bool descendente = false)
        {
            try
            {
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
                return StatusCode(500, $"Error al listar recibos: {ex.Message}");
            }
        }
    }
}