using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using horizonisp.Context;
using horizonisp.Models;

namespace horizonisp.Pages.Assinaturas
{
    public class IndexModel(AppDbContext db) : PageModel
    {
        public IList<Assinatura> Assinaturas { get; private set; } = [];

        public async Task OnGetAsync()
        {
            Assinaturas = await db.Assinaturas
                .Include(a => a.Cliente)
                .Include(a => a.Plano)
                .OrderByDescending(a => a.DataInicio)
                .ToListAsync();
        }
    }
}
