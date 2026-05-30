using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using horizonisp.Context;
using horizonisp.Models;

namespace horizonisp.Pages.Clientes
{
    public class IndexModel(AppDbContext db) : PageModel
    {
        public IList<Cliente> Clientes { get; private set; } = [];

        public async Task OnGetAsync()
        {
            Clientes = await db.Clientes
                .OrderBy(c => c.Nome)
                .ToListAsync();
        }
    }
}
