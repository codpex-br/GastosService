using Gastos.Domain.Entities;
using System.Security.Claims;

namespace Gastos.Application.Interfaces
{
    public interface IAuthService
    {
        Task<UsuarioModel> Authenticate(string email, string senha);

        Task<string> GenerateAccessToken(UsuarioModel usuarioModel);
        Task<string> GenerateRefreshToken(UsuarioModel usuarioModel);
        Task SaveRefreshToken(string token, DateTime expires, string createdByIp, int usuarioId);
        ClaimsPrincipal GetPrincipalFromRefreshToken(string token);

        Task<(UsuarioModel Usuario, RefreshToken Token)?> ValidateRefreshToken(string refreshToken, string ipAddress);
    }
}
