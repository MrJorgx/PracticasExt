using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ejercicios.Backend.Data;
using Ejercicios.Backend.Models;

namespace Ejercicios.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AuthController> _logger;

        public AuthController(AppDbContext context, ILogger<AuthController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            try
            {
                _logger.LogInformation("Intento de login para email: {Email}", request.Email);

                if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                {
                    return BadRequest("Email y contraseña son requeridos");
                }

                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());

                if (usuario == null)
                {
                    _logger.LogWarning("Intento de login con email inexistente: {Email}", request.Email);
                    return Unauthorized("Credenciales inválidas");
                }

                if (!PasswordHelper.VerifyPassword(request.Password, usuario.PasswordHash))
                {
                    _logger.LogWarning("Intento de login con contraseña incorrecta para: {Email}", request.Email);
                    return Unauthorized("Credenciales inválidas");
                }

                var response = new LoginResponse
                {
                    Id = usuario.Id,
                    NombreUsuario = usuario.NombreUsuario,
                    Email = usuario.Email,
                    NombreCompleto = usuario.NombreCompleto,
                    Token = $"token_{usuario.Id}_{DateTime.UtcNow.Ticks}", // Token simplificado
                    FechaRegistro = usuario.FechaRegistro
                };

                _logger.LogInformation("Login exitoso para usuario: {NombreUsuario} ({Email})", usuario.NombreUsuario, usuario.Email);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante el login para email: {Email}", request?.Email);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPost("register")]
        public async Task<ActionResult<LoginResponse>> Register([FromBody] RegistroRequest request)
        {
            try
            {
                _logger.LogInformation("Intento de registro para usuario: {NombreUsuario}, Email: {Email}", 
                    request.NombreUsuario, request.Email);

                if (request == null)
                {
                    return BadRequest("Datos de registro requeridos");
                }

                // Verificar si el usuario ya existe
                var usuarioExistente = await _context.Usuarios
                    .AnyAsync(u => u.Email.ToLower() == request.Email.ToLower() || 
                                  u.NombreUsuario.ToLower() == request.NombreUsuario.ToLower());

                if (usuarioExistente)
                {
                    _logger.LogWarning("Intento de registro con email o usuario existente: {Email}, {NombreUsuario}", 
                        request.Email, request.NombreUsuario);
                    return BadRequest("Ya existe un usuario con ese email o nombre de usuario");
                }

                // Crear nuevo usuario
                var usuario = new Usuario
                {
                    NombreUsuario = request.NombreUsuario.Trim(),
                    Email = request.Email.Trim().ToLower(),
                    PasswordHash = PasswordHelper.HashPassword(request.Password),
                    NombreCompleto = !string.IsNullOrWhiteSpace(request.NombreCompleto) ? 
                                    request.NombreCompleto.Trim() : 
                                    request.NombreUsuario.Trim(),
                    FechaRegistro = DateTime.UtcNow
                };

                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();

                var response = new LoginResponse
                {
                    Id = usuario.Id,
                    NombreUsuario = usuario.NombreUsuario,
                    Email = usuario.Email,
                    NombreCompleto = usuario.NombreCompleto,
                    Token = $"token_{usuario.Id}_{DateTime.UtcNow.Ticks}",
                    FechaRegistro = usuario.FechaRegistro
                };

                _logger.LogInformation("Usuario registrado exitosamente: {NombreUsuario} ({Email}) con ID: {Id}", 
                    usuario.NombreUsuario, usuario.Email, usuario.Id);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante el registro para usuario: {NombreUsuario}, Email: {Email}", 
                    request?.NombreUsuario, request?.Email);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("verify")]
        public async Task<ActionResult<LoginResponse>> VerifyToken([FromQuery] string token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token) || !token.StartsWith("token_"))
                {
                    return Unauthorized("Token inválido");
                }

                var parts = token.Split('_');
                if (parts.Length != 3 || !int.TryParse(parts[1], out var userId))
                {
                    return Unauthorized("Token inválido");
                }

                var usuario = await _context.Usuarios.FindAsync(userId);
                if (usuario == null)
                {
                    return Unauthorized("Usuario no encontrado");
                }

                var response = new LoginResponse
                {
                    Id = usuario.Id,
                    NombreUsuario = usuario.NombreUsuario,
                    Email = usuario.Email,
                    NombreCompleto = usuario.NombreCompleto,
                    Token = token,
                    FechaRegistro = usuario.FechaRegistro
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verificando token: {Token}", token);
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}