using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using horizonisp.Helpers;
using horizonisp.Models;
using horizonisp.Services;

namespace horizonisp.Pages.Portal.Chamados
{
    public class IndexModel(IChamadoService chamadoService) : PageModel
    {
        public IReadOnlyList<Chamado> Chamados { get; private set; } = [];

        public async Task<IActionResult> OnGetAsync()
        {
            var clienteId = User.ObterClienteId();
            if (clienteId is null)
            {
                return RedirectToPage("/Login");
            }

            Chamados = await chamadoService.ListarPorClienteAsync(clienteId.Value);
            return Page();
        }
    }
}
