using System.Text;
using System.Text.Json;
using horizonisp.Configuration;
using horizonisp.Models;
using horizonisp.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace horizonisp.Services
{
    public interface INfseService
    {
        bool EstaHabilitado { get; }
        Task<NotaFiscalServico?> ObterPorFaturaAsync(int faturaId);
        Task<IReadOnlyList<NotaFiscalServico>> ListarAsync(StatusNfse? status = null);
        Task EmitirAposPagamentoAsync(int faturaId, CancellationToken cancellationToken = default);
    }

    public class NfseService(
        Context.AppDbContext db,
        IHttpClientFactory httpClientFactory,
        IOptions<HorizonIspOptions> options,
        ILogger<NfseService> logger) : INfseService
    {
        private readonly NfseOptions _config = options.Value.Nfse;

        public bool EstaHabilitado => _config.Habilitado;

        public Task<NotaFiscalServico?> ObterPorFaturaAsync(int faturaId) =>
            db.NotasFiscaisServico
                .Include(n => n.Fatura)
                    .ThenInclude(f => f.Assinatura)
                        .ThenInclude(a => a.Cliente)
                .FirstOrDefaultAsync(n => n.FaturaId == faturaId);

        public async Task<IReadOnlyList<NotaFiscalServico>> ListarAsync(StatusNfse? status = null)
        {
            var query = db.NotasFiscaisServico
                .Include(n => n.Fatura)
                    .ThenInclude(f => f.Assinatura)
                        .ThenInclude(a => a.Cliente)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(n => n.Status == status.Value);
            }

            return await query
                .OrderByDescending(n => n.DataEmissao)
                .ThenByDescending(n => n.Id)
                .ToListAsync();
        }

        public async Task EmitirAposPagamentoAsync(int faturaId, CancellationToken cancellationToken = default)
        {
            if (!_config.Habilitado || !_config.EmitirAutomaticamente)
            {
                return;
            }

            if (await db.NotasFiscaisServico.AnyAsync(n => n.FaturaId == faturaId, cancellationToken))
            {
                return;
            }

            var fatura = await db.Faturas
                .Include(f => f.Assinatura)
                    .ThenInclude(a => a.Cliente)
                .Include(f => f.Assinatura)
                    .ThenInclude(a => a.Plano)
                .FirstOrDefaultAsync(f => f.Id == faturaId, cancellationToken);

            if (fatura is null || fatura.Status != StatusFatura.Paga)
            {
                return;
            }

            var nota = new NotaFiscalServico
            {
                FaturaId = faturaId,
                Valor = fatura.Valor,
                Status = StatusNfse.Processando,
                Discriminacao = $"Assinatura {fatura.Assinatura.Plano.Nome} — ref. {fatura.Referencia}"
            };

            db.NotasFiscaisServico.Add(nota);
            await db.SaveChangesAsync(cancellationToken);

            try
            {
                await EmitirAsync(nota, fatura, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Falha ao emitir NFS-e para fatura {FaturaId}.", faturaId);
                nota.Status = StatusNfse.Rejeitada;
                nota.MensagemErro = ex.Message;
                await db.SaveChangesAsync(cancellationToken);
            }
        }

        private async Task EmitirAsync(
            NotaFiscalServico nota,
            Fatura fatura,
            CancellationToken cancellationToken)
        {
            if (string.Equals(_config.Provedor, "Simulado", StringComparison.OrdinalIgnoreCase)
                || string.IsNullOrWhiteSpace(_config.BaseUrl))
            {
                EmitirSimulada(nota, fatura);
                await db.SaveChangesAsync(cancellationToken);
                return;
            }

            await EmitirViaApiAsync(nota, fatura, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
        }

        private void EmitirSimulada(NotaFiscalServico nota, Fatura fatura)
        {
            var sequencia = nota.Id.ToString("D8");
            nota.Numero = $"{DateTime.UtcNow:yyyy}{sequencia[..6]}";
            nota.CodigoVerificacao = Guid.NewGuid().ToString("N")[..16].ToUpperInvariant();
            nota.Status = StatusNfse.Autorizada;
            nota.DataEmissao = DateTime.UtcNow;
            nota.LinkPdf = $"/Nfse/Detalhe?id={nota.Id}";
            nota.MensagemErro = null;

            logger.LogInformation(
                "NFS-e simulada emitida: fatura {FaturaId}, número {Numero}.",
                fatura.Id,
                nota.Numero);
        }

        private async Task EmitirViaApiAsync(
            NotaFiscalServico nota,
            Fatura fatura,
            CancellationToken cancellationToken)
        {
            var client = httpClientFactory.CreateClient("Nfse");
            var baseUrl = _config.BaseUrl.TrimEnd('/');
            var cliente = fatura.Assinatura.Cliente;

            var payload = new
            {
                cnpjPrestador = ApenasDigitos(_config.CnpjPrestador),
                inscricaoMunicipal = _config.InscricaoMunicipal,
                codigoServico = _config.CodigoServico,
                aliquotaIss = _config.AliquotaIss,
                valor = fatura.Valor,
                discriminacao = nota.Discriminacao,
                tomador = new
                {
                    nome = cliente.Nome,
                    documento = ApenasDigitos(cliente.Documento),
                    email = cliente.Email
                },
                referenciaExterna = fatura.Id.ToString()
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/nfse");
            if (!string.IsNullOrWhiteSpace(_config.Token))
            {
                request.Headers.Add("Authorization", $"Bearer {_config.Token}");
            }

            request.Content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

            var response = await client.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var json = await JsonSerializer.DeserializeAsync<NfseApiResponse>(
                stream,
                cancellationToken: cancellationToken);

            nota.Numero = json?.Numero ?? nota.Id.ToString("D8");
            nota.CodigoVerificacao = json?.CodigoVerificacao ?? Guid.NewGuid().ToString("N")[..12];
            nota.Status = StatusNfse.Autorizada;
            nota.DataEmissao = DateTime.UtcNow;
            nota.LinkPdf = json?.LinkPdf;
        }

        private static string ApenasDigitos(string valor) =>
            new string(valor.Where(char.IsDigit).ToArray());

        private sealed class NfseApiResponse
        {
            public string? Numero { get; set; }
            public string? CodigoVerificacao { get; set; }
            public string? LinkPdf { get; set; }
        }
    }
}
