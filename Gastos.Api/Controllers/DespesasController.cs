using Gastos.Application.DTOs;
using Gastos.Application.Interfaces;
using Gastos.Domain.Entities;
using Gastos.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Gastos.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DespesasController : ControllerBase
    {
        private readonly IDespesaRepository _despesaRepository;
        private readonly IUsuarioService _usuarioService;

        public DespesasController(IDespesaRepository despesaRepository, IUsuarioService usuarioService)
        {
            _despesaRepository = despesaRepository;
            _usuarioService = usuarioService;
        }
        private int ObterIdUsuarioLogado() => 1;
        private int GetCurrentUserId() => int.Parse(User.Claims.FirstOrDefault(c => c.Type == "IdClaimType")?.Value);

        [HttpPost("InserirDespesa")]
        public async Task<IActionResult> CriarDespesa([FromBody] CriarDespesaDto createDespesa)
        {
            int userId = ObterIdUsuarioLogado();

            if (createDespesa.TipoTransacao.ToUpper() == "CREDITO" &&
                (createDespesa.ParcelasTotais == null || createDespesa.ParcelasTotais <= 0))
            {
                return BadRequest(new { Erro = "Para transações de Crédito, o número total de parcelas é obrigatório." });
            }

            try
            {
                int parcelas = (createDespesa.TipoTransacao.ToUpper() == "CREDITO" && createDespesa.ParcelasTotais > 0)
                             ? createDespesa.ParcelasTotais.Value
                             : 1;

                DateTime dataDespesaFinal = createDespesa.DataDespesa ?? DateTime.Today;

                var despesaModel = new DespesaModel
                {
                    Nome = createDespesa.Nome, 
                    ValorTotal = createDespesa.ValorTotal,
                    DataDespesa = dataDespesaFinal,
                    TipoDespesa = createDespesa.TipoDespesa,
                    TipoTransacao = createDespesa.TipoTransacao,
                    ParcelasTotais = parcelas,
                    DataPrimeiraParcela = createDespesa.DataPrimeiraParcela ?? dataDespesaFinal,

                    FkIdUsuario = userId, 
                    FkIdCategoria = createDespesa.CategoriaId,
                    FkIdFormaPagamento = createDespesa.FormaPagamentoId
                };

                var despesaCriada = await _despesaRepository.CriarDespesa(despesaModel);

                return CreatedAtAction(
                    nameof(CriarDespesa),
                    new { id = despesaCriada.Id },
                    despesaCriada
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Erro = "Ocorreu um erro interno ao salvar a despesa.", Detalhes = ex.Message });
            }
        }

        [HttpGet("ListarDespesas")]
        public async Task<IActionResult> ListarDespesas()
        {
            try
            {
                var userId = _usuarioService.GetCurrentUserId();

                if (userId <= 0)
                    return Unauthorized(new { Mensagem = "Usuário não identificado no sistema." });
                var despesas = await _despesaRepository.ObterDespesasPorUsuario(userId);

                if (despesas == null || !despesas.Any())
                {
                    return Ok(new { Mensagem = "Nenhuma despesa encontrada para este usuário.", Dados = new List<object>() });
                }

                return Ok(despesas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Erro = "Erro ao listar despesas.", Detalhes = ex.Message });
            }
        }

        [HttpGet("total-mes-atual")]
        public async Task<IActionResult> GetTotalDespesasMesAtual()
        {
            try
            {
                var userId = _usuarioService.GetCurrentUserId();
                var total = await _despesaRepository.CalcularTotalDespesasMesAtual(userId);

                return Ok(new { TotalMes = total });
            }
            catch (InvalidOperationException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { ErroReal = ex.Message });
            }
        }

        [HttpGet("previsao-proximo-mes")]
        public async Task<IActionResult> GetPrevisao()
        {
            var userId = _usuarioService.GetCurrentUserId();

            if (userId <= 0)
                return Unauthorized("Usuário não identificado.");

            var previsao = await _despesaRepository.ObterPrevisaoMesSeguinte(userId);

            return Ok(new
            {
                PrevisaoMesSeguinte = previsao,
                Mensagem = "Previsão baseada em despesas fixas e parcelas pendentes."
            });
        }
    }
}
