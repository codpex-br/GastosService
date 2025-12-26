using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Gastos.Domain.Entities
{
    [Table("FORMA_PAGAMENTO")]
    public class FormaPagamentoModel
    {
        [Key]
        [Column("ID_FORMA_PAGAMENTO")]
        public int IdFormaPagamento { get; set; }

        [Required]
        [Column("NOME")]
        public string Nome { get; set; }
    }
}
