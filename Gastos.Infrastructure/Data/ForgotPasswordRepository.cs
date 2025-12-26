using Gastos.Domain.Entities;
using Gastos.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Gastos.Infrastructure.Data
{
    public class ForgotPasswordRepository : IForgotPasswordRepository
    {
        private readonly string _connectionString;

        public ForgotPasswordRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task SalvarCodigoRecuperacao(int usuarioId, string codigo)
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
            INSERT INTO RECUPERACAO_SENHA (USUARIO_ID, CODIGO, DATA_EXPIRACAO)
            VALUES (@UsuarioId, @Codigo, @DataExpiracao)";

            command.Parameters.AddWithValue("UsuarioId", usuarioId);
            command.Parameters.AddWithValue("Codigo", codigo);
            command.Parameters.AddWithValue("DataExpiracao", DateTime.Now.AddMinutes(2));

            await command.ExecuteNonQueryAsync();
        }

        public async Task<SolicitarCodigoModel> ObterUsuarioIdPorEmail(string email)
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT ID_USUARIO, EMAIL, NOME
                FROM USUARIOS 
                WHERE EMAIL ILIKE @Email";

            command.Parameters.AddWithValue("Email", email);

            await using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var nomeOriginal = reader.GetString(2);
                var nomeFormatado = string.Join(' ',
                    nomeOriginal
                        .ToLower()
                        .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                        .Select(p => char.ToUpper(p[0]) + p.Substring(1))
                );

                return new SolicitarCodigoModel
                {
                    Id = reader.GetInt32(0),
                    Email = reader.GetString(1),
                    Nome = nomeFormatado
                };
            }

            return null;
        }
    }
}
