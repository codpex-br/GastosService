using Gastos.Application.DTOs;
using Gastos.Application.Interfaces;
using Gastos.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;

namespace Gastos.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecuperacaoSenhaController : ControllerBase
    {
        private readonly IForgotPasswordRepository _forgotPasswordRepository;
        private readonly IRecuperacaoSenhaRepository _recuperacaoSenhaRepository;
        private readonly IEmailService _emailService;

        public RecuperacaoSenhaController(IForgotPasswordRepository forgotPasswordRepository, IRecuperacaoSenhaRepository recuperacaoSenhaRepository, IEmailService emailService)
        {
            _emailService = emailService;
            _forgotPasswordRepository = forgotPasswordRepository;
            _recuperacaoSenhaRepository = recuperacaoSenhaRepository;
        }

        [HttpPost("enviar-codigo")]
        public async Task<IActionResult> EnviarCodigo([FromBody] EnviarCodigoDto enviarCodigoDto)
        {
            var usuario = await _forgotPasswordRepository.ObterUsuarioIdPorEmail(enviarCodigoDto.Email);

            if (usuario != null)
            {
                int codigoInt = RandomNumberGenerator.GetInt32(100000, 1000000);
                string codigo = codigoInt.ToString();

                await _forgotPasswordRepository.SalvarCodigoRecuperacao(usuario.Id, codigo);
                await _emailService.EnviarCodigo(enviarCodigoDto.Email, codigo, usuario.Nome);
            }

            return Ok("Código enviado para o e-mail.");
        }

        [HttpPost("validar-codigo")]
        public async Task<IActionResult> ValidarCodigo([FromBody] ValidarCodigoDto validarCodigoDto)
        {
            var usuario = await _forgotPasswordRepository.ObterUsuarioIdPorEmail(validarCodigoDto.Email);
            if (usuario == null)
                return NotFound("Usuário não encontrado.");

            var codigoValido = await _recuperacaoSenhaRepository.ValidarCodigo(usuario.Id, validarCodigoDto.Codigo);
            if (codigoValido == null)
                return BadRequest("Código inválido ou expirado.");

            var tokenTemporario = Guid.NewGuid().ToString();
            var expiracaoToken = DateTime.UtcNow.AddMinutes(10);

            await _recuperacaoSenhaRepository.SalvarTokenTemporario(codigoValido.Id, tokenTemporario, expiracaoToken);
            return Ok(new { message = "Código validado com sucesso.", tokenTemporario });
        }
    }
}
