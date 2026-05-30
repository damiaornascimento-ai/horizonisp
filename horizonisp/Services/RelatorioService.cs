using Microsoft.EntityFrameworkCore;
using horizonisp.Context;
using horizonisp.Models.Enums;

namespace horizonisp.Services
{
    public record RelatorioResumo(
        decimal ReceitaMesAtual,
        decimal ReceitaMesAnterior,
        decimal TotalInadimplente,
        int FaturasAtrasadas,
        int ClientesAtivos,
        int ClientesSuspensos,
        int AssinaturasAtivas,
        int ChamadosAbertos,
        decimal MrrEstimado);

    public interface IRelatorioService
    {
        Task<RelatorioResumo> ObterResumoAsync();
        Task<IReadOnlyList<ClienteInadimplenteItem>> ObterInadimplentesAsync();
    }

    public record ClienteInadimplenteItem(
        int ClienteId,
        string Nome,
        string Email,
        int FaturasAtrasadas,
        decimal ValorDevido,
        DateTime VencimentoMaisAntigo);

    public class RelatorioService(AppDbContext db, IChamadoService chamadoService) : IRelatorioService
    {
        public async Task<RelatorioResumo> ObterResumoAsync()
        {
            var hoje = DateTime.UtcNow.Date;
            var inicioMes = new DateTime(hoje.Year, hoje.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var inicioMesAnterior = inicioMes.AddMonths(-1);

            var receitaMesAtual = await db.Faturas
                .Where(f => f.Status == StatusFatura.Paga
                    && f.DataPagamento >= inicioMes
                    && f.DataPagamento < inicioMes.AddMonths(1))
                .SumAsync(f => (decimal?)f.Valor) ?? 0;

            var receitaMesAnterior = await db.Faturas
                .Where(f => f.Status == StatusFatura.Paga
                    && f.DataPagamento >= inicioMesAnterior
                    && f.DataPagamento < inicioMes)
                .SumAsync(f => (decimal?)f.Valor) ?? 0;

            var faturasAtrasadas = await db.Faturas
                .Where(f => f.Status == StatusFatura.Atrasada
                    || (f.Status == StatusFatura.Pendente && f.DataVencimento.Date < hoje))
                .ToListAsync();

            var totalInadimplente = faturasAtrasadas.Sum(f => f.Valor);

            var clientesAtivos = await db.Clientes.CountAsync(c => c.Status == StatusCliente.Ativo);
            var clientesSuspensos = await db.Clientes.CountAsync(c =>
                c.Status == StatusCliente.Suspenso || c.Status == StatusCliente.Inadimplente);
            var assinaturasAtivas = await db.Assinaturas.CountAsync(a => a.Status == StatusAssinatura.Ativa);

            var mrr = await db.Assinaturas
                .Include(a => a.Plano)
                .Where(a => a.Status == StatusAssinatura.Ativa)
                .SumAsync(a => (decimal?)a.Plano.PrecoMensal) ?? 0;

            var chamadosAbertos = await chamadoService.ContarAbertosAsync();

            return new RelatorioResumo(
                receitaMesAtual,
                receitaMesAnterior,
                totalInadimplente,
                faturasAtrasadas.Count,
                clientesAtivos,
                clientesSuspensos,
                assinaturasAtivas,
                chamadosAbertos,
                mrr);
        }

        public async Task<IReadOnlyList<ClienteInadimplenteItem>> ObterInadimplentesAsync()
        {
            var hoje = DateTime.UtcNow.Date;

            var faturas = await db.Faturas
                .Include(f => f.Assinatura)
                    .ThenInclude(a => a.Cliente)
                .Where(f => f.Status == StatusFatura.Atrasada
                    || (f.Status == StatusFatura.Pendente && f.DataVencimento.Date < hoje))
                .ToListAsync();

            return faturas
                .GroupBy(f => f.Assinatura.ClienteId)
                .Select(g =>
                {
                    var cliente = g.First().Assinatura.Cliente;
                    return new ClienteInadimplenteItem(
                        cliente.Id,
                        cliente.Nome,
                        cliente.Email,
                        g.Count(),
                        g.Sum(f => f.Valor),
                        g.Min(f => f.DataVencimento));
                })
                .OrderByDescending(i => i.ValorDevido)
                .ToList();
        }
    }
}
