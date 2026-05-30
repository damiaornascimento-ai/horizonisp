using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using horizonisp.Services;

namespace horizonisp.Pages.Relatorios
{
    public class IndexModel(IRelatorioService relatorioService) : PageModel
    {
        public RelatorioResumo Resumo { get; private set; } = null!;
        public IReadOnlyList<ClienteInadimplenteItem> Inadimplentes { get; private set; } = [];

        public async Task OnGetAsync()
        {
            Resumo = await relatorioService.ObterResumoAsync();
            Inadimplentes = await relatorioService.ObterInadimplentesAsync();
        }

        public async Task<IActionResult> OnGetExportarCsvAsync()
        {
            var inadimplentes = await relatorioService.ObterInadimplentesAsync();
            var sb = new StringBuilder();
            sb.AppendLine("ClienteId;Nome;Email;FaturasAtrasadas;ValorDevido;VencimentoMaisAntigo");

            foreach (var item in inadimplentes)
            {
                sb.Append(item.ClienteId).Append(';');
                sb.Append(EscaparCsv(item.Nome)).Append(';');
                sb.Append(EscaparCsv(item.Email)).Append(';');
                sb.Append(item.FaturasAtrasadas).Append(';');
                sb.Append(item.ValorDevido.ToString("F2", CultureInfo.InvariantCulture)).Append(';');
                sb.AppendLine(item.VencimentoMaisAntigo.ToString("yyyy-MM-dd"));
            }

            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
            return File(bytes, "text/csv", $"inadimplentes-{DateTime.UtcNow:yyyyMMdd}.csv");
        }

        private static string EscaparCsv(string valor)
        {
            if (valor.Contains(';') || valor.Contains('"'))
            {
                return $"\"{valor.Replace("\"", "\"\"")}\"";
            }

            return valor;
        }
    }
}
