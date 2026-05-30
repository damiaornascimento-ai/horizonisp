using Microsoft.AspNetCore.Mvc.RazorPages;
using horizonisp.Models.Enums;
using horizonisp.Services;

namespace horizonisp.Pages.Chamados
{
    public class IndexModel(IChamadoService chamadoService) : PageModel
    {
        public IReadOnlyList<Models.Chamado> Chamados { get; private set; } = [];

        public StatusChamado? FiltroStatus { get; private set; }

        public async Task OnGetAsync(StatusChamado? status)
        {
            FiltroStatus = status;
            Chamados = await chamadoService.ListarTodosAsync(status);
        }
    }
}
