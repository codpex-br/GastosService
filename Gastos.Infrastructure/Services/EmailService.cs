using Microsoft.Extensions.Configuration;
using Gastos.Application.Interfaces;
using MailKit.Net.Smtp;
using MimeKit;

namespace Gastos.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task EnviarCodigo(string destino, string codigo, string nomeUsuario)
        {
            var mensagem = new MimeMessage();
            mensagem.From.Add(new MailboxAddress("Gexxze", _config["Email:From"]));
            mensagem.To.Add(new MailboxAddress("", destino));
            mensagem.Subject = "Código de recuperação de senha";

            var htmlBody = $@"
                <html>
                  <body style=""font-family: Arial, sans-serif; padding: 20px; background-color: #f4f6fa; color: #333;"">
                    <div style=""max-width: 600px; margin: auto; background-color: #ffffff; padding: 30px; border-radius: 10px; box-shadow: 0 4px 10px rgba(0,0,0,0.05);"">

                      <h2 style=""color: #4169E1; font-size: 24px; margin-bottom: 20px;"">Recuperação de Senha</h2>

                      <p style=""font-size: 16px;"">Olá, {nomeUsuario}!</p>

                      <p style=""font-size: 16px;"">Você solicitou a recuperação de sua senha. Aqui está seu código:</p>

                      <h1 style=""color: #ffffff; background-color: #4169E1; padding: 15px 30px; display: inline-block; border-radius: 8px; letter-spacing: 2px; font-size: 28px;"">{codigo}</h1>

                      <p style=""font-size: 14px; margin-top: 20px;"">Este código é válido por <strong>2 minutos</strong>.</p>

                      <p style=""font-size: 14px;"">Se você não solicitou isso, apenas ignore este e-mail.</p>

                      <hr style=""margin: 30px 0; border: none; border-top: 1px solid #ddd;"" />

                      <p style=""font-size: 14px; display: flex; align-items: center;"">
                        <img src=""https://raw.githubusercontent.com/joaogentelucio/teste-app/master/src/assets/logo.png"" alt=""Logo"" style=""height: 24px; margin-right: 8px; vertical-align: middle;"">
                        <strong>Equipe App</strong>
                      </p>
                    </div>
                  </body>
                </html>
            ";

            mensagem.Body = new TextPart("html")
            {
                Text = htmlBody
            };

            using var client = new SmtpClient();
            await client.ConnectAsync(_config["Email:SmtpHost"], int.Parse(_config["Email:SmtpPort"]), MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_config["Email:Username"], _config["Email:Password"]);
            await client.SendAsync(mensagem);
            await client.DisconnectAsync(true);
        }
    }
}
