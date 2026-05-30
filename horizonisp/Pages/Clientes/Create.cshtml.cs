using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using horizonisp.Context;
using horizonisp.Helpers;
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
            if (!DocumentoValidator.EhValido(Cliente.Documento))
            {
                ModelState.AddModelError($"{nameof(Cliente)}.{nameof(Cliente.Documento)}", "CPF ou CNPJ inválido.");
            }

            if (await db.Clientes.AnyAsync(c => c.Documento == DocumentoValidator.Formatar(Cliente.Documento)))
            {
                ModelState.AddModelError($"{nameof(Cliente)}.{nameof(Cliente.Documento)}", "Documento já cadastrado.");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            Cliente.Documento = DocumentoValidator.Formatar(Cliente.Documento);
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
