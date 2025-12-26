using System;
using System.Collections.Generic;
using System.Text;

namespace Gastos.Application.DTOs
{
    public class ValidarCodigoDto
    {
        public string Email { get; set; } = string.Empty;
        public string Codigo { get; set; } = string.Empty;
    }
}
