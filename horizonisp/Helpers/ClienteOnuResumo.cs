using Microsoft.EntityFrameworkCore;
using horizonisp.Context;
using horizonisp.Models;
using horizonisp.Models.Enums;

namespace horizonisp.Helpers
{
    public static class ClienteOnuResumo
    {
        public record Dados(int? SinalDbm, StatusOnu? StatusOnu, string? Serial);

        public static Dados SelecionarOnu(IEnumerable<Onu> onus, IEnumerable<Assinatura>? assinaturas = null)
        {
            var lista = onus.ToList();
            if (lista.Count == 0)
            {
                return new Dados(null, null, null);
            }

            var statusPorAssinatura = assinaturas?
                .ToDictionary(a => a.Id, a => a.Status)
                ?? [];

            var onu = lista
                .Select(o => new
                {
                    Onu = o,
                    AssinaturaAtiva = o.AssinaturaId.HasValue
                        && statusPorAssinatura.TryGetValue(o.AssinaturaId.Value, out var status)
                        && status == StatusAssinatura.Ativa
                })
                .OrderByDescending(x => x.AssinaturaAtiva)
                .ThenByDescending(x => x.Onu.Status == StatusOnu.Online)
                .ThenByDescending(x => x.Onu.UltimaAtualizacao ?? DateTime.MinValue)
                .Select(x => x.Onu)
                .First();

            return new Dados(onu.SinalDbm, onu.Status, onu.Serial);
        }

        public static async Task<Dictionary<int, Dados>> CarregarPorClienteAsync(
            AppDbContext db,
            IEnumerable<int> clienteIds,
            CancellationToken cancellationToken = default)
        {
            var ids = clienteIds.Distinct().ToList();
            if (ids.Count == 0)
            {
                return [];
            }

            var onus = await db.Onus
                .AsNoTracking()
                .Where(o => o.AssinaturaId != null)
                .Include(o => o.Assinatura)
                .Where(o => ids.Contains(o.Assinatura!.ClienteId))
                .ToListAsync(cancellationToken);

            return onus
                .GroupBy(o => o.Assinatura!.ClienteId)
                .ToDictionary(
                    g => g.Key,
                    g => SelecionarOnu(g, g.Select(o => o.Assinatura!).DistinctBy(a => a.Id)));
        }
    }
}
