using Gastos.Domain.Entities;

namespace Gastos.Domain.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<UsuarioModel> InserirUsuario(UsuarioModel usuarioModel, int UsuarioId);
        Task<IEnumerable<UsuarioModel>> ListarUsuarios();
        Task<UsuarioModel> ListarUsuarioPorId(int id);

        Task<UsuarioModel?> ObterUsuarioPorId(int id);
        Task<UsuarioModel> ListarUsuarioPorEmail(string email);
        Task<bool> AtualizarDadosUsuario(int usuarioId, string novoNome, string novoEmail, string novaSenha);
        Task<bool> AlterarSenha(int usuarioId, string novaSenhaHash);
    }
}
