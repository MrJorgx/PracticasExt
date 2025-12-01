using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;

namespace Ejercicios.Frontend.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        
        public bool IsAuthenticated { get; private set; } = false;
        public Usuario? Usuario { get; private set; }
        public string? Token { get; private set; }
        
        public event Action<bool>? AuthenticationStateChanged;

        public AuthService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<(bool Success, string Message)> LoginAsync(LoginRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/login", request);
                
                if (response.IsSuccessStatusCode)
                {
                    var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
                    
                    if (loginResponse != null)
                    {
                        IsAuthenticated = true;
                        Token = loginResponse.Token;
                        Usuario = new Usuario 
                        { 
                            NombreUsuario = loginResponse.NombreUsuario,
                            Email = loginResponse.Email,
                            NombreCompleto = loginResponse.NombreCompleto,
                            FechaRegistro = loginResponse.FechaRegistro
                        };
                        
                        AuthenticationStateChanged?.Invoke(true);
                        return (true, "Login exitoso");
                    }
                }
                
                var error = await response.Content.ReadAsStringAsync();
                return (false, error);
            }
            catch (Exception ex)
            {
                return (false, $"Error de conexión: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> RegistrarAsync(RegistroRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/register", request);
                
                if (response.IsSuccessStatusCode)
                {
                    var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
                    
                    if (loginResponse != null)
                    {
                        IsAuthenticated = true;
                        Token = loginResponse.Token;
                        Usuario = new Usuario 
                        { 
                            NombreUsuario = loginResponse.NombreUsuario,
                            Email = loginResponse.Email,
                            NombreCompleto = loginResponse.NombreCompleto,
                            FechaRegistro = loginResponse.FechaRegistro
                        };
                        
                        AuthenticationStateChanged?.Invoke(true);
                        return (true, "Registro exitoso");
                    }
                }
                
                var error = await response.Content.ReadAsStringAsync();
                return (false, error);
            }
            catch (Exception ex)
            {
                return (false, $"Error de conexión: {ex.Message}");
            }
        }

        public async Task<bool> VerifyTokenAsync()
        {
            if (string.IsNullOrEmpty(Token))
                return false;
                
            try
            {
                var response = await _httpClient.GetAsync($"api/auth/verify?token={Token}");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task LogoutAsync()
        {
            IsAuthenticated = false;
            Usuario = null;
            Token = null;
            AuthenticationStateChanged?.Invoke(false);
            await Task.CompletedTask;
        }

        public void Logout()
        {
            IsAuthenticated = false;
            Usuario = null;
            Token = null;
            AuthenticationStateChanged?.Invoke(false);
        }
    }

    // Modelos para el frontend
    public class Usuario
    {
        public string NombreUsuario { get; set; } = "";
        public string Email { get; set; } = "";
        public string NombreCompleto { get; set; } = "";
        public DateTime FechaRegistro { get; set; } = DateTime.Now;
    }

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
        [MinLength(3, ErrorMessage = "El nombre debe tener al menos 3 caracteres")]
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
}