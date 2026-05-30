namespace horizonisp.Api
{
    public record PixWebhookRequest(string TxId, decimal Valor, string? EndToEndId);
}
