using Gastos.Domain.Entities;
using Gastos.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Gastos.Infrastructure.Data
{
    public class DespesaRepository : IDespesaRepository
    {
        private readonly string _connectionString;

        public DespesaRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' não encontrada.");
        }

        public async Task<DespesaModel> CriarDespesa(DespesaModel despesaModel)
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var transaction = await connection.BeginTransactionAsync();

            try 
            {
                var insertDespesa = new NpgsqlCommand(@"
                    INSERT INTO DESPESAS (
                    NOME, VALOR_TOTAL, DATA_DESPESA, TIPO_DESPESA, TIPO_TRANSACAO, 
                    PARCELAS_TOTAIS, DATA_PRIMEIRA_PARCELA, DATA_ULTIMA_ATIVIDADE_CONFIRMADA, 
                    FK_ID_USUARIO, FK_ID_CATEGORIA, FK_ID_FORMA_PAGAMENTO
                    )
                    VALUES (
                        @Nome, @ValorTotal, @DataDespesa, @TipoDespesa, @TipoTransacao, 
                        @ParcelasTotais, @DataPrimeiraParcela, @DataUltimaAtividadeConfirmada,
                        @FkIdUsuario, @FkIdCategoria, @FkIdFormaPagamento
                    )
                    RETURNING ID_DESPESA;",
                    connection,
                    transaction
                );

                insertDespesa.Parameters.AddWithValue("@Nome", despesaModel.Nome.ToUpper());
                insertDespesa.Parameters.AddWithValue("@ValorTotal", despesaModel.ValorTotal);
                insertDespesa.Parameters.AddWithValue("@DataDespesa", despesaModel.DataDespesa);
                insertDespesa.Parameters.AddWithValue("@TipoDespesa", despesaModel.TipoDespesa.ToUpper());
                insertDespesa.Parameters.AddWithValue("@TipoTransacao", despesaModel.TipoTransacao.ToUpper());
                insertDespesa.Parameters.AddWithValue("@ParcelasTotais", (object)despesaModel.ParcelasTotais ?? DBNull.Value);
                insertDespesa.Parameters.AddWithValue("@DataPrimeiraParcela", (object)despesaModel.DataPrimeiraParcela ?? DBNull.Value);
                insertDespesa.Parameters.AddWithValue("@DataUltimaAtividadeConfirmada", (object)despesaModel.DataUltimaAtividadeConfirmada ?? DBNull.Value);

                insertDespesa.Parameters.AddWithValue("@FkIdUsuario", despesaModel.FkIdUsuario);
                insertDespesa.Parameters.AddWithValue("@FkIdCategoria", despesaModel.FkIdCategoria);
                insertDespesa.Parameters.AddWithValue("@FkIdFormaPagamento", despesaModel.FkIdFormaPagamento);

                var idDespesa = await insertDespesa.ExecuteScalarAsync();

                if (idDespesa != null)
                {
                    despesaModel.Id = Convert.ToInt32(idDespesa);
                }

                despesaModel.Id = Convert.ToInt32(idDespesa);

                await transaction.CommitAsync();
                return despesaModel;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }  
        }
        public async Task<IEnumerable<DespesaModel>> ObterDespesasPorUsuario(int usuarioId)
        {
            var despesas = new List<DespesaModel>();

            const string query = @"
            SELECT 
                ID_DESPESA, NOME, VALOR_TOTAL, DATA_DESPESA, TIPO_DESPESA, TIPO_TRANSACAO, 
                PARCELAS_TOTAIS, DATA_PRIMEIRA_PARCELA, DATA_ULTIMA_ATIVIDADE_CONFIRMADA, 
                FK_ID_USUARIO, FK_ID_CATEGORIA, FK_ID_FORMA_PAGAMENTO
            FROM DESPESAS
            WHERE FK_ID_USUARIO = @usuarioId
            ORDER BY DATA_DESPESA DESC;";

            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@usuarioId", usuarioId);

            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                despesas.Add(new DespesaModel
                {
                    Id = reader.GetInt32(reader.GetOrdinal("ID_DESPESA")),
                    Nome = reader.GetString(reader.GetOrdinal("NOME")),
                    ValorTotal = reader.GetDecimal(reader.GetOrdinal("VALOR_TOTAL")),
                    DataDespesa = reader.GetDateTime(reader.GetOrdinal("DATA_DESPESA")),
                    TipoDespesa = reader.GetString(reader.GetOrdinal("TIPO_DESPESA")),
                    TipoTransacao = reader.GetString(reader.GetOrdinal("TIPO_TRANSACAO")),

                    ParcelasTotais = reader.IsDBNull(reader.GetOrdinal("PARCELAS_TOTAIS"))
                        ? null : reader.GetInt32(reader.GetOrdinal("PARCELAS_TOTAIS")),

                    DataPrimeiraParcela = reader.IsDBNull(reader.GetOrdinal("DATA_PRIMEIRA_PARCELA"))
                        ? null : reader.GetDateTime(reader.GetOrdinal("DATA_PRIMEIRA_PARCELA")),

                    DataUltimaAtividadeConfirmada = reader.IsDBNull(reader.GetOrdinal("DATA_ULTIMA_ATIVIDADE_CONFIRMADA"))
                        ? null : reader.GetDateTime(reader.GetOrdinal("DATA_ULTIMA_ATIVIDADE_CONFIRMADA")),

                    FkIdUsuario = reader.GetInt32(reader.GetOrdinal("FK_ID_USUARIO")),
                    FkIdCategoria = reader.GetInt32(reader.GetOrdinal("FK_ID_CATEGORIA")),
                    FkIdFormaPagamento = reader.GetInt32(reader.GetOrdinal("FK_ID_FORMA_PAGAMENTO"))
                });
            }

            return despesas;
        }
        public async Task<decimal> CalcularTotalDespesasMesAtual(int userId)
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
                var command = new NpgsqlCommand(@"
                    SELECT 
                        COALESCE(SUM(VALOR_TOTAL), 0)
                    FROM 
                        DESPESAS
                    WHERE 
                        FK_ID_USUARIO = @UserId
                        AND EXTRACT(YEAR FROM DATA_DESPESA) = EXTRACT(YEAR FROM CURRENT_DATE)
                        AND EXTRACT(MONTH FROM DATA_DESPESA) = EXTRACT(MONTH FROM CURRENT_DATE);
                    ", connection
                );

                command.Parameters.AddWithValue("@UserId", userId);

                var total = await command.ExecuteScalarAsync();

                if (total != null && total != DBNull.Value)
                {
                    return Convert.ToDecimal(total);
                }

                return 0m;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao calcular despesas: {ex.Message}");
                throw;
            }
        }

        public async Task<decimal> ObterPrevisaoMesSeguinte(int usuarioId)
        {
            var query = @"
            SELECT COALESCE(SUM(valor_mensal), 0)
            FROM (
                -- 1. Despesas Fixas (Sempre repetem)
                SELECT VALOR_TOTAL AS valor_mensal
                FROM DESPESAS
                WHERE FK_ID_USUARIO = @usuarioId AND TIPO_DESPESA = 'FIXA'

                UNION ALL

                -- 2. Despesas Adicionais Parceladas (que ainda terão parcelas no mês que vem)
                SELECT (VALOR_TOTAL / PARCELAS_TOTAIS) AS valor_mensal
                FROM DESPESAS
                WHERE FK_ID_USUARIO = @usuarioId 
                  AND TIPO_DESPESA = 'ADICIONAL' 
                  AND PARCELAS_TOTAIS > 1
                  AND (DATA_PRIMEIRA_PARCELA + (PARCELAS_TOTAIS || ' month')::interval) > (CURRENT_DATE + interval '1 month')
            ) AS subquery";

            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@usuarioId", usuarioId);

            var result = await command.ExecuteScalarAsync();

            if (result != null && result != DBNull.Value)
            {
                return Convert.ToDecimal(result);
            }

            return 0m;
        }
    }
}
