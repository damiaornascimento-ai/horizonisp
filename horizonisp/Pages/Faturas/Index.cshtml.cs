using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using horizonisp.Context;
using horizonisp.Models.Enums;
using horizonisp.Services;

namespace horizonisp.Pages.Faturas
{
    public class IndexModel(IFaturamentoService faturamentoService, AppDbContext db) : PageModel
    {
        public IList<Models.Fatura> Faturas { get; private set; } = [];
        public StatusFatura? FiltroStatus { get; private set; }

        public async Task OnGetAsync(StatusFatura? status)
        {
            FiltroStatus = status;

            var query = db.Faturas
                .Include(f => f.Assinatura)
                    .ThenInclude(a => a.Cliente)
                .Include(f => f.Assinatura)
                    .ThenInclude(a => a.Plano)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(f => f.Status == status.Value);
            }

            Faturas = await query
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
