using System;
using System.Collections.Generic;
using System.Text;

namespace Gastos.Application.Interfaces
{
    public interface IEmailService
    {
        Task EnviarCodigo(string destino, string codigo, string nomeUsuario);
    }
}
