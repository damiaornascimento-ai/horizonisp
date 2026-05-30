using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using horizonisp.Context;
using horizonisp.Models;

namespace horizonisp.Pages.Clientes
{
    public class EditModel(AppDbContext db, PasswordHasher<Cliente> passwordHasher) : PageModel
    {
        [BindProperty]
        public Cliente Cliente { get; set; } = new();

        [BindProperty]
        public string? SenhaPortal { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            var cliente = await db.Clientes.FirstOrDefaultAsync(c => c.Id == id);
            if (cliente is null)
            {
                return NotFound();
            }

            Cliente = cliente;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var existente = await db.Clientes.FirstOrDefaultAsync(c => c.Id == Cliente.Id);
            if (existente is null)
            {
                return NotFound();
            }

            existente.Nome = Cliente.Nome;
            existente.Documento = Cliente.Documento;
            existente.Email = Cliente.Email;
            existente.Telefone = Cliente.Telefone;
            existente.Endereco = Cliente.Endereco;
            existente.Cidade = Cliente.Cidade;
            existente.Estado = Cliente.Estado;
            existente.Cep = Cliente.Cep;
            existente.Status = Cliente.Status;
            existente.PortalAtivo = Cliente.PortalAtivo;

            if (!string.IsNullOrWhiteSpace(SenhaPortal))
            {
                existente.SenhaPortalHash = passwordHasher.HashPassword(existente, SenhaPortal);
            }

            await db.SaveChangesAsync();
            return RedirectToPage("Index");
        }
    }
}
