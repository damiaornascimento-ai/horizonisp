using Microsoft.EntityFrameworkCore;
using horizonisp.Context;
using horizonisp.Models;
using horizonisp.Models.Enums;

namespace horizonisp.Services
{
    public interface IOrdemServicoService
    {
        Task<IReadOnlyList<OrdemServico>> ListarAsync(StatusOrdemServico? status = null);
        Task<IReadOnlyList<OrdemServico>> ListarParaTecnicoAsync();
        Task<OrdemServico?> ObterAsync(int id);
        Task<OrdemServico> CriarAsync(OrdemServico ordem);
        Task SalvarAsync(OrdemServico ordem);
        Task AtualizarStatusAsync(int id, StatusOrdemServico status, string? observacaoConclusao = null);
        Task<int> ContarAbertasAsync();
    }

    public class OrdemServicoService(AppDbContext db) : IOrdemServicoService
    {
        public async Task<IReadOnlyList<OrdemServico>> ListarAsync(StatusOrdemServico? status = null)
        {
            var query = db.OrdensServico
                .Include(o => o.Cliente)
                .Include(o => o.Assinatura)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(o => o.Status == status.Value);
            }

            return await query
                .OrderByDescending(o => o.DataAtualizacao)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<OrdemServico>> ListarParaTecnicoAsync()
        {
            return await db.OrdensServico
                .Include(o => o.Cliente)
                .Include(o => o.Assinatura)
                .Where(o =>
                    o.Status == StatusOrdemServico.Aberta
                    || o.Status == StatusOrdemServico.Agendada
                    || o.Status == StatusOrdemServico.EmCampo)
                .OrderBy(o => o.DataAgendada ?? o.DataAbertura)
                .ToListAsync();
        }

        public Task<OrdemServico?> ObterAsync(int id) =>
            db.OrdensServico
                .Include(o => o.Cliente)
                .Include(o => o.Assinatura)
                .Include(o => o.Chamado)
                .FirstOrDefaultAsync(o => o.Id == id);

        public async Task<OrdemServico> CriarAsync(OrdemServico ordem)
        {
            ordem.DataAbertura = DateTime.UtcNow;
            ordem.DataAtualizacao = DateTime.UtcNow;
            db.OrdensServico.Add(ordem);
            await db.SaveChangesAsync();
            return ordem;
        }

        public async Task SalvarAsync(OrdemServico ordem)
        {
            ordem.DataAtualizacao = DateTime.UtcNow;

            if (ordem.Id == 0)
            {
                await CriarAsync(ordem);
                return;
            }

            db.OrdensServico.Update(ordem);
            await db.SaveChangesAsync();
        }

        public async Task AtualizarStatusAsync(int id, StatusOrdemServico status, string? observacaoConclusao = null)
        {
            var ordem = await db.OrdensServico.FindAsync(id)
                ?? throw new InvalidOperationException("Ordem de serviço não encontrada.");

            ordem.Status = status;
            ordem.DataAtualizacao = DateTime.UtcNow;

            if (status == StatusOrdemServico.Concluida)
            {
                ordem.DataConclusao = DateTime.UtcNow;
                if (!string.IsNullOrWhiteSpace(observacaoConclusao))
                {
                    ordem.ObservacaoConclusao = observacaoConclusao.Trim();
                }
            }

            await db.SaveChangesAsync();
        }

        public Task<int> ContarAbertasAsync() =>
            db.OrdensServico.CountAsync(o =>
                o.Status == StatusOrdemServico.Aberta
                || o.Status == StatusOrdemServico.Agendada
                || o.Status == StatusOrdemServico.EmCampo);
    }
}
