using horizonisp.Configuration;

namespace horizonisp.Services
{
    public interface IPixService
    {
        bool EstaHabilitado { get; }
        bool UsaGateway { get; }
        string GerarCopiaColaEstatico(decimal valor, string txId);
        Task AplicarCobrancaAsync(
            Models.Fatura fatura,
            Models.Cliente? cliente,
            string txId,
            CancellationToken cancellationToken = default);
    }

    public class PixService(
        IPixGatewayService pixGateway,
        Microsoft.Extensions.Options.IOptions<HorizonIspOptions> options) : IPixService
    {
        private readonly PixOptions _pix = options.Value.Pix;

        public bool EstaHabilitado => _pix.Habilitado && !string.IsNullOrWhiteSpace(_pix.Chave);

        public bool UsaGateway =>
            EstaHabilitado
            && string.Equals(_pix.Modo, "Gateway", StringComparison.OrdinalIgnoreCase);

        public async Task AplicarCobrancaAsync(
            Models.Fatura fatura,
            Models.Cliente? cliente,
            string txId,
            CancellationToken cancellationToken = default)
        {
            if (!EstaHabilitado)
            {
                return;
            }

            fatura.PixTxId = txId;

            if (UsaGateway && cliente is not null)
            {
                var cobranca = await pixGateway.CriarCobrancaAsync(fatura, cliente, txId, cancellationToken);
                fatura.PixCopiaCola = cobranca.CopiaCola;
                fatura.PixGatewayRef = cobranca.GatewayRef;
                fatura.PixExpiracaoEm = cobranca.ExpiracaoEm;
                return;
            }

            fatura.PixCopiaCola = GerarCopiaColaEstatico(fatura.Valor, txId);
            fatura.PixGatewayRef = null;
            fatura.PixExpiracaoEm = null;
        }

        public string GerarCopiaColaEstatico(decimal valor, string txId) =>
            EstaHabilitado ? PixEmvGenerator.Gerar(_pix, valor, txId) : string.Empty;
    }
}
