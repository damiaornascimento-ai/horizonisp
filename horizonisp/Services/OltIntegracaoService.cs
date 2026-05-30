using System.Net.Http.Headers;
using System.Text.Json;
using horizonisp.Configuration;
using horizonisp.Models;
using horizonisp.Models.Enums;
using Microsoft.Extensions.Options;

namespace horizonisp.Services
{
    public record OnuLeituraRemota(string Serial, StatusOnu Status, int? SinalDbm);

    public interface IOltIntegracaoService
    {
        Task<IReadOnlyList<OnuLeituraRemota>> ObterLeiturasAsync(Olt olt, CancellationToken cancellationToken = default);
    }

    public class OltIntegracaoService(
        IOptions<HorizonIspOptions> options,
        IHttpClientFactory httpClientFactory,
        ILogger<OltIntegracaoService> logger) : IOltIntegracaoService
    {
        private readonly RedeOptions _config = options.Value.Rede;

        public async Task<IReadOnlyList<OnuLeituraRemota>> ObterLeiturasAsync(
            Olt olt,
            CancellationToken cancellationToken = default)
        {
            if (!_config.Habilitado)
            {
                return SimularLeituras(olt);
            }

            try
            {
                return await ConsultarOltAsync(olt, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Falha ao consultar OLT {Olt}. Usando simulação.", olt.Nome);
                return SimularLeituras(olt);
            }
        }

        private async Task<IReadOnlyList<OnuLeituraRemota>> ConsultarOltAsync(
            Olt olt,
            CancellationToken cancellationToken)
        {
            var client = httpClientFactory.CreateClient("Olt");
            var scheme = olt.PortaApi == 443 ? "https" : "http";
            var url = $"{scheme}://{olt.Host}:{olt.PortaApi}{_config.CaminhoOnus}";

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            if (!string.IsNullOrEmpty(olt.UsuarioApi))
            {
                var credenciais = Convert.ToBase64String(
                    System.Text.Encoding.UTF8.GetBytes($"{olt.UsuarioApi}:{olt.SenhaApi}"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credenciais);
            }

            var response = await client.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var leituras = await JsonSerializer.DeserializeAsync<List<OnuLeituraRemotaDto>>(
                stream,
                cancellationToken: cancellationToken);

            if (leituras is null || leituras.Count == 0)
            {
                return SimularLeituras(olt);
            }

            return leituras
                .Select(l => new OnuLeituraRemota(
                    l.Serial,
                    ParseStatus(l.Status),
                    l.SinalDbm))
                .ToList();
        }

        private static IReadOnlyList<OnuLeituraRemota> SimularLeituras(Olt olt)
        {
            var agora = DateTime.UtcNow;
            var leituras = new List<OnuLeituraRemota>();

            foreach (var onu in olt.Onus)
            {
                var sinalBase = onu.SinalDbm ?? (-20 - Math.Abs(onu.Serial.GetHashCode()) % 10);
                var variacao = (agora.Minute % 7) - 3;
                var sinal = sinalBase + variacao;

                var status = onu.Status == StatusOnu.Offline
                    ? StatusOnu.Offline
                    : StatusOnu.Online;

                leituras.Add(new OnuLeituraRemota(onu.Serial, status, sinal));
            }

            return leituras;
        }

        private static StatusOnu ParseStatus(string? status) =>
            status?.ToLowerInvariant() switch
            {
                "online" => StatusOnu.Online,
                "offline" => StatusOnu.Offline,
                _ => StatusOnu.Desconhecido
            };

        private sealed class OnuLeituraRemotaDto
        {
            public string Serial { get; set; } = string.Empty;
            public string? Status { get; set; }
            public int? SinalDbm { get; set; }
        }
    }
}
