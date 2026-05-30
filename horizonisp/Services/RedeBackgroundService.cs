using Microsoft.Extensions.Options;
using horizonisp.Configuration;

namespace horizonisp.Services
{
    public class RedeBackgroundService(
        IServiceScopeFactory scopeFactory,
        IOptions<HorizonIspOptions> options,
        ILogger<RedeBackgroundService> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var intervaloMinutos = Math.Max(5, options.Value.Rede.IntervaloSincronizacaoMinutos);
            var intervalo = TimeSpan.FromMinutes(intervaloMinutos);

            await ExecutarComEscopoAsync(stoppingToken);

            using var timer = new PeriodicTimer(intervalo);
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await ExecutarComEscopoAsync(stoppingToken);
            }
        }

        private async Task ExecutarComEscopoAsync(CancellationToken cancellationToken)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var sincronizacao = scope.ServiceProvider.GetRequiredService<IRedeSincronizacaoService>();
                var resultado = await sincronizacao.SincronizarTodasAsync(cancellationToken);

                if (resultado.OnusAtualizadas > 0)
                {
                    logger.LogInformation(
                        "Sync rede: {Olts} OLT(s), {Onus} ONU(s) atualizada(s).",
                        resultado.OltsProcessadas,
                        resultado.OnusAtualizadas);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Erro ao sincronizar rede em background.");
            }
        }
    }
}
