using Microsoft.EntityFrameworkCore;
using horizonisp.Context;
using horizonisp.Models.Enums;

namespace horizonisp.Services
{
    public record ClienteBloqueioResult(bool Sucesso, string Mensagem);

    public interface IClienteBloqueioService
    {
        Task<ClienteBloqueioResult> BloquearManualmenteAsync(int clienteId, CancellationToken cancellationToken = default);
        Task<ClienteBloqueioResult> AtivarManualmenteAsync(int clienteId, CancellationToken cancellationToken = default);
    }

    public class ClienteBloqueioService(AppDbContext db, IMikrotikService mikrotikService) : IClienteBloqueioService
    {
        public static bool PodeBloquearManualmente(StatusCliente status) =>
            status is StatusCliente.Ativo or StatusCliente.Inadimplente;

        public static bool PodeAtivarManualmente(StatusCliente status) =>
            status is StatusCliente.Suspenso or StatusCliente.Inadimplente;
        public async Task<ClienteBloqueioResult> BloquearManualmenteAsync(
            int clienteId,
            CancellationToken cancellationToken = default)
        {
            var cliente = await db.Clientes
                .Include(c => c.Assinaturas)
                .FirstOrDefaultAsync(c => c.Id == clienteId, cancellationToken);

            if (cliente is null)
            {
                return new ClienteBloqueioResult(false, "Cliente não encontrado.");
            }

            if (cliente.Status == StatusCliente.Cancelado)
            {
                return new ClienteBloqueioResult(false, "Cliente cancelado não pode ser bloqueado.");
            }

            if (cliente.Status == StatusCliente.Suspenso)
            {
                return new ClienteBloqueioResult(false, "Cliente já está bloqueado.");
            }

            cliente.Status = StatusCliente.Suspenso;

            var assinaturasSuspensas = 0;
            foreach (var assinatura in cliente.Assinaturas.Where(a => a.Status == StatusAssinatura.Ativa))
            {
                assinatura.Status = StatusAssinatura.Suspensa;
                await mikrotikService.SuspenderLoginAsync(assinatura.LoginPppoe, cancellationToken);
                assinaturasSuspensas++;
            }

            await db.SaveChangesAsync(cancellationToken);

            var mensagem = assinaturasSuspensas > 0
                ? $"Cliente {cliente.Nome} bloqueado. {assinaturasSuspensas} assinatura(s) suspensa(s)."
                : $"Cliente {cliente.Nome} bloqueado.";

            return new ClienteBloqueioResult(true, mensagem);
        }

        public async Task<ClienteBloqueioResult> AtivarManualmenteAsync(
            int clienteId,
            CancellationToken cancellationToken = default)
        {
            var cliente = await db.Clientes
                .Include(c => c.Assinaturas)
                .FirstOrDefaultAsync(c => c.Id == clienteId, cancellationToken);

            if (cliente is null)
            {
                return new ClienteBloqueioResult(false, "Cliente não encontrado.");
            }

            if (cliente.Status == StatusCliente.Cancelado)
            {
                return new ClienteBloqueioResult(false, "Cliente cancelado não pode ser reativado por aqui.");
            }

            if (cliente.Status == StatusCliente.Ativo
                && cliente.Assinaturas.All(a => a.Status != StatusAssinatura.Suspensa))
            {
                return new ClienteBloqueioResult(false, "Cliente já está ativo.");
            }

            cliente.Status = StatusCliente.Ativo;

            var assinaturasReativadas = 0;
            foreach (var assinatura in cliente.Assinaturas.Where(a => a.Status == StatusAssinatura.Suspensa))
            {
                assinatura.Status = StatusAssinatura.Ativa;
                await mikrotikService.ReativarLoginAsync(assinatura.LoginPppoe, cancellationToken);
                assinaturasReativadas++;
            }

            await db.SaveChangesAsync(cancellationToken);

            var mensagem = assinaturasReativadas > 0
                ? $"Cliente {cliente.Nome} ativado. {assinaturasReativadas} assinatura(s) reativada(s)."
                : $"Cliente {cliente.Nome} ativado.";

            return new ClienteBloqueioResult(true, mensagem);
        }
    }
}
