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

        public async Task<(bool Success, string Message, bool RequiresTwoFactor)> LoginAsync(LoginRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/login", request);
                
                if (response.IsSuccessStatusCode)
                {
                    var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
                    
                    if (loginResponse != null)
                    {
                        // Si requiere 2FA, NO marcar como autenticado todavía
                        if (loginResponse.RequiresTwoFactor)
                        {
                            // Guardar temporalmente el usuario para la verificación 2FA
                            Usuario = new Usuario 
                            { 
                                NombreUsuario = loginResponse.NombreUsuario,
                                Email = loginResponse.Email,
                                NombreCompleto = loginResponse.NombreCompleto,
                                FechaRegistro = loginResponse.FechaRegistro
                            };
                            
                            return (true, "Código de verificación enviado a tu email", true);
                        }
                        
                        // Login normal sin 2FA
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
                        return (true, "Login exitoso", false);
                    }
                }
                
                var error = await response.Content.ReadAsStringAsync();
                return (false, error, false);
            }
            catch (Exception ex)
            {
                return (false, $"Error de conexión: {ex.Message}", false);
            }
        }

        public async Task<(bool Success, string Message)> VerifyTwoFactorAsync(VerifyTwoFactorRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/verify-2fa", request);
                
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
                        return (true, "Verificación exitosa");
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

        public async Task<(bool Success, string Message)> ResendTwoFactorCodeAsync(string email, string password)
        {
            try
            {
                var request = new LoginRequest
                {
                    Email = email,
                    Password = password
                };

                var response = await _httpClient.PostAsJsonAsync("api/auth/resend-2fa-code", request);
                
                if (response.IsSuccessStatusCode)
                {
                    return (true, "Nuevo código enviado a tu email");
                }
                
                var error = await response.Content.ReadAsStringAsync();
                return (false, error);
            }
            catch (Exception ex)
            {
                return (false, $"Error de conexión: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> Toggle2FAAsync(int userId, bool enable)
        {
            try
            {
                var request = new Enable2FARequest
                {
                    UserId = userId,
                    Enable = enable
                };

                var response = await _httpClient.PostAsJsonAsync("api/auth/toggle-2fa", request);
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<Toggle2FAResponse>();
                    return (true, result?.Message ?? "Estado de 2FA actualizado");
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
                // Aquí podrías implementar una verificación real del token
                return true;
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

        public async Task<(bool Success, string Message)> UpdateProfileAsync(UpdateProfileRequest request)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync("api/auth/update-profile", request);
                
                if (response.IsSuccessStatusCode)
                {
                    var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
                    
                    if (loginResponse != null)
                    {
                        Usuario = new Usuario 
                        { 
                            NombreUsuario = loginResponse.NombreUsuario,
                            Email = loginResponse.Email,
                            NombreCompleto = loginResponse.NombreCompleto,
                            FechaRegistro = loginResponse.FechaRegistro
                        };
                        
                        Token = loginResponse.Token;
                        AuthenticationStateChanged?.Invoke(true);
                        return (true, "Perfil actualizado correctamente");
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

        public async Task<(bool Success, string Message)> ChangePasswordAsync(ChangePasswordRequest request)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync("api/auth/change-password", request);
                
                if (response.IsSuccessStatusCode)
                {
                    return (true, "Contraseña cambiada correctamente");
                }
                
                var error = await response.Content.ReadAsStringAsync();
                return (false, error);
            }
            catch (Exception ex)
            {
                return (false, $"Error de conexión: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> DeleteAccountAsync(int userId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/auth/delete-account/{userId}");
                
                if (response.IsSuccessStatusCode)
                {
                    IsAuthenticated = false;
                    Usuario = null;
                    Token = null;
                    AuthenticationStateChanged?.Invoke(false);
                    
                    return (true, "Cuenta eliminada correctamente");
                }
                
                var error = await response.Content.ReadAsStringAsync();
                return (false, error);
            }
            catch (Exception ex)
            {
                return (false, $"Error de conexión: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> ForgotPasswordAsync(string email)
        {
            try
            {
                var request = new ForgotPasswordRequest { Email = email };
                var response = await _httpClient.PostAsJsonAsync("api/auth/forgot-password", request);
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<MessageResponse>();
                    return (true, result?.Message ?? "Email enviado");
                }
                
                var error = await response.Content.ReadAsStringAsync();
                return (false, error);
            }
            catch (Exception ex)
            {
                return (false, $"Error de conexión: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> ResetPasswordAsync(ResetPasswordRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/reset-password", request);
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<MessageResponse>();
                    return (true, result?.Message ?? "Contraseña restablecida");
                }
                
                var error = await response.Content.ReadAsStringAsync();
                return (false, error);
            }
            catch (Exception ex)
            {
                return (false, $"Error de conexión: {ex.Message}");
            }
        }

        public int? GetUserId()
        {
            if (Token != null && Token.StartsWith("token_"))
            {
                var parts = Token.Split('_');
                if (parts.Length >= 2 && int.TryParse(parts[1], out var userId))
                {
                    return userId;
                }
            }
            return null;
        }
    }

    // Modelos para el frontend
    public class Usuario
    {
        public string NombreUsuario { get; set; } = "";
        public string Email { get; set; } = "";
        public string NombreCompleto { get; set; } = "";
        public DateTime FechaRegistro { get; set; }
    }

    public class LoginRequest
    {
        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "La contraseña es requerida")]
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
        public bool RequiresTwoFactor { get; set; } = false;
        public bool TwoFactorEnabled { get; set; } = false;
    }

    public class VerifyTwoFactorRequest
    {
        [Required(ErrorMessage = "El email es requerido")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "El código es requerido")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "El código debe tener 6 dígitos")]
        public string Code { get; set; } = "";
    }

    public class Enable2FARequest
    {
        public int UserId { get; set; }
        public bool Enable { get; set; }
    }

    public class Toggle2FAResponse
    {
        public string Message { get; set; } = "";
        public bool TwoFactorEnabled { get; set; }
    }

    public class UpdateProfileRequest
    {
        [Required(ErrorMessage = "El ID de usuario es requerido")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "El nombre de usuario es requerido")]
        [MinLength(3, ErrorMessage = "El nombre debe tener al menos 3 caracteres")]
        public string NombreUsuario { get; set; } = "";

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        public string Email { get; set; } = "";
        
        public string NombreCompleto { get; set; } = "";
    }

    public class ChangePasswordRequest
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "La contraseña actual es requerida")]
        public string CurrentPassword { get; set; } = "";

        [Required(ErrorMessage = "La nueva contraseña es requerida")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
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

    public class MessageResponse
    {
        public string Message { get; set; } = "";
    }
}