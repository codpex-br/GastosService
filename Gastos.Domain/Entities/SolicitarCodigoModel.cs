using System;
using System.Collections.Generic;
using System.Text;

namespace Gastos.Domain.Entities
{
    public class SolicitarCodigoModel
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Nome { get; set; }
    }
}