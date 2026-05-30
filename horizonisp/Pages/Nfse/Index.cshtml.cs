using Microsoft.AspNetCore.Mvc.RazorPages;
using horizonisp.Models;
using horizonisp.Models.Enums;
using horizonisp.Services;

namespace horizonisp.Pages.Nfse
{
    public class IndexModel(INfseService nfseService) : PageModel
    {
        public IReadOnlyList<NotaFiscalServico> Notas { get; private set; } = [];
        public StatusNfse? FiltroStatus { get; private set; }

        public async Task OnGetAsync(StatusNfse? status)
        {
            FiltroStatus = status;
            Notas = await nfseService.ListarAsync(status);
        }
    }
}
