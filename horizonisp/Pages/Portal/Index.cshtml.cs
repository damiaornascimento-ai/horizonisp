using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using horizonisp.Helpers;
using horizonisp.Services;

namespace horizonisp.Pages.Portal
{
    public class IndexModel(IPortalService portalService) : PageModel
    {
        public PortalResumo Resumo { get; private set; } = null!;

        public async Task<IActionResult> OnGetAsync()
        {
            var clienteId = User.ObterClienteId();
            if (clienteId is null)
            {
                return RedirectToPage("/Portal/Login");
            }

            var resumo = await portalService.ObterResumoAsync(clienteId.Value);
            if (resumo is null)
            {
                return RedirectToPage("/Portal/Login");
            }

            Resumo = resumo;
            return Page();
        }
    }
}
