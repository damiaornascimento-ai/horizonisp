namespace horizonisp.Configuration
{
    public class HorizonIspOptions
    {
        public const string SectionName = "HorizonIsp";

        public FaturamentoOptions Faturamento { get; set; } = new();
        public PixOptions Pix { get; set; } = new();
        public EmailOptions Email { get; set; } = new();
        public MikrotikOptions Mikrotik { get; set; } = new();
        public ApiOptions Api { get; set; } = new();
        public WhatsAppOptions WhatsApp { get; set; } = new();
        public RedeOptions Rede { get; set; } = new();
        public BoletoOptions Boleto { get; set; } = new();
        public NfseOptions Nfse { get; set; } = new();
    }

    public class FaturamentoOptions
    {
        public int DiaVencimento { get; set; } = 10;
        public int DiasParaSuspensao { get; set; } = 5;
        public int DiasLembreteVencimento { get; set; } = 3;
        public int IntervaloRotinaHoras { get; set; } = 6;
    }

    public class PixOptions
    {
        public bool Habilitado { get; set; } = true;

        /// <summary>Estatico = QR local; Gateway = cobrança via PSP (Efi, Asaas).</summary>
        public string Modo { get; set; } = "Estatico";

        public string Provedor { get; set; } = "Simulado";
        public string BaseUrl { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public int ExpiracaoSegundos { get; set; } = 86400;

        public string Chave { get; set; } = "horizonisp@email.com";
        public string NomeRecebedor { get; set; } = "Horizon ISP";
        public string Cidade { get; set; } = "SAO PAULO";
        public string WebhookToken { get; set; } = "altere-este-token";
    }

    public class EmailOptions
    {
        public bool Habilitado { get; set; }
        public string Host { get; set; } = string.Empty;
        public int Porta { get; set; } = 587;
        public string Remetente { get; set; } = "noreply@horizonisp.local";
        public string Usuario { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
        public bool UsarSsl { get; set; } = true;
    }

    public class MikrotikOptions
    {
        public bool Habilitado { get; set; }
        public string Host { get; set; } = string.Empty;
        public int Porta { get; set; } = 443;
        public string Usuario { get; set; } = "admin";
        public string Senha { get; set; } = string.Empty;
        public bool UsarSsl { get; set; } = true;
        public string PerfilPppoe { get; set; } = "default";
    }

    public class ApiOptions
    {
        public bool Habilitado { get; set; } = true;
        public string Chave { get; set; } = "altere-esta-api-key";
    }

    public class WhatsAppOptions
    {
        public bool Habilitado { get; set; }
        public string Provedor { get; set; } = "Evolution";
        public string BaseUrl { get; set; } = string.Empty;
        public string Instancia { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }

    public class RedeOptions
    {
        public bool Habilitado { get; set; }
        public int IntervaloSincronizacaoMinutos { get; set; } = 15;
        public string CaminhoOnus { get; set; } = "/api/onus";
    }

    public class BoletoOptions
    {
        public bool Habilitado { get; set; } = true;
        public string Banco { get; set; } = "341";
        public string Carteira { get; set; } = "109";
        public string Agencia { get; set; } = "1234";
        public string Conta { get; set; } = "56789";
        public string Cedente { get; set; } = "Horizon ISP Ltda";
    }

    public class NfseOptions
    {
        public bool Habilitado { get; set; } = true;
        public bool EmitirAutomaticamente { get; set; } = true;
        public string Provedor { get; set; } = "Simulado";
        public string BaseUrl { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string CnpjPrestador { get; set; } = "00.000.000/0001-00";
        public string InscricaoMunicipal { get; set; } = string.Empty;
        public string CodigoServico { get; set; } = "01.07";
        public decimal AliquotaIss { get; set; } = 2.0m;
        public string Municipio { get; set; } = "São Paulo";
    }
}
