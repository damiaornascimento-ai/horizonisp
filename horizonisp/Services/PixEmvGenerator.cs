using System.Text;
using horizonisp.Configuration;

namespace horizonisp.Services
{
    public static class PixEmvGenerator
    {
        public static string Gerar(PixOptions pix, decimal valor, string txId)
        {
            var merchantAccount = MontarTlv("00", "br.gov.bcb.pix")
                + MontarTlv("01", pix.Chave.Trim());

            var additionalData = MontarTlv("05", NormalizarTxId(txId));

            var payloadSemCrc =
                MontarTlv("00", "01") +
                MontarTlv("26", merchantAccount) +
                MontarTlv("52", "0000") +
                MontarTlv("53", "986") +
                MontarTlv("54", valor.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)) +
                MontarTlv("58", "BR") +
                MontarTlv("59", NormalizarTexto(pix.NomeRecebedor, 25)) +
                MontarTlv("60", NormalizarTexto(pix.Cidade, 15)) +
                MontarTlv("62", additionalData);

            return payloadSemCrc + MontarCrc(payloadSemCrc);
        }

        private static string NormalizarTxId(string txId)
        {
            var normalizado = new string(txId
                .ToUpperInvariant()
                .Where(c => char.IsLetterOrDigit(c))
                .ToArray());

            if (string.IsNullOrEmpty(normalizado))
            {
                normalizado = "HISP";
            }

            return normalizado.Length > 25 ? normalizado[..25] : normalizado;
        }

        private static string NormalizarTexto(string valor, int maxLength)
        {
            var texto = RemoverAcentos(valor.Trim().ToUpperInvariant());
            return texto.Length > maxLength ? texto[..maxLength] : texto;
        }

        private static string RemoverAcentos(string texto)
        {
            var normalized = texto.Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder();

            foreach (var c in normalized)
            {
                var category = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (category != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    builder.Append(c);
                }
            }

            return builder.ToString().Normalize(NormalizationForm.FormC);
        }

        private static string MontarTlv(string id, string valor) =>
            $"{id}{valor.Length:D2}{valor}";

        private static string MontarCrc(string payload)
        {
            const ushort polynomial = 0x1021;
            ushort crc = 0xFFFF;
            var bytes = Encoding.UTF8.GetBytes(payload + "6304");

            foreach (var b in bytes)
            {
                crc ^= (ushort)(b << 8);
                for (var i = 0; i < 8; i++)
                {
                    crc = (crc & 0x8000) != 0
                        ? (ushort)((crc << 1) ^ polynomial)
                        : (ushort)(crc << 1);
                }
            }

            return "6304" + crc.ToString("X4");
        }
    }
}
