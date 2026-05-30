using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using horizonisp.Helpers;
using horizonisp.Models;
using horizonisp.Services;

namespace horizonisp.Pages.Portal.Faturas
{
    public class IndexModel(IPortalService portalService) : PageModel
    {
        public IReadOnlyList<Fatura> Faturas { get; private set; } = [];

        public async Task<IActionResult> OnGetAsync()
        {
            var clienteId = User.ObterClienteId();
            if (clienteId is null)
            {
                return RedirectToPage("/Portal/Login");
            }

            Faturas = await portalService.ObterFaturasAsync(clienteId.Value);
            return Page();
        }
    }
}
