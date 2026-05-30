using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using horizonisp.Context;
using horizonisp.Models;

namespace horizonisp.Pages.Clientes
{
    public class CreateModel(AppDbContext db, PasswordHasher<Cliente> passwordHasher) : PageModel
    {
        [BindProperty]
        public Cliente Cliente { get; set; } = new();

        [BindProperty]
        public string? SenhaPortal { get; set; }

        public void OnGet()
        {
            Cliente.PortalAtivo = true;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            Cliente.DataCadastro = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(SenhaPortal))
            {
                Cliente.SenhaPortalHash = passwordHasher.HashPassword(Cliente, SenhaPortal);
            }

            db.Clientes.Add(Cliente);
            await db.SaveChangesAsync();
            return RedirectToPage("Index");
        }
    }
}
