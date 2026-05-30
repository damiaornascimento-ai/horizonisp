using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using horizonisp.Context;
using horizonisp.Models;

namespace horizonisp.Pages.Planos
{
    public class EditModel(AppDbContext db) : PageModel
    {
        [BindProperty]
        public Plano Plano { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            var plano = await db.Planos.FirstOrDefaultAsync(p => p.Id == id);
            if (plano is null)
            {
                return NotFound();
            }

            Plano = plano;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            db.Attach(Plano).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await db.Planos.AnyAsync(p => p.Id == Plano.Id))
                {
                    return NotFound();
                }

                throw;
            }

            return RedirectToPage("Index");
        }
    }
}
