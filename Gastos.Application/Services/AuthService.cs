using Gastos.Application.Interfaces;
using Gastos.Domain.Entities;
using Gastos.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Gastos.Shared.Helpers;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Gastos.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public AuthService(IUsuarioRepository usuarioRepository, IRefreshTokenRepository refreshTokenRepository, IConfiguration configuration)
        {
            _usuarioRepository = usuarioRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _configuration = configuration;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<UsuarioModel> Authenticate(string email, string senha)
        {
            var usuario = await _usuarioRepository.ListarUsuarioPorEmail(email);

            if (usuario == null)
            {
                return null;
            }

            if (!PasswordHasher.VerificarSenha(senha, usuario.Senha))
            {
                return null;
            }
            return usuario;
        }

        public async Task<string> GenerateAccessToken(UsuarioModel usuarioModel)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:AccessSecret"]);

            var claims = new List<Claim>
            {
                new Claim("Id", usuarioModel.IdUsuario.ToString()),
                new Claim(ClaimTypes.Email, usuarioModel.Email),
                new Claim(ClaimTypes.Name, usuarioModel.Nome)
            };
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(15),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"]

            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task<string> GenerateRefreshToken(UsuarioModel usuarioModel)
        {
            var randomNumber = new byte[32];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        public async Task SaveRefreshToken(string token, DateTime expires, string createdByIp, int usuarioId)
        {
            await _refreshTokenRepository.RevogarRefreshToken(usuarioId, createdByIp, "Novo Login");
            var refreshToken = new RefreshToken
            {
                Token = token,
                Expires = expires,
                Created = DateTime.UtcNow,
                CreatedByIp = createdByIp,
                UsuarioId = usuarioId
            };

            await _refreshTokenRepository.InserirRefreshToken(refreshToken);
        }



        public ClaimsPrincipal GetPrincipalFromRefreshToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var key = Encoding.ASCII.GetBytes(_configuration["Jwt:RefreshSecret"]);
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"],
                    ValidateLifetime = false
                };
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
                var jwtSecurityToken = securityToken as JwtSecurityToken;
                if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new SecurityTokenException("Token de refresh inválido.");
                }
                return principal;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<(UsuarioModel Usuario, RefreshToken Token)?> ValidateRefreshToken(string refreshToken, string ipAddress)
        {
            var storedToken = await _refreshTokenRepository.ObterRefreshToken(refreshToken);

            if (storedToken == null)
            {
                Console.WriteLine("DEBUG: Token não encontrado no banco.");
                return null;
            }

            if (storedToken.Expires < DateTime.UtcNow)
            {
                Console.WriteLine($"DEBUG: Token expirou. Banco: {storedToken.Expires}, Agora: {DateTime.UtcNow}");
                return null;
            }

            if (storedToken.IsRevoked || storedToken.Revoked != null)
            { 
                Console.WriteLine("DEBUG: Token está revogado.");
                return null;
            }

            var usuario = await _usuarioRepository.ListarUsuarioPorId(storedToken.UsuarioId);
            if (usuario == null)
            {
                Console.WriteLine("DEBUG: Usuário dono do token não encontrado.");
                return null;
            }

            return (usuario, storedToken);
        }
    }
}
