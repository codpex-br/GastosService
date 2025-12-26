using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Gastos.Domain.Entities
{
    [Table("RECUPERACAO_SENHA")]
    public class RecuperacaoSenhaModel
    {
        [Key]
        [Column("ID_RECUPERACAO")]
        public int Id { get; set; }

        [Column("USUARIO_ID")]
        public int UsuarioId { get; set; }

        [Column("CODIGO")]
        public string Codigo { get; set; } = string.Empty;

        [Column("DATA_EXPIRACAO")]
        public DateTime DataExpiracao { get; set; }

        [Column("USADO")]
        public bool Usado { get; set; } = false;

        [Column("TOKEN_TEMPORARIO")]
        public string? TokenTemporario { get; set; }

        [Column("TOKEN_EXPIRACAO")]
        public DateTime? TokenExpiracao { get; set; }

    }
}
