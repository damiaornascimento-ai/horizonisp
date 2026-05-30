using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using horizonisp.Configuration;

namespace horizonisp.Services
{
    public interface IWhatsAppService
    {
        Task EnviarAsync(string telefone, string mensagem, CancellationToken cancellationToken = default);
    }

    public class WhatsAppService(
        IHttpClientFactory httpClientFactory,
        IOptions<HorizonIspOptions> options,
        ILogger<WhatsAppService> logger) : IWhatsAppService
    {
        private readonly WhatsAppOptions _config = options.Value.WhatsApp;

        public async Task EnviarAsync(string telefone, string mensagem, CancellationToken cancellationToken = default)
        {
            var numero = NormalizarTelefone(telefone);
            if (string.IsNullOrEmpty(numero))
            {
                logger.LogWarning("Telefone inválido para WhatsApp: {Telefone}", telefone);
                return;
            }

            if (!_config.Habilitado || string.IsNullOrWhiteSpace(_config.BaseUrl))
            {
                logger.LogInformation(
                    "WhatsApp simulado → {Telefone}: {Mensagem}",
                    numero,
                    mensagem);
                return;
            }

            var client = httpClientFactory.CreateClient("WhatsApp");
            client.BaseAddress = new Uri(_config.BaseUrl.TrimEnd('/') + "/");
            client.DefaultRequestHeaders.Remove("apikey");
            client.DefaultRequestHeaders.Add("apikey", _config.Token);

            var payload = new
            {
                number = numero,
                text = mensagem
            };

            var url = $"message/sendText/{_config.Instancia}";
            var response = await client.PostAsJsonAsync(url, payload, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                logger.LogError(
                    "Falha WhatsApp {Status}. Resposta: {Body}",
                    response.StatusCode,
                    body);
            }
        }

        private static string NormalizarTelefone(string telefone)
        {
            var digits = new string(telefone.Where(char.IsDigit).ToArray());
            if (digits.Length is 10 or 11)
            {
                digits = "55" + digits;
            }

            return digits.Length >= 12 ? digits : string.Empty;
        }
    }
}
