using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using horizonisp.Context;
using horizonisp.Services;

namespace horizonisp.Pages.Faturas
{
    public class IndexModel(IFaturamentoService faturamentoService, AppDbContext db) : PageModel
    {
        public IList<Models.Fatura> Faturas { get; private set; } = [];

        public async Task OnGetAsync()
        {
            Faturas = await db.Faturas
                .Include(f => f.Assinatura)
                    .ThenInclude(a => a.Cliente)
                .Include(f => f.Assinatura)
                    .ThenInclude(a => a.Plano)
                .OrderByDescending(f => f.DataVencimento)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostPagarAsync(int id)
        {
            await faturamentoService.RegistrarPagamentoAsync(id);
            return RedirectToPage();
        }
    }
}
