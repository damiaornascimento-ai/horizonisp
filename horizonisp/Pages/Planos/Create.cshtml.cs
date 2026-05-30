using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using horizonisp.Context;
using horizonisp.Models;

namespace horizonisp.Pages.Planos
{
    public class CreateModel(AppDbContext db) : PageModel
    {
        [BindProperty]
        public Plano Plano { get; set; } = new();

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            db.Planos.Add(Plano);
            await db.SaveChangesAsync();
            return RedirectToPage("Index");
        }
    }
}
