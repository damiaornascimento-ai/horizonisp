using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using horizonisp.Context;
using horizonisp.Models;

namespace horizonisp.Pages.Planos
{
    public class IndexModel(AppDbContext db) : PageModel
    {
        public IList<Plano> Planos { get; private set; } = [];

        public async Task OnGetAsync()
        {
            Planos = await db.Planos
                .OrderBy(p => p.PrecoMensal)
                .ToListAsync();
        }
    }
}
