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

        [HttpPut("update-profile")]
        public async Task<ActionResult<LoginResponse>> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            try
            {
                _logger.LogInformation("Actualizando perfil para usuario ID: {UserId}", request.UserId);

                var usuario = await _context.Usuarios.FindAsync(request.UserId);
                if (usuario == null)
                {
                    return NotFound("Usuario no encontrado");
                }

                // Verificar si el nuevo nombre de usuario ya existe (si cambió)
                if (usuario.NombreUsuario != request.NombreUsuario)
                {
                    var existeUsuario = await _context.Usuarios
                        .AnyAsync(u => u.NombreUsuario.ToLower() == request.NombreUsuario.ToLower() && u.Id != request.UserId);
                    
                    if (existeUsuario)
                    {
                        return BadRequest("Ya existe un usuario con ese nombre de usuario");
                    }
                }

                // Verificar si el nuevo email ya existe (si cambió)
                if (usuario.Email != request.Email.ToLower())
                {
                    var existeEmail = await _context.Usuarios
                        .AnyAsync(u => u.Email.ToLower() == request.Email.ToLower() && u.Id != request.UserId);
                    
                    if (existeEmail)
                    {
                        return BadRequest("Ya existe un usuario con ese email");
                    }
                }

                // Actualizar datos
                usuario.NombreUsuario = request.NombreUsuario.Trim();
                usuario.Email = request.Email.Trim().ToLower();
                usuario.NombreCompleto = !string.IsNullOrWhiteSpace(request.NombreCompleto) ? 
                                        request.NombreCompleto.Trim() : 
                                        request.NombreUsuario.Trim();

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

                _logger.LogInformation("Perfil actualizado exitosamente para usuario: {NombreUsuario}", usuario.NombreUsuario);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando perfil para usuario ID: {UserId}", request.UserId);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                _logger.LogInformation("Cambio de contraseña solicitado para usuario ID: {UserId}", request.UserId);

                var usuario = await _context.Usuarios.FindAsync(request.UserId);
                if (usuario == null)
                {
                    return NotFound("Usuario no encontrado");
                }

                // Verificar contraseña actual
                if (!PasswordHelper.VerifyPassword(request.CurrentPassword, usuario.PasswordHash))
                {
                    _logger.LogWarning("Intento de cambio de contraseña con contraseña incorrecta para usuario ID: {UserId}", request.UserId);
                    return BadRequest("La contraseña actual es incorrecta");
                }

                // Actualizar contraseña
                usuario.PasswordHash = PasswordHelper.HashPassword(request.NewPassword);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Contraseña actualizada exitosamente para usuario: {NombreUsuario}", usuario.NombreUsuario);
                return Ok(new { message = "Contraseña actualizada correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cambiando contraseña para usuario ID: {UserId}", request.UserId);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpDelete("delete-account/{userId}")]
        public async Task<IActionResult> DeleteAccount(int userId)
        {
            try
            {
                _logger.LogWarning("Solicitud de eliminación de cuenta para usuario ID: {UserId}", userId);

                var usuario = await _context.Usuarios.FindAsync(userId);
                if (usuario == null)
                {
                    return NotFound("Usuario no encontrado");
                }

                var nombreUsuario = usuario.NombreUsuario;
                
                _context.Usuarios.Remove(usuario);
                await _context.SaveChangesAsync();

                _logger.LogWarning("Cuenta eliminada permanentemente para usuario: {NombreUsuario} (ID: {UserId})", 
                    nombreUsuario, userId);
                
                return Ok(new { message = "Cuenta eliminada correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando cuenta para usuario ID: {UserId}", userId);
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}