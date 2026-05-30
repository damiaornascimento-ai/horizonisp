using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using horizonisp.Context;
using horizonisp.Models;
using horizonisp.Models.Enums;

namespace horizonisp.Services
{
    public record PortalResumo(
        Cliente Cliente,
        IReadOnlyList<Assinatura> Assinaturas,
        Fatura? ProximaFatura,
        int FaturasPendentes,
        int FaturasAtrasadas);

    public interface IPortalService
    {
        Task<PortalResumo?> ObterResumoAsync(int clienteId);
        Task<IReadOnlyList<Fatura>> ObterFaturasAsync(int clienteId);
        Task<Fatura?> ObterFaturaAsync(int clienteId, int faturaId);
        Task<bool> AlterarSenhaPortalAsync(int clienteId, string senhaAtual, string novaSenha);
        Task<bool> AlterarSenhaPppoeAsync(int clienteId, int assinaturaId, string senhaAtual, string novaSenha);
    }

    public class PortalService(
        AppDbContext db,
        PasswordHasher<Cliente> passwordHasher,
        IMikrotikService mikrotikService) : IPortalService
    {
        public async Task<PortalResumo?> ObterResumoAsync(int clienteId)
        {
            var cliente = await db.Clientes
                .Include(c => c.Assinaturas)
                    .ThenInclude(a => a.Plano)
                .Include(c => c.Assinaturas)
                    .ThenInclude(a => a.Faturas)
                .FirstOrDefaultAsync(c => c.Id == clienteId);

            if (cliente is null)
            {
                return null;
            }

            var hoje = DateTime.UtcNow.Date;
            var faturas = cliente.Assinaturas
                .SelectMany(a => a.Faturas)
                .OrderByDescending(f => f.DataVencimento)
                .ToList();

            var proximaFatura = faturas
                .Where(f => f.Status is StatusFatura.Pendente or StatusFatura.Atrasada)
                .OrderBy(f => f.DataVencimento)
                .FirstOrDefault();

            var pendentes = faturas.Count(f => f.Status == StatusFatura.Pendente && f.DataVencimento >= hoje);
            var atrasadas = faturas.Count(f =>
                f.Status is StatusFatura.Pendente or StatusFatura.Atrasada && f.DataVencimento < hoje);

            return new PortalResumo(
                cliente,
                cliente.Assinaturas.OrderByDescending(a => a.DataInicio).ToList(),
                proximaFatura,
                pendentes,
                atrasadas);
        }

        public async Task<IReadOnlyList<Fatura>> ObterFaturasAsync(int clienteId)
        {
            return await db.Faturas
                .Include(f => f.Assinatura)
                    .ThenInclude(a => a.Plano)
                .Where(f => f.Assinatura.ClienteId == clienteId)
                .OrderByDescending(f => f.DataVencimento)
                .ToListAsync();
        }

        public async Task<Fatura?> ObterFaturaAsync(int clienteId, int faturaId)
        {
            return await db.Faturas
                .Include(f => f.Assinatura)
                    .ThenInclude(a => a.Cliente)
                .Include(f => f.Assinatura)
                    .ThenInclude(a => a.Plano)
                .FirstOrDefaultAsync(f => f.Id == faturaId && f.Assinatura.ClienteId == clienteId);
        }

        public async Task<bool> AlterarSenhaPortalAsync(int clienteId, string senhaAtual, string novaSenha)
        {
            var cliente = await db.Clientes.FindAsync(clienteId);
            if (cliente is null || string.IsNullOrEmpty(cliente.SenhaPortalHash))
            {
                return false;
            }

            var result = passwordHasher.VerifyHashedPassword(cliente, cliente.SenhaPortalHash, senhaAtual);
            if (result != PasswordVerificationResult.Success)
            {
                return false;
            }

            cliente.SenhaPortalHash = passwordHasher.HashPassword(cliente, novaSenha);
            await db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AlterarSenhaPppoeAsync(int clienteId, int assinaturaId, string senhaAtual, string novaSenha)
        {
            var assinatura = await db.Assinaturas
                .FirstOrDefaultAsync(a => a.Id == assinaturaId && a.ClienteId == clienteId);

            if (assinatura is null || assinatura.SenhaPppoe != senhaAtual)
            {
                return false;
            }

            assinatura.SenhaPppoe = novaSenha;
            await db.SaveChangesAsync();
            await mikrotikService.AtualizarSenhaAsync(assinatura.LoginPppoe, novaSenha);
            return true;
        }
    }
}
