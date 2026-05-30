using Microsoft.EntityFrameworkCore;
using horizonisp.Context;
using horizonisp.Models;
using horizonisp.Models.Enums;

namespace horizonisp.Services
{
    public interface IRedeService
    {
        Task<IReadOnlyList<Olt>> ListarOltsAsync();
        Task<Olt?> ObterOltAsync(int id);
        Task<Olt> SalvarOltAsync(Olt olt);
        Task<IReadOnlyList<Onu>> ListarOnusAsync(int? oltId = null);
        Task<Onu?> ObterOnuAsync(int id);
        Task<Onu> SalvarOnuAsync(Onu onu);
        Task<int> ContarOnusOfflineAsync();
    }

    public class RedeService(AppDbContext db) : IRedeService
    {
        public async Task<IReadOnlyList<Olt>> ListarOltsAsync() =>
            await db.Olts.Include(o => o.Onus).OrderBy(o => o.Nome).ToListAsync();

        public Task<Olt?> ObterOltAsync(int id) =>
            db.Olts.Include(o => o.Onus).FirstOrDefaultAsync(o => o.Id == id);

        public async Task<Olt> SalvarOltAsync(Olt olt)
        {
            if (olt.Id == 0)
            {
                db.Olts.Add(olt);
            }
            else
            {
                db.Olts.Update(olt);
            }

            await db.SaveChangesAsync();
            return olt;
        }

        public async Task<IReadOnlyList<Onu>> ListarOnusAsync(int? oltId = null)
        {
            var query = db.Onus
                .Include(o => o.Olt)
                .Include(o => o.Assinatura)
                    .ThenInclude(a => a!.Cliente)
                .AsQueryable();

            if (oltId.HasValue)
            {
                query = query.Where(o => o.OltId == oltId.Value);
            }

            return await query.OrderBy(o => o.Olt.Nome).ThenBy(o => o.PonPorta).ToListAsync();
        }

        public Task<Onu?> ObterOnuAsync(int id) =>
            db.Onus
                .Include(o => o.Olt)
                .Include(o => o.Assinatura)
                    .ThenInclude(a => a!.Cliente)
                .FirstOrDefaultAsync(o => o.Id == id);

        public async Task<Onu> SalvarOnuAsync(Onu onu)
        {
            onu.UltimaAtualizacao = DateTime.UtcNow;

            if (onu.Id == 0)
            {
                db.Onus.Add(onu);
            }
            else
            {
                db.Onus.Update(onu);
            }

            await db.SaveChangesAsync();
            return onu;
        }

        public Task<int> ContarOnusOfflineAsync() =>
            db.Onus.CountAsync(o => o.Status == StatusOnu.Offline);
    }
}
