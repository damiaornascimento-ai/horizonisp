using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using horizonisp.Context;
using horizonisp.Models;

namespace horizonisp.Pages.Clientes
{
    public class LocalizacaoModel(AppDbContext db) : PageModel
    {
        public Cliente Cliente { get; private set; } = new();

        [BindProperty]
        public double? Latitude { get; set; }

        [BindProperty]
        public double? Longitude { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            var cliente = await CarregarClienteAsync(id);
            if (cliente is null)
            {
                return NotFound();
            }

            Cliente = cliente;
            Latitude = cliente.Latitude;
            Longitude = cliente.Longitude;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var cliente = await CarregarClienteAsync(id);
            if (cliente is null)
            {
                return NotFound();
            }

            Cliente = cliente;

            if (!Latitude.HasValue || !Longitude.HasValue)
            {
                ModelState.AddModelError(string.Empty, "Marque a posição no mapa antes de salvar.");
                return Page();
            }

            if (Latitude is < -90 or > 90 || Longitude is < -180 or > 180)
            {
                ModelState.AddModelError(string.Empty, "Coordenadas inválidas.");
                return Page();
            }

            cliente.Latitude = Latitude;
            cliente.Longitude = Longitude;
            cliente.LocalizacaoInstalacaoEm = DateTime.UtcNow;

            await db.SaveChangesAsync();
            TempData["Sucesso"] = "Localização da instalação salva com sucesso.";
            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostRemoverAsync(int id)
        {
            var cliente = await CarregarClienteAsync(id);
            if (cliente is null)
            {
                return NotFound();
            }

            cliente.Latitude = null;
            cliente.Longitude = null;
            cliente.LocalizacaoInstalacaoEm = null;

            await db.SaveChangesAsync();
            TempData["Sucesso"] = "Localização removida.";
            return RedirectToPage(new { id });
        }

        private async Task<Cliente?> CarregarClienteAsync(int? id)
        {
            if (id is null)
            {
                return null;
            }

            return await db.Clientes.FirstOrDefaultAsync(c => c.Id == id);
        }
    }
}
