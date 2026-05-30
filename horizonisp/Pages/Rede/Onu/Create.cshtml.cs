using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using horizonisp.Context;
using horizonisp.Models;
using horizonisp.Services;

namespace horizonisp.Pages.Rede.Onu
{
    public class CreateModel(IRedeService redeService, AppDbContext db) : PageModel
    {
        [BindProperty]
        public Models.Onu Onu { get; set; } = new();

        public SelectList Olts { get; private set; } = null!;
        public SelectList Assinaturas { get; private set; } = null!;

        public async Task OnGetAsync()
        {
            await CarregarListasAsync();
            Onu.Status = Models.Enums.StatusOnu.Desconhecido;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await CarregarListasAsync();
                return Page();
            }

            await redeService.SalvarOnuAsync(Onu);
            return RedirectToPage("/Rede/Index");
        }

        private async Task CarregarListasAsync()
        {
            var olts = await redeService.ListarOltsAsync();
            var assinaturas = await db.Assinaturas.Include(a => a.Cliente).OrderBy(a => a.Cliente.Nome).ToListAsync();
            Olts = new SelectList(olts, nameof(Models.Olt.Id), nameof(Models.Olt.Nome), Onu.OltId);
            Assinaturas = new SelectList(
                assinaturas.Select(a => new { a.Id, Nome = $"{a.Cliente.Nome} ({a.LoginPppoe})" }),
                "Id",
                "Nome",
                Onu.AssinaturaId);
            ViewData["Olts"] = Olts;
            ViewData["Assinaturas"] = Assinaturas;
        }
    }
}
