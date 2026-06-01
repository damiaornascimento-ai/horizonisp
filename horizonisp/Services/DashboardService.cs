using Microsoft.EntityFrameworkCore;
using horizonisp.Context;
using horizonisp.Helpers;
using horizonisp.Models.Enums;

namespace horizonisp.Services
{
    public record DashboardResumo(
        int TotalClientes,
        int ClientesAtivos,
        int ClientesOnline,
        int ClientesOffline,
        int ClientesBloqueados,
        int AssinaturasAtivas,
        int FaturasPendentes,
        int FaturasAtrasadas,
        decimal ReceitaMesAtual,
        int ChamadosAbertos,
        int OnusOffline,
        int OrdensServicoAbertas);

    public interface IDashboardService
    {
        Task<DashboardResumo> ObterResumoAsync();
    }

    public class DashboardService(AppDbContext db, IOrdemServicoService ordemServicoService) : IDashboardService
    {
        public async Task<DashboardResumo> ObterResumoAsync()
        {
            var hoje = DateTime.UtcNow.Date;
            var inicioMes = new DateTime(hoje.Year, hoje.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            var totalClientes = await db.Clientes.CountAsync();
            var clientesAtivos = await db.Clientes.CountAsync(c => c.Status == StatusCliente.Ativo);
            var assinaturasAtivas = await db.Assinaturas.CountAsync(a => a.Status == StatusAssinatura.Ativa);
            var faturasPendentes = await db.Faturas.CountAsync(f => f.Status == StatusFatura.Pendente);
            var faturasAtrasadas = await db.Faturas.CountAsync(f =>
                f.Status == StatusFatura.Atrasada
                || (f.Status == StatusFatura.Pendente && f.DataVencimento.Date < hoje));

            var receitaMesAtual = await db.Faturas
                .Where(f => f.Status == StatusFatura.Paga
                    && f.DataPagamento >= inicioMes
                    && f.DataPagamento < inicioMes.AddMonths(1))
                .SumAsync(f => (decimal?)f.Valor) ?? 0;

            var chamadosAbertos = await db.Chamados.CountAsync(c =>
                c.Status == StatusChamado.Aberto || c.Status == StatusChamado.EmAndamento);

            var onusOffline = await db.Onus.CountAsync(o => o.Status == StatusOnu.Offline);
            var ordensServicoAbertas = await ordemServicoService.ContarAbertasAsync();

            var (clientesOnline, clientesOffline, clientesBloqueados) = await ObterContagemConexaoClientesAsync();

            return new DashboardResumo(
                totalClientes,
                clientesAtivos,
                clientesOnline,
                clientesOffline,
                clientesBloqueados,
                assinaturasAtivas,
                faturasPendentes,
                faturasAtrasadas,
                receitaMesAtual,
                chamadosAbertos,
                onusOffline,
                ordensServicoAbertas);
        }

        private async Task<(int Online, int Offline, int Bloqueados)> ObterContagemConexaoClientesAsync()
        {
            var clientes = await db.Clientes
                .Include(c => c.Assinaturas)
                .ToListAsync();

            var assinaturaIds = clientes
                .SelectMany(c => c.Assinaturas.Select(a => a.Id))
                .ToHashSet();

            var onusPorAssinatura = await db.Onus
                .Where(o => o.AssinaturaId.HasValue && assinaturaIds.Contains(o.AssinaturaId.Value))
                .GroupBy(o => o.AssinaturaId!.Value)
                .ToDictionaryAsync(g => g.Key, g => g.ToList());

            var online = 0;
            var offline = 0;
            var bloqueados = 0;

            foreach (var cliente in clientes)
            {
                switch (ClienteConexaoClassifier.Classificar(cliente, onusPorAssinatura))
                {
                    case CategoriaConexaoCliente.Online:
                        online++;
                        break;
                    case CategoriaConexaoCliente.Offline:
                        offline++;
                        break;
                    case CategoriaConexaoCliente.Bloqueado:
                        bloqueados++;
                        break;
                }
            }

            return (online, offline, bloqueados);
        }
    }
}
