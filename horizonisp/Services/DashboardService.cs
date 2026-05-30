using Microsoft.EntityFrameworkCore;
using horizonisp.Context;
using horizonisp.Models.Enums;

namespace horizonisp.Services
{
    public record DashboardResumo(
        int TotalClientes,
        int ClientesAtivos,
        int AssinaturasAtivas,
        int FaturasPendentes,
        int FaturasAtrasadas,
        decimal ReceitaMesAtual);

    public interface IDashboardService
    {
        Task<DashboardResumo> ObterResumoAsync();
    }

    public class DashboardService(AppDbContext db) : IDashboardService
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
                f.Status == StatusFatura.Pendente && f.DataVencimento < hoje);

            var receitaMesAtual = await db.Faturas
                .Where(f => f.Status == StatusFatura.Paga
                    && f.DataPagamento >= inicioMes
                    && f.DataPagamento < inicioMes.AddMonths(1))
                .SumAsync(f => (decimal?)f.Valor) ?? 0;

            return new DashboardResumo(
                totalClientes,
                clientesAtivos,
                assinaturasAtivas,
                faturasPendentes,
                faturasAtrasadas,
                receitaMesAtual);
        }
    }
}
