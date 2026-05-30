using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using horizonisp.Configuration;
using horizonisp.Context;
using horizonisp.Models;
using horizonisp.Models.Enums;

namespace horizonisp.Services
{
    public record FaturamentoResult(
        int FaturasGeradas,
        int FaturasMarcadasAtrasadas,
        int ClientesSuspensos,
        int ClientesReativados,
        int LembretesEnviados,
        int AvisosAtrasoEnviados);

    public interface IFaturamentoService
    {
        Task<FaturamentoResult> ExecutarRotinaDiariaAsync(CancellationToken cancellationToken = default);
        Task<Fatura> GerarFaturaAsync(Assinatura assinatura, Plano plano, string? referencia = null, CancellationToken cancellationToken = default);
        Task RegistrarPagamentoAsync(int faturaId, CancellationToken cancellationToken = default);
        Task GarantirPixAsync(Fatura fatura, CancellationToken cancellationToken = default);
    }

    public class FaturamentoService(
        AppDbContext db,
        IPixService pixService,
        IEmailService emailService,
        IMikrotikService mikrotikService,
        IOptions<HorizonIspOptions> options,
        ILogger<FaturamentoService> logger) : IFaturamentoService
    {
        private readonly FaturamentoOptions _config = options.Value.Faturamento;

        public async Task<FaturamentoResult> ExecutarRotinaDiariaAsync(CancellationToken cancellationToken = default)
        {
            await PreencherPixPendentesAsync(cancellationToken);
            var faturasGeradas = await GerarFaturasMensaisAsync(cancellationToken);
            var faturasMarcadasAtrasadas = await MarcarFaturasAtrasadasAsync(cancellationToken);
            var lembretesEnviados = await EnviarLembretesVencimentoAsync(cancellationToken);
            var avisosAtrasoEnviados = await EnviarAvisosAtrasoAsync(cancellationToken);
            var clientesSuspensos = await SuspenderInadimplentesAsync(cancellationToken);
            var clientesReativados = await ReativarClientesEmDiaAsync(cancellationToken);

            logger.LogInformation(
                "Rotina de faturamento concluída. Geradas={Geradas}, Atrasadas={Atrasadas}, Suspensos={Suspensos}, Reativados={Reativados}",
                faturasGeradas,
                faturasMarcadasAtrasadas,
                clientesSuspensos,
                clientesReativados);

            return new FaturamentoResult(
                faturasGeradas,
                faturasMarcadasAtrasadas,
                clientesSuspensos,
                clientesReativados,
                lembretesEnviados,
                avisosAtrasoEnviados);
        }

        public async Task<Fatura> GerarFaturaAsync(
            Assinatura assinatura,
            Plano plano,
            string? referencia = null,
            CancellationToken cancellationToken = default)
        {
            var referenciaAtual = referencia ?? DateTime.UtcNow.ToString("yyyy-MM");
            var vencimento = ObterDataVencimento(referenciaAtual);

            var fatura = new Fatura
            {
                AssinaturaId = assinatura.Id,
                Referencia = referenciaAtual,
                Valor = plano.PrecoMensal,
                DataVencimento = vencimento,
                Status = StatusFatura.Pendente,
                PixTxId = $"FAT{assinatura.Id:D6}{referenciaAtual.Replace("-", "")}"[..25]
            };

            fatura.PixCopiaCola = pixService.GerarCopiaCola(fatura.Valor, fatura.PixTxId);

            db.Faturas.Add(fatura);
            await db.SaveChangesAsync(cancellationToken);

            var cliente = await db.Clientes.FindAsync([assinatura.ClienteId], cancellationToken);
            if (cliente is not null)
            {
                await emailService.EnviarAsync(
                    cliente.Email,
                    $"Nova fatura {referenciaAtual} - Horizon ISP",
                    $"""
                    <p>Olá, {cliente.Nome}.</p>
                    <p>Sua fatura de referência <strong>{referenciaAtual}</strong> no valor de <strong>R$ {fatura.Valor:F2}</strong> foi gerada.</p>
                    <p>Vencimento: {fatura.DataVencimento:dd/MM/yyyy}</p>
                    <p>Acesse o portal do cliente para pagar via Pix.</p>
                    """);
            }

            return fatura;
        }

        public async Task RegistrarPagamentoAsync(int faturaId, CancellationToken cancellationToken = default)
        {
            var fatura = await db.Faturas
                .Include(f => f.Assinatura)
                    .ThenInclude(a => a.Cliente)
                .Include(f => f.Assinatura)
                    .ThenInclude(a => a.Faturas)
                .FirstOrDefaultAsync(f => f.Id == faturaId, cancellationToken);

            if (fatura is null)
            {
                return;
            }

            fatura.Status = StatusFatura.Paga;
            fatura.DataPagamento = DateTime.UtcNow;

            await db.SaveChangesAsync(cancellationToken);
            await ReativarClienteSeNecessarioAsync(fatura.Assinatura.Cliente, cancellationToken);
        }

        public Task GarantirPixAsync(Fatura fatura, CancellationToken cancellationToken = default)
        {
            if (!string.IsNullOrWhiteSpace(fatura.PixCopiaCola) || !pixService.EstaHabilitado)
            {
                return Task.CompletedTask;
            }

            fatura.PixTxId ??= $"FAT{fatura.AssinaturaId:D6}{fatura.Referencia.Replace("-", "")}"[..25];
            fatura.PixCopiaCola = pixService.GerarCopiaCola(fatura.Valor, fatura.PixTxId);
            return db.SaveChangesAsync(cancellationToken);
        }

        private async Task<int> GerarFaturasMensaisAsync(CancellationToken cancellationToken)
        {
            var referenciaAtual = DateTime.UtcNow.ToString("yyyy-MM");
            var assinaturas = await db.Assinaturas
                .Include(a => a.Plano)
                .Include(a => a.Faturas)
                .Where(a => a.Status == StatusAssinatura.Ativa)
                .ToListAsync(cancellationToken);

            var geradas = 0;

            foreach (var assinatura in assinaturas)
            {
                if (assinatura.Faturas.Any(f => f.Referencia == referenciaAtual))
                {
                    continue;
                }

                await GerarFaturaAsync(assinatura, assinatura.Plano, referenciaAtual, cancellationToken);
                geradas++;
            }

            return geradas;
        }

        private async Task<int> MarcarFaturasAtrasadasAsync(CancellationToken cancellationToken)
        {
            var hoje = DateTime.UtcNow.Date;

            var faturas = await db.Faturas
                .Where(f => f.Status == StatusFatura.Pendente && f.DataVencimento.Date < hoje)
                .ToListAsync(cancellationToken);

            foreach (var fatura in faturas)
            {
                fatura.Status = StatusFatura.Atrasada;
            }

            if (faturas.Count > 0)
            {
                await db.SaveChangesAsync(cancellationToken);
            }

            return faturas.Count;
        }

        private async Task<int> EnviarLembretesVencimentoAsync(CancellationToken cancellationToken)
        {
            var hoje = DateTime.UtcNow.Date;
            var limite = hoje.AddDays(_config.DiasLembreteVencimento);

            var faturas = await db.Faturas
                .Include(f => f.Assinatura)
                    .ThenInclude(a => a.Cliente)
                .Where(f =>
                    (f.Status == StatusFatura.Pendente || f.Status == StatusFatura.Atrasada)
                    && f.LembreteVencimentoEnviadoEm == null
                    && f.DataVencimento.Date >= hoje
                    && f.DataVencimento.Date <= limite)
                .ToListAsync(cancellationToken);

            foreach (var fatura in faturas)
            {
                var cliente = fatura.Assinatura.Cliente;
                await emailService.EnviarAsync(
                    cliente.Email,
                    $"Lembrete de vencimento - fatura {fatura.Referencia}",
                    $"""
                    <p>Olá, {cliente.Nome}.</p>
                    <p>Sua fatura <strong>{fatura.Referencia}</strong> vence em {fatura.DataVencimento:dd/MM/yyyy}.</p>
                    <p>Valor: R$ {fatura.Valor:F2}</p>
                    <p>Pague pelo portal do cliente via Pix.</p>
                    """);

                fatura.LembreteVencimentoEnviadoEm = DateTime.UtcNow;
            }

            if (faturas.Count > 0)
            {
                await db.SaveChangesAsync(cancellationToken);
            }

            return faturas.Count;
        }

        private async Task<int> EnviarAvisosAtrasoAsync(CancellationToken cancellationToken)
        {
            var faturas = await db.Faturas
                .Include(f => f.Assinatura)
                    .ThenInclude(a => a.Cliente)
                .Where(f => f.Status == StatusFatura.Atrasada && f.AvisoAtrasoEnviadoEm == null)
                .ToListAsync(cancellationToken);

            foreach (var fatura in faturas)
            {
                var cliente = fatura.Assinatura.Cliente;
                await emailService.EnviarAsync(
                    cliente.Email,
                    $"Fatura em atraso - {fatura.Referencia}",
                    $"""
                    <p>Olá, {cliente.Nome}.</p>
                    <p>Sua fatura <strong>{fatura.Referencia}</strong> está em atraso desde {fatura.DataVencimento:dd/MM/yyyy}.</p>
                    <p>Regularize o pagamento para evitar suspensão do serviço.</p>
                    """);

                fatura.AvisoAtrasoEnviadoEm = DateTime.UtcNow;
            }

            if (faturas.Count > 0)
            {
                await db.SaveChangesAsync(cancellationToken);
            }

            return faturas.Count;
        }

        private async Task<int> SuspenderInadimplentesAsync(CancellationToken cancellationToken)
        {
            var limite = DateTime.UtcNow.Date.AddDays(-_config.DiasParaSuspensao);

            var faturasAtrasadas = await db.Faturas
                .Include(f => f.Assinatura)
                    .ThenInclude(a => a.Cliente)
                .Where(f =>
                    f.Status == StatusFatura.Atrasada
                    && f.DataVencimento.Date <= limite)
                .ToListAsync(cancellationToken);

            var suspensos = 0;

            foreach (var grupo in faturasAtrasadas.GroupBy(f => f.Assinatura.ClienteId))
            {
                var cliente = grupo.First().Assinatura.Cliente;
                if (cliente.Status is StatusCliente.Suspenso or StatusCliente.Cancelado)
                {
                    continue;
                }

                cliente.Status = StatusCliente.Suspenso;

                foreach (var assinatura in await db.Assinaturas
                             .Where(a => a.ClienteId == cliente.Id && a.Status == StatusAssinatura.Ativa)
                             .ToListAsync(cancellationToken))
                {
                    assinatura.Status = StatusAssinatura.Suspensa;
                    await mikrotikService.SuspenderLoginAsync(assinatura.LoginPppoe, cancellationToken);
                }

                await emailService.EnviarAsync(
                    cliente.Email,
                    "Serviço suspenso por inadimplência",
                    $"""
                    <p>Olá, {cliente.Nome}.</p>
                    <p>Seu acesso foi suspenso por falta de pagamento.</p>
                    <p>Regularize suas faturas no portal do cliente para reativação.</p>
                    """);

                suspensos++;
            }

            if (suspensos > 0)
            {
                await db.SaveChangesAsync(cancellationToken);
            }

            return suspensos;
        }

        private async Task<int> ReativarClientesEmDiaAsync(CancellationToken cancellationToken)
        {
            var clientes = await db.Clientes
                .Include(c => c.Assinaturas)
                    .ThenInclude(a => a.Faturas)
                .Where(c => c.Status == StatusCliente.Suspenso || c.Status == StatusCliente.Inadimplente)
                .ToListAsync(cancellationToken);

            var reativados = 0;

            foreach (var cliente in clientes)
            {
                var possuiDebito = cliente.Assinaturas
                    .SelectMany(a => a.Faturas)
                    .Any(f => f.Status is StatusFatura.Pendente or StatusFatura.Atrasada);

                if (possuiDebito)
                {
                    continue;
                }

                await ReativarClienteSeNecessarioAsync(cliente, cancellationToken);
                reativados++;
            }

            return reativados;
        }

        private async Task ReativarClienteSeNecessarioAsync(Cliente cliente, CancellationToken cancellationToken)
        {
            if (cliente.Status is StatusCliente.Cancelado)
            {
                return;
            }

            cliente.Status = StatusCliente.Ativo;

            foreach (var assinatura in await db.Assinaturas
                         .Where(a => a.ClienteId == cliente.Id && a.Status == StatusAssinatura.Suspensa)
                         .ToListAsync(cancellationToken))
            {
                assinatura.Status = StatusAssinatura.Ativa;
                await mikrotikService.ReativarLoginAsync(assinatura.LoginPppoe, cancellationToken);
            }

            await db.SaveChangesAsync(cancellationToken);

            await emailService.EnviarAsync(
                cliente.Email,
                "Serviço reativado - Horizon ISP",
                $"""
                <p>Olá, {cliente.Nome}.</p>
                <p>Identificamos o pagamento e seu acesso foi reativado.</p>
                <p>Obrigado por continuar conosco.</p>
                """);
        }

        private DateTime ObterDataVencimento(string referencia)
        {
            var partes = referencia.Split('-');
            var ano = int.Parse(partes[0]);
            var mes = int.Parse(partes[1]);
            var dia = Math.Min(_config.DiaVencimento, DateTime.DaysInMonth(ano, mes));
            return new DateTime(ano, mes, dia, 0, 0, 0, DateTimeKind.Utc);
        }

        private async Task PreencherPixPendentesAsync(CancellationToken cancellationToken)
        {
            var faturas = await db.Faturas
                .Where(f =>
                    string.IsNullOrEmpty(f.PixCopiaCola)
                    && (f.Status == StatusFatura.Pendente || f.Status == StatusFatura.Atrasada))
                .ToListAsync(cancellationToken);

            foreach (var fatura in faturas)
            {
                await GarantirPixAsync(fatura, cancellationToken);
            }
        }
    }
}
