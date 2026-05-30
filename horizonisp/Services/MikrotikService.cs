using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using horizonisp.Configuration;
using horizonisp.Models;
using Microsoft.Extensions.Options;

namespace horizonisp.Services
{
    public interface IMikrotikService
    {
        Task ProvisionarAssinaturaAsync(Assinatura assinatura, Plano plano, CancellationToken cancellationToken = default);
        Task SuspenderLoginAsync(string loginPppoe, CancellationToken cancellationToken = default);
        Task ReativarLoginAsync(string loginPppoe, CancellationToken cancellationToken = default);
        Task AtualizarSenhaAsync(string loginPppoe, string novaSenha, CancellationToken cancellationToken = default);
    }

    public class MikrotikService(
        IHttpClientFactory httpClientFactory,
        IOptions<HorizonIspOptions> options,
        ILogger<MikrotikService> logger) : IMikrotikService
    {
        private readonly MikrotikOptions _config = options.Value.Mikrotik;

        public async Task ProvisionarAssinaturaAsync(Assinatura assinatura, Plano plano, CancellationToken cancellationToken = default)
        {
            if (!EstaHabilitado())
            {
                logger.LogInformation(
                    "Mikrotik desabilitado. Simulando criação PPPoE para {Login}.",
                    assinatura.LoginPppoe);
                return;
            }

            await ExecutarRestAsync(HttpMethod.Put, "/rest/ppp/secret", new
            {
                name = assinatura.LoginPppoe,
                password = assinatura.SenhaPppoe,
                service = "pppoe",
                profile = _config.PerfilPppoe,
                comment = $"HorizonISP assinatura {assinatura.Id} - {plano.Nome}",
                disabled = assinatura.Status != Models.Enums.StatusAssinatura.Ativa ? "yes" : "no"
            }, cancellationToken);
        }

        public async Task SuspenderLoginAsync(string loginPppoe, CancellationToken cancellationToken = default)
        {
            if (!EstaHabilitado())
            {
                logger.LogInformation("Mikrotik desabilitado. Simulando suspensão de {Login}.", loginPppoe);
                return;
            }

            var id = await ObterSecretIdAsync(loginPppoe, cancellationToken);
            if (id is null)
            {
                return;
            }

            await ExecutarRestAsync(HttpMethod.Patch, $"/rest/ppp/secret/{id}", new
            {
                disabled = "yes"
            }, cancellationToken);

            await DesconectarSessaoAsync(loginPppoe, cancellationToken);
        }

        public async Task ReativarLoginAsync(string loginPppoe, CancellationToken cancellationToken = default)
        {
            if (!EstaHabilitado())
            {
                logger.LogInformation("Mikrotik desabilitado. Simulando reativação de {Login}.", loginPppoe);
                return;
            }

            var id = await ObterSecretIdAsync(loginPppoe, cancellationToken);
            if (id is null)
            {
                return;
            }

            await ExecutarRestAsync(HttpMethod.Patch, $"/rest/ppp/secret/{id}", new
            {
                disabled = "no"
            }, cancellationToken);
        }

        public async Task AtualizarSenhaAsync(string loginPppoe, string novaSenha, CancellationToken cancellationToken = default)
        {
            if (!EstaHabilitado())
            {
                logger.LogInformation("Mikrotik desabilitado. Simulando troca de senha de {Login}.", loginPppoe);
                return;
            }

            var id = await ObterSecretIdAsync(loginPppoe, cancellationToken);
            if (id is null)
            {
                return;
            }

            await ExecutarRestAsync(HttpMethod.Patch, $"/rest/ppp/secret/{id}", new
            {
                password = novaSenha
            }, cancellationToken);
        }

        private bool EstaHabilitado() =>
            _config.Habilitado
            && !string.IsNullOrWhiteSpace(_config.Host)
            && !string.IsNullOrWhiteSpace(_config.Usuario);

        private async Task<string?> ObterSecretIdAsync(string loginPppoe, CancellationToken cancellationToken)
        {
            var client = CriarCliente();
            var encodedLogin = Uri.EscapeDataString(loginPppoe);
            var response = await client.GetAsync($"/rest/ppp/secret?name={encodedLogin}", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Secret PPPoE {Login} não encontrado no Mikrotik.", loginPppoe);
                return null;
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

            if (document.RootElement.ValueKind != JsonValueKind.Array || document.RootElement.GetArrayLength() == 0)
            {
                logger.LogWarning("Secret PPPoE {Login} não encontrado no Mikrotik.", loginPppoe);
                return null;
            }

            if (document.RootElement[0].TryGetProperty(".id", out var id))
            {
                return id.GetString();
            }

            return null;
        }

        private async Task DesconectarSessaoAsync(string loginPppoe, CancellationToken cancellationToken)
        {
            var client = CriarCliente();
            var encodedLogin = Uri.EscapeDataString(loginPppoe);
            var response = await client.GetAsync($"/rest/ppp/active?name={encodedLogin}", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return;
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

            if (document.RootElement.ValueKind != JsonValueKind.Array)
            {
                return;
            }

            foreach (var sessao in document.RootElement.EnumerateArray())
            {
                if (sessao.TryGetProperty(".id", out var id) && id.GetString() is { Length: > 0 } sessionId)
                {
                    await ExecutarRestAsync(HttpMethod.Delete, $"/rest/ppp/active/{sessionId}", null, cancellationToken);
                }
            }
        }

        private async Task ExecutarRestAsync(
            HttpMethod method,
            string path,
            object? body,
            CancellationToken cancellationToken)
        {
            var client = CriarCliente();
            using var request = new HttpRequestMessage(method, path);

            if (body is not null)
            {
                request.Content = new StringContent(
                    JsonSerializer.Serialize(body),
                    Encoding.UTF8,
                    "application/json");
            }

            var response = await client.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var conteudo = await response.Content.ReadAsStringAsync(cancellationToken);
                logger.LogError(
                    "Falha Mikrotik {Metodo} {Path}. Status {Status}. Resposta: {Resposta}",
                    method,
                    path,
                    response.StatusCode,
                    conteudo);
            }
        }

        private HttpClient CriarCliente()
        {
            var scheme = _config.UsarSsl ? "https" : "http";
            var client = httpClientFactory.CreateClient("Mikrotik");
            client.BaseAddress = new Uri($"{scheme}://{_config.Host}:{_config.Porta}");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Basic",
                Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_config.Usuario}:{_config.Senha}")));
            return client;
        }
    }
}
