using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using horizonisp.Helpers;
using horizonisp.Models;
using horizonisp.Services;

namespace horizonisp.Pages.Portal.Faturas
{
    public class DetalheModel(IPortalService portalService, IFaturamentoService faturamentoService) : PageModel
    {
        public Fatura Fatura { get; private set; } = null!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            var clienteId = User.ObterClienteId();
            if (clienteId is null)
            {
                return RedirectToPage("/Login");
            }

            var fatura = await portalService.ObterFaturaAsync(clienteId.Value, id.Value);
            if (fatura is null)
            {
                return NotFound();
            }

            await faturamentoService.GarantirPixAsync(fatura);
            Fatura = fatura;
            return Page();
        }
    }
}
