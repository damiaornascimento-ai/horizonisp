using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using horizonisp.Context;
using horizonisp.Models;

namespace horizonisp.Pages.Usuarios
{
    public class IndexModel(AppDbContext db) : PageModel
    {
        public IList<Usuario> Usuarios { get; private set; } = [];

        public async Task OnGetAsync()
        {
            Usuarios = await db.Usuarios
                .OrderBy(u => u.Nome)
                .ToListAsync();
        }
    }
}
