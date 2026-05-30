namespace horizonisp.Services
{
    public record PixCobrancaResult(
        string TxId,
        string CopiaCola,
        string? GatewayRef,
        DateTime? ExpiracaoEm);
}
