using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using horizonisp.Configuration;
using horizonisp.Models;
using Microsoft.Extensions.Options;

namespace horizonisp.Services
{
    public interface IPixGatewayService
    {
        bool EstaAtivo { get; }
        Task<PixCobrancaResult> CriarCobrancaAsync(
            Fatura fatura,
            Cliente cliente,
            string txId,
            CancellationToken cancellationToken = default);
    }

    public class PixGatewayService(
        IHttpClientFactory httpClientFactory,
        IOptions<HorizonIspOptions> options,
        ILogger<PixGatewayService> logger) : IPixGatewayService
    {
        private readonly PixOptions _pix = options.Value.Pix;

        public bool EstaAtivo =>
            _pix.Habilitado
            && string.Equals(_pix.Modo, "Gateway", StringComparison.OrdinalIgnoreCase);

        public async Task<PixCobrancaResult> CriarCobrancaAsync(
            Fatura fatura,
            Cliente cliente,
            string txId,
            CancellationToken cancellationToken = default)
        {
            if (string.Equals(_pix.Provedor, "Efi", StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(_pix.BaseUrl)
                && !string.IsNullOrWhiteSpace(_pix.ClientId))
            {
                try
                {
                    return await CriarCobrancaEfiAsync(fatura, cliente, txId, cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Falha Efi Pix; usando simulação.");
                }
            }

            if (string.Equals(_pix.Provedor, "Asaas", StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(_pix.BaseUrl)
                && !string.IsNullOrWhiteSpace(_pix.ClientSecret))
            {
                try
                {
                    return await CriarCobrancaAsaasAsync(fatura, cliente, txId, cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Falha Asaas Pix; usando simulação.");
                }
            }

            return CriarCobrancaSimulada(fatura, txId);
        }

        private PixCobrancaResult CriarCobrancaSimulada(Fatura fatura, string txId)
        {
            var copiaCola = PixEmvGenerator.Gerar(_pix, fatura.Valor, txId);
            var expiracao = DateTime.UtcNow.AddSeconds(_pix.ExpiracaoSegundos);

            logger.LogInformation(
                "Pix Gateway simulado: fatura {FaturaId}, txId {TxId}, expira {Expiracao}.",
                fatura.Id,
                txId,
                expiracao);

            return new PixCobrancaResult(txId, copiaCola, $"SIM-{fatura.Id}", expiracao);
        }

        private async Task<PixCobrancaResult> CriarCobrancaEfiAsync(
            Fatura fatura,
            Cliente cliente,
            string txId,
            CancellationToken cancellationToken)
        {
            var client = httpClientFactory.CreateClient("PixGateway");
            var baseUrl = _pix.BaseUrl.TrimEnd('/');

            var credenciais = Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{_pix.ClientId}:{_pix.ClientSecret}"));

            using var tokenRequest = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/oauth/token");
            tokenRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", credenciais);
            tokenRequest.Content = new FormUrlEncodedContent([
                new KeyValuePair<string, string>("grant_type", "client_credentials")
            ]);

            var tokenResponse = await client.SendAsync(tokenRequest, cancellationToken);
            tokenResponse.EnsureSuccessStatusCode();

            await using var tokenStream = await tokenResponse.Content.ReadAsStreamAsync(cancellationToken);
            var tokenJson = await JsonSerializer.DeserializeAsync<EfiTokenResponse>(
                tokenStream,
                JsonOptions,
                cancellationToken);

            var accessToken = tokenJson?.AccessToken
                ?? throw new InvalidOperationException("Token Efi inválido.");

            var payload = new
            {
                calendario = new { expiracao = _pix.ExpiracaoSegundos },
                devedor = new
                {
                    cpf = NormalizarDocumento(cliente.Documento).Length == 11
                        ? NormalizarDocumento(cliente.Documento)
                        : null,
                    cnpj = NormalizarDocumento(cliente.Documento).Length == 14
                        ? NormalizarDocumento(cliente.Documento)
                        : null,
                    nome = cliente.Nome
                },
                valor = new { original = fatura.Valor.ToString("F2", System.Globalization.CultureInfo.InvariantCulture) },
                chave = _pix.Chave,
                solicitacaoPagador = $"Fatura {fatura.Referencia} — Horizon ISP"
            };

            using var cobRequest = new HttpRequestMessage(HttpMethod.Put, $"{baseUrl}/v2/cob/{txId}");
            cobRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            cobRequest.Content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

            var cobResponse = await client.SendAsync(cobRequest, cancellationToken);
            cobResponse.EnsureSuccessStatusCode();

            await using var cobStream = await cobResponse.Content.ReadAsStreamAsync(cancellationToken);
            var cobJson = await JsonSerializer.DeserializeAsync<EfiCobResponse>(
                cobStream,
                JsonOptions,
                cancellationToken);

            var copiaCola = cobJson?.PixCopiaECola
                ?? throw new InvalidOperationException("Resposta Efi sem pixCopiaECola.");

            var expiracao = DateTime.UtcNow.AddSeconds(_pix.ExpiracaoSegundos);
            return new PixCobrancaResult(txId, copiaCola, cobJson?.Loc?.Id.ToString(), expiracao);
        }

        private async Task<PixCobrancaResult> CriarCobrancaAsaasAsync(
            Fatura fatura,
            Cliente cliente,
            string txId,
            CancellationToken cancellationToken)
        {
            var client = httpClientFactory.CreateClient("PixGateway");
            var baseUrl = _pix.BaseUrl.TrimEnd('/');

            var payload = new
            {
                billingType = "PIX",
                value = fatura.Valor,
                dueDate = fatura.DataVencimento.ToString("yyyy-MM-dd"),
                description = $"Fatura {fatura.Referencia} — {cliente.Nome}",
                externalReference = txId
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/v3/payments");
            request.Headers.Add("access_token", _pix.ClientSecret);
            request.Content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

            var response = await client.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var json = await JsonSerializer.DeserializeAsync<AsaasPaymentResponse>(
                stream,
                JsonOptions,
                cancellationToken);

            var copiaCola = json?.PixQrCode?.Payload
                ?? throw new InvalidOperationException("Resposta Asaas sem QR Pix.");

            return new PixCobrancaResult(
                txId,
                copiaCola,
                json?.Id,
                DateTime.UtcNow.AddSeconds(_pix.ExpiracaoSegundos));
        }

        private static string NormalizarDocumento(string documento) =>
            new string(documento.Where(char.IsDigit).ToArray());

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private sealed class EfiTokenResponse
        {
            [JsonPropertyName("access_token")]
            public string? AccessToken { get; set; }
        }

        private sealed class EfiCobResponse
        {
            [JsonPropertyName("pixCopiaECola")]
            public string? PixCopiaECola { get; set; }

            public EfiLoc? Loc { get; set; }
        }

        private sealed class EfiLoc
        {
            public int Id { get; set; }
        }

        private sealed class AsaasPaymentResponse
        {
            public string? Id { get; set; }
            public AsaasPixQr? PixQrCode { get; set; }
        }

        private sealed class AsaasPixQr
        {
            public string? Payload { get; set; }
        }
    }
}