using Microsoft.Extensions.Options;
using horizonisp.Configuration;

namespace horizonisp.Services
{
    public class FaturamentoBackgroundService(
        IServiceScopeFactory scopeFactory,
        IOptions<HorizonIspOptions> options,
        ILogger<FaturamentoBackgroundService> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await ExecutarComEscopoAsync(stoppingToken);

            var intervalo = TimeSpan.FromHours(Math.Max(1, options.Value.Faturamento.IntervaloRotinaHoras));

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
                var faturamento = scope.ServiceProvider.GetRequiredService<IFaturamentoService>();
                await faturamento.ExecutarRotinaDiariaAsync(cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Erro ao executar rotina de faturamento em background.");
            }
        }
    }
}
