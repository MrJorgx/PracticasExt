using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace Ejercicios.Frontend.Services
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly AuthService _authService;
        
        public CustomAuthenticationStateProvider(AuthService authService)
        {
            _authService = authService;
            _authService.AuthenticationStateChanged += OnAuthenticationStateChanged;
        }

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            ClaimsPrincipal user;
            
            if (_authService.IsAuthenticated && _authService.Usuario != null)
            {
                var claims = new[]
                {
                    new Claim(ClaimTypes.Name, _authService.Usuario.NombreUsuario),
                    new Claim(ClaimTypes.Email, _authService.Usuario.Email)
                };
                var identity = new ClaimsIdentity(claims, "custom");
                user = new ClaimsPrincipal(identity);
            }
            else
            {
                user = new ClaimsPrincipal(new ClaimsIdentity());
            }

            return Task.FromResult(new AuthenticationState(user));
        }

        private void OnAuthenticationStateChanged(bool isAuthenticated)
        {
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}