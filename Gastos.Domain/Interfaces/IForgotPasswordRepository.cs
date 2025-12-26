using Gastos.Domain.Entities;

namespace Gastos.Domain.Interfaces
{
    public interface IForgotPasswordRepository
    {
        Task SalvarCodigoRecuperacao(int usuarioId, string codigo);
        Task<SolicitarCodigoModel> ObterUsuarioIdPorEmail(string email);
    }
}
