using Microsoft.AspNetCore.Mvc.RazorPages;
using horizonisp.Services;

namespace horizonisp.Pages
{
    public class IndexModel(IDashboardService dashboardService) : PageModel
    {
        public DashboardResumo Resumo { get; private set; } = null!;

        public async Task OnGetAsync()
        {
            Resumo = await dashboardService.ObterResumoAsync();
        }
    }
}
