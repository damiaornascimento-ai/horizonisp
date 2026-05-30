using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using horizonisp.Context;
using horizonisp.Models;

namespace horizonisp.Pages.Assinaturas
{
    public class EditModel(AppDbContext db) : PageModel
    {
        [BindProperty]
        public Assinatura Assinatura { get; set; } = new();

        public SelectList Clientes { get; private set; } = null!;
        public SelectList Planos { get; private set; } = null!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            var assinatura = await db.Assinaturas.FirstOrDefaultAsync(a => a.Id == id);
            if (assinatura is null)
            {
                return NotFound();
            }

            Assinatura = assinatura;
            await CarregarListasAsync();
            ViewData["Clientes"] = Clientes;
            ViewData["Planos"] = Planos;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await CarregarListasAsync();
                ViewData["Clientes"] = Clientes;
                ViewData["Planos"] = Planos;
                return Page();
            }

            db.Attach(Assinatura).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await db.Assinaturas.AnyAsync(a => a.Id == Assinatura.Id))
                {
                    return NotFound();
                }

                throw;
            }

            return RedirectToPage("Index");
        }

        private async Task CarregarListasAsync()
        {
            var clientes = await db.Clientes.OrderBy(c => c.Nome).ToListAsync();
            var planos = await db.Planos.OrderBy(p => p.Nome).ToListAsync();

            Clientes = new SelectList(clientes, nameof(Cliente.Id), nameof(Cliente.Nome), Assinatura.ClienteId);
            Planos = new SelectList(planos, nameof(Plano.Id), nameof(Plano.Nome), Assinatura.PlanoId);
        }
    }
}
