using System;
using System.Collections.Generic;
using System.Text;

namespace Gastos.Application.DTOs
{
    public class LoginRequestDto
    {
        public required string Email { get; set; }
        public required string Senha { get; set; }
    }
}
