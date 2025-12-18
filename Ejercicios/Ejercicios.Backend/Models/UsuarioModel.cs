using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;

namespace Ejercicios.Backend.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string NombreUsuario { get; set; } = "";
        
        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = "";
        
        [Required]
        public string PasswordHash { get; set; } = "";
        
        [StringLength(100)]
        public string NombreCompleto { get; set; } = "";
        
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

        public bool TwoFactorEnabled { get; set; } = false;
        public string? TwoFactorCode { get; set;}
        public DateTime? TwoFactorCodeExpiry { get; set; }
        public int FailedTwoFactorAttempts { get; set; } = 0;

        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpiry { get; set; }
    }

    // DTOs para autenticación
    public class LoginRequest
    {
        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "La contraseña es requerida")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string Password { get; set; } = "";
    }

    public class RegistroRequest
    {
        [Required(ErrorMessage = "El nombre de usuario es requerido")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre de usuario debe tener entre 3 y 50 caracteres")]
        public string NombreUsuario { get; set; } = "";

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "La contraseña es requerida")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string Password { get; set; } = "";

        [Required(ErrorMessage = "Confirma tu contraseña")]
        [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmPassword { get; set; } = "";

        public string NombreCompleto { get; set; } = "";
    }

    public class LoginResponse
    {
        public int Id { get; set; }
        public string NombreUsuario { get; set; } = "";
        public string Email { get; set; } = "";
        public string NombreCompleto { get; set; } = "";
        public string Token { get; set; } = "";
        public DateTime FechaRegistro { get; set; }
        public bool RequiresTwoFactor { get; set; } = false;
        public bool TwoFactorEnabled { get; set; } = false;
    }

    
    public class VerifyTwoFactorRequest
    {
        [Required(ErrorMessage = "El email es requerido")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "El código de verificación es requerido")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "El código debe tener 6 dígitos")]
        public string Code { get; set; } = "";
    }

    public class Enable2FARequest
    {
        [Required(ErrorMessage = "El ID de usuario es requerido")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Se requiere habilitar o deshabilitar 2FA")]
        public bool Enable { get; set; }
    }

    public static class PasswordHelper
    {
        public static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "SaltEjercicios2024"));
            return Convert.ToBase64String(hashedBytes);
        }

        public static bool VerifyPassword(string password, string hash)
        {
            return HashPassword(password) == hash;
        }

        public static string GenerateTwoFactorCode()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[4];
            rng.GetBytes(bytes);
            var code = BitConverter.ToUInt32(bytes, 0) % 1000000;
            return code.ToString("D6");
        }
    }

    public class UpdateProfileRequest
    {
        [Required(ErrorMessage = "El ID de usuario es requerido")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "El nombre de usuario es requerido")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre de usuario debe tener entre 3 y 50 caracteres")]
        public string NombreUsuario { get; set; } = "";

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        public string Email { get; set; } = "";

        public string NombreCompleto { get; set; } = "";
    }

    public class ChangePasswordRequest
    {
        [Required(ErrorMessage = "El ID de usuario es requerido")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "La contraseña actual es requerida")]
        public string CurrentPassword { get; set; } = "";

        [Required(ErrorMessage = "La nueva contraseña es requerida")]
        [MinLength(6, ErrorMessage = "La nueva contraseña debe tener al menos 6 caracteres")]
        public string NewPassword { get; set; } = "";

        [Required(ErrorMessage = "Confirma la nueva contraseña")]
        [Compare(nameof(NewPassword), ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmNewPassword { get; set; } = "";
    }

    public class ForgotPasswordRequest
    {
        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        public string Email { get; set; } = "";
    }

    public class ResetPasswordRequest
    {
        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "El token es requerido")]
        public string Token { get; set; } = "";

        [Required(ErrorMessage = "La nueva contraseña es requerida")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string NewPassword { get; set; } = "";

        [Required(ErrorMessage = "Confirma la nueva contraseña")]
        [Compare(nameof(NewPassword), ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmNewPassword { get; set; } = "";
    }
}