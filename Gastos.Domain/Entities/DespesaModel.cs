using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Gastos.Domain.Entities
{
    [Table("DESPESAS")]
    public class DespesaModel
    {
        [Key]
        [Column("ID_DESPESA")]
        public int Id { get; set; }

        [Required]
        [Column("NOME")]
        public string Nome { get; set; }

        [Required]
        [Column("VALOR_TOTAL", TypeName = "numeric(10, 2)")]
        public decimal ValorTotal { get; set; }

        [Required]
        [Column("DATA_DESPESA")]
        public DateTime DataDespesa { get; set; }

        [Required]
        [Column("TIPO_DESPESA")] // 'FIXA' ou 'ADICIONAL'
        public string TipoDespesa { get; set; }

        [Required]
        [Column("TIPO_TRANSACAO")] // 'CREDITO' ou 'DEBITO'
        public string TipoTransacao { get; set; }

        [Column("PARCELAS_TOTAIS")]
        public int? ParcelasTotais { get; set; }

        [Column("DATA_PRIMEIRA_PARCELA")]                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                
        public DateTime? DataPrimeiraParcela { get; set; }

        [Column("DATA_ULTIMA_ATIVIDADE_CONFIRMADA")]
        public DateTime? DataUltimaAtividadeConfirmada { get; set; }

        [Column("FK_ID_USUARIO")]
        public int FkIdUsuario { get; set; }

        [Column("FK_ID_CATEGORIA")]
        public int FkIdCategoria { get; set; }

        [Column("FK_ID_FORMA_PAGAMENTO")]
        public int FkIdFormaPagamento { get; set; }

        [ForeignKey("IdUsuario")]
        public UsuarioModel? Usuario { get; set; }

        [ForeignKey("IdCategoria")]
        public CategoriaModel? Categoria { get; set; }

        [ForeignKey("IdFormaPagamento")]
        public FormaPagamentoModel? FormaPagamento { get; set; }

    }
}
