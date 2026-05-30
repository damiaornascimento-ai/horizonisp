using System.Net;
using System.Net.Mail;
using horizonisp.Configuration;
using Microsoft.Extensions.Options;

namespace horizonisp.Services
{
    public interface IEmailService
    {
        Task EnviarAsync(string destinatario, string assunto, string corpoHtml);
    }

    public class EmailService(
        IOptions<HorizonIspOptions> options,
        ILogger<EmailService> logger) : IEmailService
    {
        private readonly EmailOptions _email = options.Value.Email;

        public async Task EnviarAsync(string destinatario, string assunto, string corpoHtml)
        {
            if (!_email.Habilitado)
            {
                logger.LogInformation(
                    "E-mail simulado para {Destinatario}. Assunto: {Assunto}",
                    destinatario,
                    assunto);
                return;
            }

            using var mensagem = new MailMessage
            {
                From = new MailAddress(_email.Remetente, "Horizon ISP"),
                Subject = assunto,
                Body = corpoHtml,
                IsBodyHtml = true
            };
            mensagem.To.Add(destinatario);

            using var cliente = new SmtpClient(_email.Host, _email.Porta)
            {
                EnableSsl = _email.UsarSsl,
                Credentials = new NetworkCredential(_email.Usuario, _email.Senha)
            };

            await cliente.SendMailAsync(mensagem);
            logger.LogInformation("E-mail enviado para {Destinatario}", destinatario);
        }
    }
}
