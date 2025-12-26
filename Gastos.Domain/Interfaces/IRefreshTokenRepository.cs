using Gastos.Domain.Entities;

namespace Gastos.Domain.Interfaces
{
    public interface IRefreshTokenRepository
    {
        Task InserirRefreshToken(RefreshToken refreshToken);
        Task RevogarRefreshToken(int usuarioId, string revokedByIp, string reasonRevoked);
        Task RevogarToken(string token, string revokedByIp, string reason);

        Task<RefreshToken?> ObterRefreshToken(string token);
        Task AtualizarRefreshToken(RefreshToken refreshToken);
    }
}
