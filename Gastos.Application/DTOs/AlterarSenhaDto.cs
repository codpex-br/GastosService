using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Gastos.Application.DTOs
{
    public class AlterarSenhaDto
    {
        [Required(ErrorMessage = "O token é obrigatório.")]
        public string TokenTemporario { get; set; }

        [Required(ErrorMessage = "A nova senha é obrigatória.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "A nova senha deve ter entre 6 e 100 caracteres.")]
        public string NovaSenha { get; set; }
    }
}
