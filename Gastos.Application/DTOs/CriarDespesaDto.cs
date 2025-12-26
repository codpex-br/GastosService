using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Gastos.Application.DTOs
{
    public class CriarDespesaDto
    {
        [Required] 
        public string Nome { get; set; }

        [Required] 
        public decimal ValorTotal { get; set; }

        [Required] 
        public string TipoDespesa { get; set; } // "FIXA" ou "ADICIONAL"

        [Required] 
        public string TipoTransacao { get; set; } // "CREDITO" ou "DEBITO"

        public int? ParcelasTotais { get; set; } 
        public DateTime? DataDespesa { get; set; }  
        public DateTime? DataPrimeiraParcela { get; set; }

        [Required]
        public int UsuarioId { get; set; }

        [Required]
        public int CategoriaId { get; set; }

        [Required]
        public int FormaPagamentoId { get; set; }
    }
}
