using Gastos.Domain.Entities;

namespace Gastos.Domain.Interfaces
{
    public interface IDespesaRepository
    {
        Task<DespesaModel> CriarDespesa(DespesaModel despesa);

        Task<IEnumerable<DespesaModel>> ObterDespesasPorUsuario(int usuarioId);

        Task<decimal> CalcularTotalDespesasMesAtual(int userId);

        Task<decimal> ObterPrevisaoMesSeguinte(int usuarioId);
    }
}
