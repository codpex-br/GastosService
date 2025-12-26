using Gastos.Application.DTOs;
using Gastos.Application.Interfaces;
using Gastos.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Gastos.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IRecuperacaoSenhaRepository _recuperacaoSenhaRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IAuthService _authService;

        public AuthController(IUsuarioRepository usuarioRepository, IRecuperacaoSenhaRepository recuperacaoSenhaRepository, IRefreshTokenRepository refreshTokenRepository, IAuthService authService)
        {
            _usuarioRepository = usuarioRepository;
            _recuperacaoSenhaRepository = recuperacaoSenhaRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _authService = authService;
        }

        [HttpPost("AutenticarUsuario")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto authModel)
        {
            var usuario = await _authService.Authenticate(authModel.Email, authModel.Senha);

            if (usuario == null) return BadRequest(new { message = "Credenciais inválidas" });

            var accessToken = await _authService.GenerateAccessToken(usuario);
            var refreshToken = await _authService.GenerateRefreshToken(usuario);

            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            await _authService.SaveRefreshToken(refreshToken, DateTime.UtcNow.AddDays(7), ipAddress, usuario.IdUsuario);

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = false, // (true apenas em HTTPS)
                SameSite = SameSiteMode.Lax, // ajustar de NONE para STRICT
                Path = "/",
                Expires = DateTimeOffset.UtcNow.AddDays(7),
                IsEssential = true
            };

            Response.Cookies.Append("RefreshToken", refreshToken, cookieOptions);

            var usuarioCompleto = await _usuarioRepository.ListarUsuarioPorEmail(usuario.Email);

            return Ok(new
            {
                accessToken,
                usuario = new
                {
                    usuario.IdUsuario,
                    usuario.Nome,
                    usuario.Email,
                    usuario.FotoPerfil,
                },
            });
        }

        [HttpPost("auto-login")]
        public async Task<IActionResult> AutoLogin()
        {
            var refreshToken = Request.Cookies["RefreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
                return Unauthorized();

            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            var validation = await _authService.ValidateRefreshToken(refreshToken, ipAddress);
            if (validation is not { } result)
                return Unauthorized();

            var newRefreshToken = await _authService.GenerateRefreshToken(result.Usuario);
            await _authService.SaveRefreshToken(newRefreshToken, DateTime.UtcNow.AddDays(7), ipAddress, result.Usuario.IdUsuario);

            Response.Cookies.Append("RefreshToken", newRefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Lax,
                Path = "/",
                Expires = DateTimeOffset.UtcNow.AddDays(7), 
                IsEssential = true
            });

            var newAccessToken = await _authService.GenerateAccessToken(result.Usuario);

            return Ok(new
            {
                accessToken = newAccessToken,
                usuario = new
                {
                    result.Usuario.IdUsuario,
                    result.Usuario.Nome,
                    result.Usuario.Email,
                    result.Usuario.FotoPerfil
                }
            });
        }


        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            var refreshToken = Request.Cookies["RefreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
                return Unauthorized();

            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            var validation = await _authService.ValidateRefreshToken(refreshToken, ipAddress);
            if (validation is not { } result)
                return Unauthorized();

            await _refreshTokenRepository.RevogarToken(
                result.Token.Token,
                ipAddress,
                "Refresh token rotacionado"
            );

            var newRefreshToken = await _authService.GenerateRefreshToken(result.Usuario);

            await _authService.SaveRefreshToken(
                newRefreshToken,
                DateTime.UtcNow.AddDays(7),
                ipAddress,
                result.Usuario.IdUsuario
            );

            Response.Cookies.Append("RefreshToken", newRefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Lax,
                Path = "/",
                Expires = DateTimeOffset.UtcNow.AddDays(7),
                IsEssential = true
            });

            var newAccessToken = await _authService.GenerateAccessToken(result.Usuario);

            return Ok(new
            {
                accessToken = newAccessToken
            });
        }

        [HttpPost("alterar-senha")]
        public async Task<IActionResult> AlterarSenha([FromBody] AlterarSenhaDto dto)
        {
            var registroRecuperacao = await _recuperacaoSenhaRepository.ValidarTokenTemporario(dto.TokenTemporario);

            if (registroRecuperacao == null)
            {
                return BadRequest(new { message = "O token de recuperação é inválido ou expirou. Por favor, solicite um novo." });
            }

            var senhaAlterada = await _usuarioRepository.AlterarSenha(registroRecuperacao.UsuarioId, dto.NovaSenha);

            if (senhaAlterada)
            {
                await _recuperacaoSenhaRepository.MarcarTokenComoUsado(dto.TokenTemporario);
                return Ok(new { message = "Senha alterada com sucesso!" });
            }

            return BadRequest(new { message = "Não foi possível alterar a senha." });
        }

        [HttpPost("Logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var refreshToken = Request.Cookies["RefreshToken"];

            if (!string.IsNullOrEmpty(refreshToken))
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

                await _refreshTokenRepository.RevogarToken(
                    refreshToken,
                    ipAddress,
                    "Logout manual do usuário"
                );
            }

            Response.Cookies.Delete("RefreshToken", new CookieOptions
            {
                HttpOnly = true,
                Secure = false,          // true em produção
                SameSite = SameSiteMode.Lax,
                Path = "/"
            });


            return Ok(new { message = "Logout realizado com sucesso" });
        }
    }
}
