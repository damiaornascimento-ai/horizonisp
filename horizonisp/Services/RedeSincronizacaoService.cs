using Microsoft.EntityFrameworkCore;
using horizonisp.Context;
using horizonisp.Models;
using horizonisp.Models.Enums;

namespace horizonisp.Services
{
    public record RedeSincronizacaoResultado(
        int OltsProcessadas,
        int OnusAtualizadas,
        int Erros,
        IReadOnlyList<string> Mensagens);

    public interface IRedeSincronizacaoService
    {
        Task<RedeSincronizacaoResultado> SincronizarTodasAsync(CancellationToken cancellationToken = default);
        Task<RedeSincronizacaoResultado> SincronizarOltAsync(int oltId, CancellationToken cancellationToken = default);
    }

    public class RedeSincronizacaoService(
        AppDbContext db,
        IOltIntegracaoService oltIntegracao,
        ILogger<RedeSincronizacaoService> logger) : IRedeSincronizacaoService
    {
        public async Task<RedeSincronizacaoResultado> SincronizarTodasAsync(CancellationToken cancellationToken = default)
        {
            var olts = await db.Olts
                .Where(o => o.Ativo)
                .Select(o => o.Id)
                .ToListAsync(cancellationToken);

            var mensagens = new List<string>();
            var onusAtualizadas = 0;
            var erros = 0;

            foreach (var oltId in olts)
            {
                var resultado = await SincronizarOltInternoAsync(oltId, cancellationToken);
                onusAtualizadas += resultado.OnusAtualizadas;
                erros += resultado.Erros;
                mensagens.AddRange(resultado.Mensagens);
            }

            return new RedeSincronizacaoResultado(olts.Count, onusAtualizadas, erros, mensagens);
        }

        public async Task<RedeSincronizacaoResultado> SincronizarOltAsync(
            int oltId,
            CancellationToken cancellationToken = default)
        {
            return await SincronizarOltInternoAsync(oltId, cancellationToken);
        }

        private async Task<RedeSincronizacaoResultado> SincronizarOltInternoAsync(
            int oltId,
            CancellationToken cancellationToken)
        {
            var mensagens = new List<string>();
            var onusAtualizadas = 0;

            var olt = await db.Olts
                .Include(o => o.Onus)
                .FirstOrDefaultAsync(o => o.Id == oltId, cancellationToken);

            if (olt is null)
            {
                return new RedeSincronizacaoResultado(0, 0, 1, ["OLT não encontrada."]);
            }

            if (!olt.Ativo)
            {
                return new RedeSincronizacaoResultado(0, 0, 0, [$"OLT {olt.Nome} inativa — ignorada."]);
            }

            try
            {
                var leituras = await oltIntegracao.ObterLeiturasAsync(olt, cancellationToken);
                var porSerial = leituras.ToDictionary(l => l.Serial, StringComparer.OrdinalIgnoreCase);
                var agora = DateTime.UtcNow;

                foreach (var onu in olt.Onus)
                {
                    if (!porSerial.TryGetValue(onu.Serial, out var leitura))
                    {
                        continue;
                    }

                    onu.Status = leitura.Status;
                    onu.SinalDbm = leitura.SinalDbm;
                    onu.UltimaAtualizacao = agora;
                    onusAtualizadas++;
                }

                olt.UltimaSincronizacao = agora;
                await db.SaveChangesAsync(cancellationToken);

                mensagens.Add($"OLT {olt.Nome}: {onusAtualizadas} ONU(s) atualizada(s).");
                logger.LogInformation("Sincronização OLT {Olt}: {Count} ONUs.", olt.Nome, onusAtualizadas);

                return new RedeSincronizacaoResultado(1, onusAtualizadas, 0, mensagens);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao sincronizar OLT {Olt}.", olt.Nome);
                return new RedeSincronizacaoResultado(1, 0, 1, [$"Erro em {olt.Nome}: {ex.Message}"]);
            }
        }
    }
}
