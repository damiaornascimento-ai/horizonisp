using System.Text.Json;

namespace horizonisp.Api
{
    public static class PixGatewayWebhookParser
    {
        public static IReadOnlyList<PixWebhookRequest> ExtrairPagamentos(string body)
        {
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            if (root.TryGetProperty("pix", out var pixArray) && pixArray.ValueKind == JsonValueKind.Array)
            {
                var lista = new List<PixWebhookRequest>();
                foreach (var item in pixArray.EnumerateArray())
                {
                    var txId = item.TryGetProperty("txid", out var tx) ? tx.GetString() : null;
                    if (string.IsNullOrWhiteSpace(txId))
                    {
                        continue;
                    }

                    var valor = 0m;
                    if (item.TryGetProperty("valor", out var v))
                    {
                        decimal.TryParse(v.GetString(), System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture, out valor);
                    }

                    var e2e = item.TryGetProperty("endToEndId", out var e) ? e.GetString() : null;
                    lista.Add(new PixWebhookRequest(txId, valor, e2e));
                }

                return lista;
            }

            if (root.TryGetProperty("payment", out var payment))
            {
                var txId = payment.TryGetProperty("externalReference", out var ext)
                    ? ext.GetString()
                    : payment.TryGetProperty("id", out var id) ? id.GetString() : null;

                if (string.IsNullOrWhiteSpace(txId))
                {
                    return [];
                }

                var valor = payment.TryGetProperty("value", out var val) ? val.GetDecimal() : 0m;
                return [new PixWebhookRequest(txId, valor, null)];
            }

            if (root.TryGetProperty("txId", out var txIdProp))
            {
                var txId = txIdProp.GetString();
                var valor = root.TryGetProperty("valor", out var v) ? v.GetDecimal() : 0m;
                var e2e = root.TryGetProperty("endToEndId", out var e) ? e.GetString() : null;
                if (!string.IsNullOrWhiteSpace(txId))
                {
                    return [new PixWebhookRequest(txId, valor, e2e)];
                }
            }

            return [];
        }
    }
}
