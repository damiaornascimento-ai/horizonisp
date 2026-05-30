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
    }
}
