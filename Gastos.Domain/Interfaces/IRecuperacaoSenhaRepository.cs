using Gastos.Domain.Entities;

namespace Gastos.Domain.Interfaces
{
    public interface IRecuperacaoSenhaRepository
    {
        Task SalvarCodigoRecuperacao(int usuarioId, string codigo);
        Task CriarCodigo(int usuarioId, string codigo, DateTime dataExpiracao);
        Task SalvarTokenTemporario(int id, string token, DateTime expiracao);
        Task<RecuperacaoSenhaModel?> ValidarTokenTemporario(string token);
        Task MarcarTokenComoUsado(string token);
        Task<RecuperacaoSenhaModel?> ValidarCodigo(int usuarioId, string codigo);
        Task MarcarCodigoComoUsado(int id);
    }
}
