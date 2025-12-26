using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Gastos.Domain.Entities
{
    [Table("USUARIOS")]
    public class UsuarioModel
    {
        [Key]
        [Column("ID_USUARIO")]
        public int IdUsuario { get; set; }

        [Required]
        [Column("NOME")]
        public string Nome { get; set; }

        [Required]
        [Column("EMAIL")]
        public string Email { get; set; }

        [Required]
        [Column("SENHA")]
        public string Senha { get; set; }

        [Column("FOTO_PERFIL")]
        public string? FotoPerfil { get; set; }

        [Column("CRIADO_EM")]
        public DateTime DataCriacao { get; set; } = DateTime.Now;
    }
}
