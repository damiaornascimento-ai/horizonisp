using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using horizonisp.Context;
using horizonisp.Models;
using horizonisp.Models.Enums;
using horizonisp.Services;

namespace horizonisp.Pages.OrdensServico
{
    public class IndexModel(IOrdemServicoService ordemServicoService) : PageModel
    {
        public IReadOnlyList<OrdemServico> Ordens { get; private set; } = [];
        public StatusOrdemServico? FiltroStatus { get; private set; }

        public async Task OnGetAsync(StatusOrdemServico? status)
        {
            FiltroStatus = status;
            Ordens = await ordemServicoService.ListarAsync(status);
        }
    }
}
