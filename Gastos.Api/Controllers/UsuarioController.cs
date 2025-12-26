using Gastos.Application.DTOs;
using Gastos.Domain.Entities;
using Gastos.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Gastos.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IForgotPasswordRepository _forgotPasswordRepository;
        private readonly IRecuperacaoSenhaRepository _recuperacaoSenhaRepository;
        public UsuarioController(IUsuarioRepository usuarioRepository, IHttpContextAccessor httpContextAccessor, IForgotPasswordRepository forgotPasswordRepository, IRecuperacaoSenhaRepository recuperacaoSenhaRepository)
        {
            _usuarioRepository = usuarioRepository;
            _httpContextAccessor = httpContextAccessor;
            _forgotPasswordRepository = forgotPasswordRepository;
            _recuperacaoSenhaRepository = recuperacaoSenhaRepository;
        }

        [HttpPost("InserirUsuario")]
        public async Task<IActionResult> InserirUsuario([FromForm] UsuarioCreateDto createUsuario)
        {
            if (createUsuario == null)
                return BadRequest("Dados do usuário inválidos.");

            string caminhoFoto = null;

            try
            {
                int? usuarioLogadoId = null;

                var userClaims = _httpContextAccessor.HttpContext?.User;
                if (userClaims != null && userClaims.Identity.IsAuthenticated)
                {
                    var idClaim = userClaims.FindFirst("Id")?.Value;
                    if (int.TryParse(idClaim, out int id))
                    {
                        usuarioLogadoId = id;
                    }

                }

                if (createUsuario.FotoPerfilFile is not null && createUsuario.FotoPerfilFile.Length > 0)
                {
                    var pastaUploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

                    if (!Directory.Exists(pastaUploads))
                        Directory.CreateDirectory(pastaUploads);

                    var nomeArquivo = $"{Guid.NewGuid()}{Path.GetExtension(createUsuario.FotoPerfilFile.FileName)}";
                    var caminhoCompleto = Path.Combine(pastaUploads, nomeArquivo);

                    using (var stream = new FileStream(caminhoCompleto, FileMode.Create))
                    {
                        await createUsuario.FotoPerfilFile.CopyToAsync(stream);
                    }
                    caminhoFoto = $"/uploads/{nomeArquivo}";
                }

                UsuarioModel usuario = new UsuarioModel
                {
                    Nome = createUsuario.Nome,
                    Email = createUsuario.Email,
                    Senha = createUsuario.Senha,
                    FotoPerfil = caminhoFoto,
                    DataCriacao = DateTime.Now,
                };

                var usuarioCriado = await _usuarioRepository.InserirUsuario(usuario, usuarioLogadoId ?? 0);

                return CreatedAtAction(nameof(ListarUsuarios), new { id = usuarioCriado.IdUsuario }, usuarioCriado);
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(caminhoFoto))
                {
                    var caminhoFisico = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", caminhoFoto.TrimStart('/'));
                    if (System.IO.File.Exists(caminhoFisico))
                    {
                        System.IO.File.Delete(caminhoFisico);
                    }
                }
                return StatusCode(500, $"Erro interno ao criar o usuário: {ex.Message}");
            }
        }

        [HttpGet("ListarUsuarios")]
        public async Task<ActionResult<List<UsuarioModel>>> ListarUsuarios()
        {
            try
            {
                var usuarios = await _usuarioRepository.ListarUsuarios();
                return Ok(usuarios);
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao obter usuários: {ex.Message}");
            }
        }
    }
}
