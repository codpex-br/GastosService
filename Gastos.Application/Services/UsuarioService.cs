using Gastos.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace Gastos.Application.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UsuarioService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int GetCurrentUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (user == null || user.Identity?.IsAuthenticated != true)
                throw new InvalidOperationException("Usuário não autenticado.");

            var userIdClaim =
                user.FindFirst(ClaimTypes.NameIdentifier) ??
                user.FindFirst("sub") ??
                user.FindFirst("id") ??
                user.FindFirst("Id");

            if (userIdClaim == null)
            {
                throw new InvalidOperationException("Claim de ID do usuário ausente no token.");
            }

            var value = userIdClaim.Value?.Trim();

            if (!int.TryParse(value, out int userId))
                throw new InvalidOperationException($"Claim de ID inválida: {userIdClaim.Value}");
            
            return userId;
        } 
    }
}
