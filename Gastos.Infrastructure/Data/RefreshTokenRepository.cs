using Gastos.Domain.Entities;
using Gastos.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gastos.Infrastructure.Data
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly string _connectionString;

        public RefreshTokenRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task InserirRefreshToken(RefreshToken refreshToken)
        {
            const string sql = @"
                INSERT INTO refresh_tokens (
                    token, expires, created, created_by_ip, usuario_id
                )
                VALUES (@Token, @Expires, @Created, @CreatedByIp, @UsuarioId)
            ";

            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Token", refreshToken.Token);
            command.Parameters.AddWithValue("@Expires", refreshToken.Expires);
            command.Parameters.AddWithValue("@Created", refreshToken.Created);
            command.Parameters.AddWithValue("@CreatedByIp", refreshToken.CreatedByIp ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@UsuarioId", refreshToken.UsuarioId);

            await command.ExecuteNonQueryAsync();
        }

        public async Task RevogarRefreshToken(int usuarioId, string revokedByIp, string reasonRevoked)
        {
            const string sql = @"
                UPDATE refresh_tokens 
                SET 
                    is_revoked = TRUE,
                    revoked = CURRENT_TIMESTAMP,
                    revoked_by_ip = @RevokedByIp,
                    reason_revoked = @ReasonRevoked
                WHERE 
                    usuario_id = @UsuarioId 
                    AND is_revoked = FALSE;
            ";

            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("@UsuarioId", usuarioId);
            command.Parameters.AddWithValue("@RevokedByIp", revokedByIp ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@ReasonRevoked", reasonRevoked ?? (object)DBNull.Value);

            await command.ExecuteNonQueryAsync();
        }
        public async Task RevogarToken(string token, string revokedByIp, string reason)
        {
            const string sql = @"
                UPDATE refresh_tokens
                SET 
                    is_revoked = TRUE,
                    revoked = CURRENT_TIMESTAMP,
                    revoked_by_ip = @RevokedByIp,
                    reason_revoked = @Reason
                WHERE 
                    token = @Token
                    AND is_revoked = FALSE;
            ";

            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Token", token);
            command.Parameters.AddWithValue("@RevokedByIp", revokedByIp ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Reason", reason ?? (object)DBNull.Value);

            await command.ExecuteNonQueryAsync();
        }


        public async Task<RefreshToken?> ObterRefreshToken(string token)
        {
            const string sql = @"
                SELECT 
                    id,
                    token,
                    expires,
                    created,
                    created_by_ip,
                    revoked,
                    revoked_by_ip,
                    reason_revoked,
                    is_revoked,
                    usuario_id
                FROM refresh_tokens
                WHERE token = @Token
                LIMIT 1;
            ";

            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Token", token);

            await using var reader = await command.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
                return null;

            return new RefreshToken
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                Token = reader.GetString(reader.GetOrdinal("token")),
                Expires = reader.GetDateTime(reader.GetOrdinal("expires")),
                Created = reader.GetDateTime(reader.GetOrdinal("created")),
                CreatedByIp = reader["created_by_ip"] as string,
                Revoked = reader["revoked"] as DateTime?,
                RevokedByIp = reader["revoked_by_ip"] as string,
                ReasonRevoked = reader["reason_revoked"] as string,
                IsRevoked = reader.GetBoolean(reader.GetOrdinal("is_revoked")),
                UsuarioId = reader.GetInt32(reader.GetOrdinal("usuario_id"))
            };
        }

        public async Task AtualizarRefreshToken(RefreshToken refreshToken)
        {
            const string sql = @"
                UPDATE refresh_tokens
                SET
                    revoked = @Revoked,
                    revoked_by_ip = @RevokedByIp,
                    reason_revoked = @ReasonRevoked,
                    is_revoked = @IsRevoked
                WHERE id = @Id;
            ";

            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Id", refreshToken.Id);
            command.Parameters.AddWithValue("@Revoked", refreshToken.Revoked ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@RevokedByIp", refreshToken.RevokedByIp ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@ReasonRevoked", refreshToken.ReasonRevoked ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@IsRevoked", refreshToken.IsRevoked);

            await command.ExecuteNonQueryAsync();
        }

    }
}
