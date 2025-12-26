using Gastos.Domain.Entities;
using Gastos.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gastos.Infrastructure.Data
{
    public class RecuperacaoSenhaRepository : IRecuperacaoSenhaRepository
    {
        private readonly string _connectionString;

        public RecuperacaoSenhaRepository(IConfiguration configuration)
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

        public async Task CriarCodigo(int usuarioId, string codigo, DateTime dataExpiracao)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var query = @"INSERT INTO RECUPERACAO_SENHA (USUARIO_FK, CODIGO, DATA_EXPIRACAO) 
                          VALUES (@USUARIO_ID, @CODIGO, @DATA_EXPIRACAO)";

            await using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@USUARIO_ID", usuarioId);
            cmd.Parameters.AddWithValue("@CODIGO", codigo);
            cmd.Parameters.AddWithValue("@DATA_EXPIRACAO", dataExpiracao);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task SalvarTokenTemporario(int id, string token, DateTime expiracao)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var query = @"UPDATE RECUPERACAO_SENHA 
                      SET TOKEN_TEMPORARIO = @TOKEN, 
                          TOKEN_EXPIRACAO = @EXPIRACAO
                      WHERE ID_RECUPERACAO = @ID";

            await using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@TOKEN", token);
            cmd.Parameters.AddWithValue("@EXPIRACAO", expiracao);
            cmd.Parameters.AddWithValue("@ID", id);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<RecuperacaoSenhaModel?> ValidarTokenTemporario(string token)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var query = @"SELECT * FROM RECUPERACAO_SENHA 
                  WHERE TOKEN_TEMPORARIO = @TOKEN 
                  AND USADO = FALSE 
                  AND TOKEN_EXPIRACAO > CURRENT_TIMESTAMP";

            await using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@TOKEN", token);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new RecuperacaoSenhaModel
                {
                    Id = reader.GetInt32(reader.GetOrdinal("ID_RECUPERACAO")),
                    UsuarioId = reader.GetInt32(reader.GetOrdinal("USUARIO_ID")),
                    Codigo = reader.GetString(reader.GetOrdinal("CODIGO")),
                    DataExpiracao = reader.GetDateTime(reader.GetOrdinal("DATA_EXPIRACAO")),
                    Usado = reader.GetBoolean(reader.GetOrdinal("USADO"))
                };
            }

            return null;
        }

        public async Task<RecuperacaoSenhaModel?> ValidarCodigo(int usuarioId, string codigo)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var query = @"SELECT * FROM RECUPERACAO_SENHA 
                          WHERE USUARIO_ID = @USUARIO_ID 
                          AND CODIGO = @CODIGO 
                          AND USADO = FALSE 
                          AND DATA_EXPIRACAO > CURRENT_TIMESTAMP";

            await using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@USUARIO_ID", usuarioId);
            cmd.Parameters.AddWithValue("@CODIGO", codigo);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new RecuperacaoSenhaModel
                {
                    Id = reader.GetInt32(reader.GetOrdinal("ID_RECUPERACAO")),
                    UsuarioId = reader.GetInt32(reader.GetOrdinal("USUARIO_ID")),
                    Codigo = reader.GetString(reader.GetOrdinal("CODIGO")),
                    DataExpiracao = reader.GetDateTime(reader.GetOrdinal("DATA_EXPIRACAO")),
                    Usado = reader.GetBoolean(reader.GetOrdinal("USADO"))
                };
            }

            return null;
        }

        public async Task MarcarTokenComoUsado(string token)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var query = @"UPDATE RECUPERACAO_SENHA SET USADO = TRUE WHERE TOKEN_TEMPORARIO = @TOKEN";
            await using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@TOKEN", token);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task MarcarCodigoComoUsado(int id)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var query = @"UPDATE RECUPERACAO_SENHA SET USADO = TRUE WHERE ID_RECUPERACAO = @ID";
            await using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@ID", id);

            await cmd.ExecuteNonQueryAsync();
        }
    }
}
