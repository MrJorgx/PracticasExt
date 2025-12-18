using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ejercicios.Backend.Data;
using Ejercicios.Backend.Models;
using Ejercicios.Backend.Services;
using System.Security.Cryptography;

namespace Ejercicios.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AuthController> _logger;
        private readonly IEmailService _emailService;

        public AuthController(AppDbContext context, ILogger<AuthController> logger, IEmailService emailService)
        {
            _context = context;
            _logger = logger;
            _emailService = emailService;
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

                // Si tiene 2FA habilitado, generar y enviar código
                if (usuario.TwoFactorEnabled)
                {
                    var code = PasswordHelper.GenerateTwoFactorCode();
                    usuario.TwoFactorCode = code;
                    usuario.TwoFactorCodeExpiry = DateTime.UtcNow.AddMinutes(10);
                    usuario.FailedTwoFactorAttempts = 0;
                    
                    await _context.SaveChangesAsync();

                    // Enviar código por email
                    await _emailService.SendTwoFactorCodeAsync(usuario.Email, usuario.NombreCompleto, code);

                    _logger.LogInformation("Código 2FA generado y enviado para usuario: {Email}", usuario.Email);

                    return Ok(new LoginResponse
                    {
                        Id = usuario.Id,
                        NombreUsuario = usuario.NombreUsuario,
                        Email = usuario.Email,
                        NombreCompleto = usuario.NombreCompleto,
                        Token = "",
                        FechaRegistro = usuario.FechaRegistro,
                        RequiresTwoFactor = true,
                        TwoFactorEnabled = true
                    });
                }

                // Login exitoso sin 2FA
                var response = new LoginResponse
                {
                    Id = usuario.Id,
                    NombreUsuario = usuario.NombreUsuario,
                    Email = usuario.Email,
                    NombreCompleto = usuario.NombreCompleto,
                    Token = $"token_{usuario.Id}_{DateTime.UtcNow.Ticks}",
                    FechaRegistro = usuario.FechaRegistro,
                    RequiresTwoFactor = false,
                    TwoFactorEnabled = false
                };

                _logger.LogInformation("Login exitoso para usuario: {NombreUsuario} ({Email})", usuario.NombreUsuario, usuario.Email);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en login para email: {Email}", request.Email);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPost("verify-2fa")]
        public async Task<ActionResult<LoginResponse>> VerifyTwoFactor([FromBody] VerifyTwoFactorRequest request)
        {
            try
            {
                _logger.LogInformation("Verificación 2FA para email: {Email}", request.Email);

                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());

                if (usuario == null)
                {
                    return Unauthorized("Usuario no encontrado");
                }

                // Verificar si el código ha expirado
                if (usuario.TwoFactorCodeExpiry == null || usuario.TwoFactorCodeExpiry < DateTime.UtcNow)
                {
                    _logger.LogWarning("Código 2FA expirado para usuario: {Email}", request.Email);
                    return BadRequest("El código ha expirado. Por favor, solicita uno nuevo.");
                }

                // Verificar límite de intentos fallidos
                if (usuario.FailedTwoFactorAttempts >= 3)
                {
                    _logger.LogWarning("Demasiados intentos fallidos de 2FA para usuario: {Email}", request.Email);
                    usuario.TwoFactorCode = null;
                    usuario.TwoFactorCodeExpiry = null;
                    await _context.SaveChangesAsync();
                    return BadRequest("Demasiados intentos fallidos. Por favor, inicia sesión nuevamente.");
                }

                // Verificar el código
                if (usuario.TwoFactorCode != request.Code)
                {
                    usuario.FailedTwoFactorAttempts++;
                    await _context.SaveChangesAsync();
                    
                    _logger.LogWarning("Código 2FA incorrecto para usuario: {Email}. Intentos fallidos: {Attempts}", 
                        request.Email, usuario.FailedTwoFactorAttempts);
                    
                    return BadRequest($"Código incorrecto. Intentos restantes: {3 - usuario.FailedTwoFactorAttempts}");
                }

                // Código correcto - limpiar datos 2FA
                usuario.TwoFactorCode = null;
                usuario.TwoFactorCodeExpiry = null;
                usuario.FailedTwoFactorAttempts = 0;
                await _context.SaveChangesAsync();

                var response = new LoginResponse
                {
                    Id = usuario.Id,
                    NombreUsuario = usuario.NombreUsuario,
                    Email = usuario.Email,
                    NombreCompleto = usuario.NombreCompleto,
                    Token = $"token_{usuario.Id}_{DateTime.UtcNow.Ticks}",
                    FechaRegistro = usuario.FechaRegistro,
                    RequiresTwoFactor = false,
                    TwoFactorEnabled = true
                };

                _logger.LogInformation("Verificación 2FA exitosa para usuario: {NombreUsuario}", usuario.NombreUsuario);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en verificación 2FA para email: {Email}", request.Email);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPost("resend-2fa-code")]
        public async Task<IActionResult> ResendTwoFactorCode([FromBody] LoginRequest request)
        {
            try
            {
                _logger.LogInformation("Reenvío de código 2FA solicitado para: {Email}", request.Email);

                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());

                if (usuario == null || !PasswordHelper.VerifyPassword(request.Password, usuario.PasswordHash))
                {
                    return Unauthorized("Credenciales inválidas");
                }

                if (!usuario.TwoFactorEnabled)
                {
                    return BadRequest("2FA no está habilitado para este usuario");
                }

                var code = PasswordHelper.GenerateTwoFactorCode();
                usuario.TwoFactorCode = code;
                usuario.TwoFactorCodeExpiry = DateTime.UtcNow.AddMinutes(10);
                usuario.FailedTwoFactorAttempts = 0;
                
                await _context.SaveChangesAsync();
                await _emailService.SendTwoFactorCodeAsync(usuario.Email, usuario.NombreCompleto, code);

                _logger.LogInformation("Código 2FA reenviado para usuario: {Email}", usuario.Email);
                return Ok(new { message = "Código enviado nuevamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reenviando código 2FA para: {Email}", request.Email);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPost("toggle-2fa")]
        public async Task<IActionResult> Toggle2FA([FromBody] Enable2FARequest request)
        {
            try
            {
                _logger.LogInformation("Solicitud de cambio de 2FA para usuario ID: {UserId}, Habilitar: {Enable}", 
                    request.UserId, request.Enable);

                var usuario = await _context.Usuarios.FindAsync(request.UserId);
                if (usuario == null)
                {
                    return NotFound("Usuario no encontrado");
                }

                usuario.TwoFactorEnabled = request.Enable;
                
                // Limpiar códigos existentes si se deshabilita
                if (!request.Enable)
                {
                    usuario.TwoFactorCode = null;
                    usuario.TwoFactorCodeExpiry = null;
                    usuario.FailedTwoFactorAttempts = 0;
                }
                
                await _context.SaveChangesAsync();

                _logger.LogInformation("2FA {Action} para usuario: {NombreUsuario}", 
                    request.Enable ? "habilitado" : "deshabilitado", usuario.NombreUsuario);

                return Ok(new { 
                    message = request.Enable ? "Autenticación de dos factores habilitada" : "Autenticación de dos factores deshabilitada",
                    twoFactorEnabled = usuario.TwoFactorEnabled
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cambiando estado de 2FA para usuario ID: {UserId}", request.UserId);
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

                var usuarioExistente = await _context.Usuarios
                    .AnyAsync(u => u.Email.ToLower() == request.Email.ToLower() || 
                                  u.NombreUsuario.ToLower() == request.NombreUsuario.ToLower());

                if (usuarioExistente)
                {
                    _logger.LogWarning("Intento de registro con email o usuario existente: {Email}, {NombreUsuario}", 
                        request.Email, request.NombreUsuario);
                    return BadRequest("Ya existe un usuario con ese email o nombre de usuario");
                }

                var usuario = new Usuario
                {
                    NombreUsuario = request.NombreUsuario.Trim(),
                    Email = request.Email.Trim().ToLower(),
                    PasswordHash = PasswordHelper.HashPassword(request.Password),
                    NombreCompleto = !string.IsNullOrWhiteSpace(request.NombreCompleto) ? 
                                    request.NombreCompleto.Trim() : 
                                    request.NombreUsuario.Trim(),
                    FechaRegistro = DateTime.UtcNow,
                    TwoFactorEnabled = false
                };

                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();

                // Enviar email de bienvenida (sin bloquear el registro)
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _emailService.SendWelcomeEmailAsync(usuario.Email, usuario.NombreCompleto);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error enviando email de bienvenida a {Email}", usuario.Email);
                    }
                });

                var response = new LoginResponse
                {
                    Id = usuario.Id,
                    NombreUsuario = usuario.NombreUsuario,
                    Email = usuario.Email,
                    NombreCompleto = usuario.NombreCompleto,
                    Token = $"token_{usuario.Id}_{DateTime.UtcNow.Ticks}",
                    FechaRegistro = usuario.FechaRegistro,
                    RequiresTwoFactor = false,
                    TwoFactorEnabled = false
                };

                _logger.LogInformation("Usuario registrado exitosamente: {NombreUsuario} ({Email})", 
                    usuario.NombreUsuario, usuario.Email);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en registro para: {NombreUsuario}, {Email}", 
                    request.NombreUsuario, request.Email);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPost("verify")]
        public async Task<ActionResult<bool>> Verify()
        {
            try
            {
                _logger.LogInformation("Verificación de token solicitada");
                return Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en verificación de token");
                return StatusCode(500, false);
            }
        }

        [HttpPut("update-profile")]
        public async Task<ActionResult<LoginResponse>> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            try
            {
                _logger.LogInformation("Actualización de perfil solicitada para usuario ID: {UserId}", request.UserId);

                var usuario = await _context.Usuarios.FindAsync(request.UserId);
                if (usuario == null)
                {
                    return NotFound("Usuario no encontrado");
                }

                // Verificar si el nuevo nombre de usuario ya existe
                if (request.NombreUsuario != usuario.NombreUsuario)
                {
                    var nombreExiste = await _context.Usuarios
                        .AnyAsync(u => u.NombreUsuario.ToLower() == request.NombreUsuario.ToLower() && u.Id != request.UserId);
                    
                    if (nombreExiste)
                    {
                        return BadRequest("El nombre de usuario ya está en uso");
                    }
                }

                // Verificar si el nuevo email ya existe
                if (request.Email != usuario.Email)
                {
                    var emailExiste = await _context.Usuarios
                        .AnyAsync(u => u.Email.ToLower() == request.Email.ToLower() && u.Id != request.UserId);
                    
                    if (emailExiste)
                    {
                        return BadRequest("El email ya está en uso");
                    }
                }

                usuario.NombreUsuario = request.NombreUsuario.Trim();
                usuario.Email = request.Email.Trim().ToLower();
                usuario.NombreCompleto = request.NombreCompleto?.Trim() ?? usuario.NombreUsuario;

                await _context.SaveChangesAsync();

                var response = new LoginResponse
                {
                    Id = usuario.Id,
                    NombreUsuario = usuario.NombreUsuario,
                    Email = usuario.Email,
                    NombreCompleto = usuario.NombreCompleto,
                    Token = $"token_{usuario.Id}_{DateTime.UtcNow.Ticks}",
                    FechaRegistro = usuario.FechaRegistro,
                    RequiresTwoFactor = false,
                    TwoFactorEnabled = usuario.TwoFactorEnabled
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

                if (!PasswordHelper.VerifyPassword(request.CurrentPassword, usuario.PasswordHash))
                {
                    _logger.LogWarning("Contraseña actual incorrecta para usuario ID: {UserId}", request.UserId);
                    return BadRequest("La contraseña actual es incorrecta");
                }

                usuario.PasswordHash = PasswordHelper.HashPassword(request.NewPassword);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Contraseña cambiada exitosamente para usuario: {NombreUsuario}", usuario.NombreUsuario);
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

                _context.Usuarios.Remove(usuario);
                await _context.SaveChangesAsync();

                _logger.LogWarning("Cuenta eliminada para usuario: {NombreUsuario} ({Email})", usuario.NombreUsuario, usuario.Email);
                return Ok(new { message = "Cuenta eliminada correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando cuenta para usuario ID: {UserId}", userId);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            try
            {
                _logger.LogInformation("Solicitud de recuperación de contraseña para: {Email}", request.Email);

                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());

                // Por seguridad, siempre devolver OK aunque el usuario no exista
                if (usuario == null)
                {
                    _logger.LogWarning("Solicitud de recuperación para email inexistente: {Email}", request.Email);
                    return Ok(new { message = "Si el email existe, recibirás instrucciones para restablecer tu contraseña" });
                }

                // Generar token único
                var resetToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
                usuario.PasswordResetToken = resetToken;
                usuario.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1); // Expira en 1 hora

                await _context.SaveChangesAsync();

                // Enviar email
                await _emailService.SendPasswordResetEmailAsync(usuario.Email, usuario.NombreCompleto, resetToken);

                _logger.LogInformation("Token de recuperación generado para usuario: {Email}", usuario.Email);
                
                return Ok(new { message = "Si el email existe, recibirás instrucciones para restablecer tu contraseña" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en solicitud de recuperación de contraseña para: {Email}", request.Email);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            try
            {
                _logger.LogInformation("Intento de reseteo de contraseña para: {Email}", request.Email);

                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());

                if (usuario == null)
                {
                    return BadRequest("Token inválido o expirado");
                }

                // Verificar token
                if (string.IsNullOrEmpty(usuario.PasswordResetToken) || usuario.PasswordResetToken != request.Token)
                {
                    _logger.LogWarning("Token inválido para usuario: {Email}", request.Email);
                    return BadRequest("Token inválido o expirado");
                }

                // Verificar expiración
                if (usuario.PasswordResetTokenExpiry == null || usuario.PasswordResetTokenExpiry < DateTime.UtcNow)
                {
                    _logger.LogWarning("Token expirado para usuario: {Email}", request.Email);
                    return BadRequest("Token inválido o expirado");
                }

                // Actualizar contraseña
                usuario.PasswordHash = PasswordHelper.HashPassword(request.NewPassword);
                
                // Limpiar token
                usuario.PasswordResetToken = null;
                usuario.PasswordResetTokenExpiry = null;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Contraseña restablecida exitosamente para usuario: {Email}", usuario.Email);
                
                return Ok(new { message = "Contraseña restablecida exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restableciendo contraseña para: {Email}", request.Email);
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}