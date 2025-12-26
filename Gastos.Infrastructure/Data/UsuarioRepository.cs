using Gastos.Domain.Entities;
using Gastos.Domain.Interfaces;
using Gastos.Shared.Helpers;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Gastos.Infrastructure.Data
{
    public class UsuarioRepository : IUsuarioRepository
    {

        private readonly string _connectionString;

        public UsuarioRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' não encontrada.");

        }

        public async Task<UsuarioModel> InserirUsuario(UsuarioModel usuarioModel, int UsuarioId)
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var hashedPassword = PasswordHasher.HashPassword(usuarioModel.Senha);
            usuarioModel.Senha = hashedPassword;

            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
                var insertUserCmd = new NpgsqlCommand(@"
                    INSERT INTO USUARIOS (
                        NOME, EMAIL, SENHA, FOTO_PERFIL, CRIADO_EM
                    )
                    VALUES (
                        @Nome, @Email, @Senha, @FotoPerfil, CURRENT_TIMESTAMP
                    )
                    RETURNING ID_USUARIO", connection, transaction
                );

                insertUserCmd.Parameters.AddWithValue("@Nome", usuarioModel.Nome.ToUpper());
                insertUserCmd.Parameters.AddWithValue("@Email", usuarioModel.Email.ToUpper());
                insertUserCmd.Parameters.AddWithValue("@Senha", hashedPassword);
                insertUserCmd.Parameters.AddWithValue("@FotoPerfil", string.IsNullOrEmpty(usuarioModel.FotoPerfil) ? "SEM_FOTO" : usuarioModel.FotoPerfil);

                var idUsuario = await insertUserCmd.ExecuteScalarAsync();
                usuarioModel.IdUsuario = Convert.ToInt32(idUsuario);

                await transaction.CommitAsync();
                return usuarioModel;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<IEnumerable<UsuarioModel>> ListarUsuarios()
        {
            var usuariosLista = new List<UsuarioModel>();

            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = "SELECT * FROM USUARIOS";
            await using var command = new NpgsqlCommand(query, connection);
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var usuario = new UsuarioModel
                {
                    IdUsuario = Convert.ToInt32(reader["ID_USUARIO"]),
                    Nome = reader["NOME"].ToString(),
                    Email = reader["EMAIL"].ToString(),
                    Senha = reader["SENHA"].ToString(),
                    FotoPerfil = reader.IsDBNull(reader.GetOrdinal("FOTO_PERFIL")) ? null : reader.GetString(reader.GetOrdinal("FOTO_PERFIL")),
                    DataCriacao = Convert.ToDateTime(reader["CRIADO_EM"])
                };

                usuariosLista.Add(usuario);
            }

            return usuariosLista;
        }

        public async Task<UsuarioModel> ListarUsuarioPorId(int id)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var query = "SELECT id_usuario, nome, email, senha, foto_perfil FROM USUARIOS WHERE id_usuario = @Id";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new UsuarioModel
                            {
                                IdUsuario = Convert.ToInt32(reader["id_usuario"]),
                                Nome = reader["nome"].ToString(),
                                Email = reader["email"].ToString(),
                                Senha = reader["senha"].ToString(),
                                FotoPerfil = reader.IsDBNull(reader.GetOrdinal("foto_perfil")) ? null : reader.GetString(reader.GetOrdinal("foto_perfil")),
                            };
                        }
                    }
                }
            }
            return null;
        }

        public async Task<UsuarioModel?> ObterUsuarioPorId(int id)
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = "SELECT ID_USUARIO FROM USUARIOS WHERE ID_USUARIO = @Id";

            await using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", id);

            await using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new UsuarioModel
                {
                    IdUsuario = Convert.ToInt32(reader["ID_USUARIO"]),
                };
            }

            return null;
        }

        public async Task<UsuarioModel> ListarUsuarioPorEmail(string email)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = "SELECT ID_USUARIO, NOME, EMAIL, SENHA, FOTO_PERFIL, CRIADO_EM FROM USUARIOS WHERE LOWER(EMAIL) = LOWER(@Email)";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Email", email);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var usuario = new UsuarioModel
                            {
                                IdUsuario = Convert.ToInt32(reader["ID_USUARIO"]),
                                Nome = reader["NOME"].ToString(),
                                Email = reader["EMAIL"].ToString(),
                                Senha = reader["SENHA"].ToString(),
                                FotoPerfil = reader.IsDBNull(reader.GetOrdinal("FOTO_PERFIL")) ? null : reader.GetString(reader.GetOrdinal("FOTO_PERFIL")),
                            };
                            return usuario;
                        }
                    }
                }
            }
            return null;
        }

        public async Task<bool> AtualizarDadosUsuario(int usuarioId, string novoNome, string novoEmail, string novaSenha)
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var senhaHasheada = PasswordHasher.HashPassword(novaSenha);

            var query = @"
            UPDATE USUARIOS 
            SET 
                NOME_USUARIO = @Nome, 
                EMAIL_USUARIO = @Email, 
                SENHA_USUARIO = @Senha, 
                DATA_ALTERACAO_USUARIO = CURRENT_TIMESTAMP 
            WHERE ID_USUARIO = @Id";

            await using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@Nome", novoNome.ToUpper());
            command.Parameters.AddWithValue("@Email", novoEmail.ToUpper());
            command.Parameters.AddWithValue("@Senha", senhaHasheada);
            command.Parameters.AddWithValue("@Id", usuarioId);

            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<bool> AlterarSenha(int usuarioId, string novaSenha)
        {
            var novaSenhaHash = PasswordHasher.HashPassword(novaSenha);

            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var query = "UPDATE USUARIOS SET SENHA = @SENHA WHERE ID_USUARIO = @ID";
            await using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@SENHA", novaSenhaHash);
            cmd.Parameters.AddWithValue("@ID", usuarioId);

            var rowsAffected = await cmd.ExecuteNonQueryAsync();

            return rowsAffected > 0;
        }
    }
}
