using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Gastos.Domain.Entities
{
    [Table("CATEGORIAS")]
    public class CategoriaModel
    {
        [Key]
        [Column("ID_CATEGORIA")]
        public int IdCategoria { get; set; }

        [Required]
        [Column("NOME")]
        public string Nome { get; set; }

        [Column("DESCRICAO")]
        public string? Descricao { get; set; } 

        [Required]
        [Column("NIVEL")]
        public string Nivel { get; set; }
    }
}
