using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using horizonisp.Context;
using horizonisp.Models;
using horizonisp.Models.Enums;
using horizonisp.Services;

namespace horizonisp.Pages.Assinaturas
{
    public class CreateModel(
        AppDbContext db,
        IFaturamentoService faturamentoService,
        IMikrotikService mikrotikService) : PageModel
    {
        [BindProperty]
        public Assinatura Assinatura { get; set; } = new();

        public SelectList Clientes { get; private set; } = null!;
        public SelectList Planos { get; private set; } = null!;

        public async Task OnGetAsync()
        {
            await CarregarListasAsync();
            Assinatura.DataInicio = DateTime.UtcNow;
            Assinatura.Status = StatusAssinatura.Ativa;
            ViewData["Clientes"] = Clientes;
            ViewData["Planos"] = Planos;
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

            var plano = await db.Planos.FindAsync(Assinatura.PlanoId);
            if (plano is null)
            {
                ModelState.AddModelError(string.Empty, "Plano inválido.");
                await CarregarListasAsync();
                ViewData["Clientes"] = Clientes;
                ViewData["Planos"] = Planos;
                return Page();
            }

            db.Assinaturas.Add(Assinatura);
            await db.SaveChangesAsync();

            await faturamentoService.GerarFaturaAsync(Assinatura, plano);
            await mikrotikService.ProvisionarAssinaturaAsync(Assinatura, plano);

            return RedirectToPage("Index");
        }

        private async Task CarregarListasAsync()
        {
            var clientes = await db.Clientes.OrderBy(c => c.Nome).ToListAsync();
            var planos = await db.Planos.Where(p => p.Ativo).OrderBy(p => p.Nome).ToListAsync();

            Clientes = new SelectList(clientes, nameof(Cliente.Id), nameof(Cliente.Nome));
            Planos = new SelectList(planos, nameof(Plano.Id), nameof(Plano.Nome));
        }
    }
}
