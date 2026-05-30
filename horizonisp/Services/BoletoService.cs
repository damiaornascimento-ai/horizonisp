using horizonisp.Configuration;
using horizonisp.Models;
using Microsoft.Extensions.Options;

namespace horizonisp.Services
{
    public record BoletoGerado(string NossoNumero, string CodigoBarras, string LinhaDigitavel);

    public interface IBoletoService
    {
        bool EstaHabilitado { get; }
        BoletoGerado Gerar(Fatura fatura);
    }

    public class BoletoService(IOptions<HorizonIspOptions> options) : IBoletoService
    {
        private readonly BoletoOptions _config = options.Value.Boleto;

        public bool EstaHabilitado => _config.Habilitado;

        public BoletoGerado Gerar(Fatura fatura)
        {
            var nossoNumero = fatura.Id.ToString("D10");
            var fator = CalcularFatorVencimento(fatura.DataVencimento);
            var valor = ((long)(fatura.Valor * 100)).ToString("D10");

            var agencia = ApenasDigitos(_config.Agencia).PadLeft(4, '0')[..4];
            var conta = ApenasDigitos(_config.Conta).PadLeft(5, '0')[..5];
            var carteira = ApenasDigitos(_config.Carteira).PadLeft(3, '0')[..3];
            var banco = ApenasDigitos(_config.Banco).PadLeft(3, '0')[..3];

            var campoLivre = $"{carteira}{nossoNumero}{agencia}{conta}".PadRight(25, '0')[..25];
            var codigoSemDv = $"{banco}9{fator}{valor}{campoLivre}";
            var dvGeral = Modulo11(codigoSemDv).ToString();
            var codigoBarras = $"{banco}9{dvGeral}{fator}{valor}{campoLivre}";

            var linhaDigitavel = MontarLinhaDigitavel(codigoBarras);

            return new BoletoGerado(nossoNumero, codigoBarras, linhaDigitavel);
        }

        private static string MontarLinhaDigitavel(string codigoBarras)
        {
            var campo1 = codigoBarras[..4] + codigoBarras[19..24];
            var campo2 = codigoBarras[24..34];
            var campo3 = codigoBarras[34..44];
            var campo4 = codigoBarras[4].ToString();
            var campo5 = codigoBarras[5..19];

            return $"{FormatarCampo(campo1)} {FormatarCampo(campo2)} {FormatarCampo(campo3)} {campo4} {campo5}";
        }

        private static string FormatarCampo(string campo)
        {
            var dv = Modulo10(campo);
            return campo.Length <= 5
                ? $"{campo}{dv}"
                : $"{campo[..5]}.{campo[5..]}{dv}";
        }

        private static int Modulo10(string numero)
        {
            var soma = 0;
            var multiplicador = 2;

            for (var i = numero.Length - 1; i >= 0; i--)
            {
                var produto = (numero[i] - '0') * multiplicador;
                soma += produto > 9 ? produto - 9 : produto;
                multiplicador = multiplicador == 2 ? 1 : 2;
            }

            var resto = soma % 10;
            return resto == 0 ? 0 : 10 - resto;
        }

        private static int Modulo11(string numero)
        {
            var soma = 0;
            var peso = 2;

            for (var i = numero.Length - 1; i >= 0; i--)
            {
                soma += (numero[i] - '0') * peso;
                peso = peso == 9 ? 2 : peso + 1;
            }

            var resto = soma % 11;
            var dv = 11 - resto;

            return dv is 0 or 10 or 11 ? 1 : dv;
        }

        private static string CalcularFatorVencimento(DateTime vencimento)
        {
            var baseDate = new DateTime(1997, 10, 7, 0, 0, 0, DateTimeKind.Utc);
            var dias = (int)(vencimento.Date - baseDate.Date).TotalDays;
            if (dias < 0)
            {
                dias = 0;
            }

            return dias.ToString("D4");
        }

        private static string ApenasDigitos(string valor) =>
            new string(valor.Where(char.IsDigit).ToArray());
    }
}
