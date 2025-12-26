using System;
using System.Collections.Generic;
using System.Text;

namespace Gastos.Application.DTOs
{
    public class RecuperacaoSenhaDto
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public DateTime DataExpiracao { get; set; }
        public bool Usado { get; set; } = false;
    }
}
