using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using horizonisp.Context;
using horizonisp.Models;
using horizonisp.Models.Enums;

namespace horizonisp.Services
{
    public enum ResultadoRedefinicaoSenhaPortal
    {
        Sucesso,
        TokenInvalido,
        TokenExpirado
    }

    public interface IRecuperacaoSenhaPortalService
    {
        Task SolicitarAsync(string identificador, string urlBase);
        Task<ResultadoRedefinicaoSenhaPortal> RedefinirAsync(string token, string novaSenha);
    }

    public class RecuperacaoSenhaPortalService(
        AppDbContext db,
        IEmailService emailService,
        PasswordHasher<Cliente> passwordHasher,
        ILogger<RecuperacaoSenhaPortalService> logger) : IRecuperacaoSenhaPortalService
    {
        private static readonly TimeSpan ValidadeToken = TimeSpan.FromHours(1);

        public async Task SolicitarAsync(string identificador, string urlBase)
        {
            var identificadorNormalizado = identificador.Trim();

            var cliente = await db.Clientes.FirstOrDefaultAsync(c =>
                c.PortalAtivo
                && c.Status != StatusCliente.Cancelado
                && (c.Email == identificadorNormalizado || c.Documento == identificadorNormalizado));

            if (cliente is null)
            {
                return;
            }

            var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
            cliente.RecuperacaoSenhaToken = token;
            cliente.RecuperacaoSenhaExpiraEm = DateTime.UtcNow.Add(ValidadeToken);
            await db.SaveChangesAsync();

            var link = $"{urlBase.TrimEnd('/')}/Portal/RedefinirSenha?token={Uri.EscapeDataString(token)}";
            var corpo = $"""
                <p>Olá, {cliente.Nome}.</p>
                <p>Recebemos uma solicitação para redefinir a senha da sua área do cliente.</p>
                <p><a href="{link}">Clique aqui para criar uma nova senha</a></p>
                <p>Este link expira em 1 hora. Se você não solicitou a redefinição, ignore este e-mail.</p>
                """;

            await emailService.EnviarAsync(
                cliente.Email,
                "Redefinição de senha - Área do Cliente Horizon ISP",
                corpo);

            logger.LogInformation(
                "Recuperação de senha solicitada para cliente {ClienteId}. Link: {Link}",
                cliente.Id,
                link);
        }

        public async Task<ResultadoRedefinicaoSenhaPortal> RedefinirAsync(string token, string novaSenha)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return ResultadoRedefinicaoSenhaPortal.TokenInvalido;
            }

            var tokenNormalizado = token.Trim();
            var cliente = await db.Clientes.FirstOrDefaultAsync(c => c.RecuperacaoSenhaToken == tokenNormalizado);
            if (cliente is null)
            {
                return ResultadoRedefinicaoSenhaPortal.TokenInvalido;
            }

            if (cliente.RecuperacaoSenhaExpiraEm is null || cliente.RecuperacaoSenhaExpiraEm <= DateTime.UtcNow)
            {
                return ResultadoRedefinicaoSenhaPortal.TokenExpirado;
            }

            cliente.SenhaPortalHash = passwordHasher.HashPassword(cliente, novaSenha);
            cliente.RecuperacaoSenhaToken = null;
            cliente.RecuperacaoSenhaExpiraEm = null;
            await db.SaveChangesAsync();

            return ResultadoRedefinicaoSenhaPortal.Sucesso;
        }
    }
}
