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
    }
}