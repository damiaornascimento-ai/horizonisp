using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using horizonisp.Context;
using horizonisp.Models;
using horizonisp.Models.Enums;

namespace horizonisp.Pages.Clientes
{
    public class LocalizacaoModel(AppDbContext db) : PageModel
    {
        public Cliente Cliente { get; private set; } = new();
        public string? PlanoAtivo { get; private set; }
        public StatusAssinatura? StatusAssinaturaAtiva { get; private set; }

        [BindProperty(SupportsGet = true)]
        public double? Latitude { get; set; }

        [BindProperty(SupportsGet = true)]
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

            var assinaturaAtiva = cliente.Assinaturas
                .Where(a => a.Status == StatusAssinatura.Ativa)
                .OrderByDescending(a => a.DataInicio)
                .FirstOrDefault();

            if (assinaturaAtiva is not null)
            {
                PlanoAtivo = assinaturaAtiva.Plano?.Nome;
                StatusAssinaturaAtiva = assinaturaAtiva.Status;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            ModelState.Remove(nameof(Latitude));
            ModelState.Remove(nameof(Longitude));

            var cliente = await CarregarClienteAsync(id);
            if (cliente is null)
            {
                return NotFound();
            }

            Cliente = cliente;

            if (!TentarLerCoordenadas(out var latitude, out var longitude, out var erro))
            {
                ModelState.AddModelError(string.Empty, erro);
                return Page();
            }

            Latitude = latitude;
            Longitude = longitude;

            cliente.Latitude = latitude;
            cliente.Longitude = longitude;
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

        private bool TentarLerCoordenadas(out double latitude, out double longitude, out string erro)
        {
            latitude = 0;
            longitude = 0;

            var latTexto = Request.Form["Latitude"].ToString();
            var lngTexto = Request.Form["Longitude"].ToString();

            if (string.IsNullOrWhiteSpace(latTexto) || string.IsNullOrWhiteSpace(lngTexto))
            {
                erro = "Marque a posição no mapa antes de salvar.";
                return false;
            }

            if (!TryParseCoordenada(latTexto, out latitude) || !TryParseCoordenada(lngTexto, out longitude))
            {
                erro = "Coordenadas inválidas. Marque novamente no mapa.";
                return false;
            }

            if (latitude is < -90 or > 90 || longitude is < -180 or > 180)
            {
                erro = "Coordenadas fora do intervalo válido.";
                return false;
            }

            erro = string.Empty;
            return true;
        }

        private static bool TryParseCoordenada(string valor, out double resultado)
        {
            if (double.TryParse(valor, NumberStyles.Float, CultureInfo.InvariantCulture, out resultado))
            {
                return true;
            }

            return double.TryParse(valor, NumberStyles.Float, CultureInfo.CurrentCulture, out resultado);
        }

        private async Task<Cliente?> CarregarClienteAsync(int? id)
        {
            if (id is null)
            {
                return null;
            }

            return await db.Clientes
                .Include(c => c.Assinaturas)
                .ThenInclude(a => a.Plano)
                .FirstOrDefaultAsync(c => c.Id == id);
        }
    }
}
