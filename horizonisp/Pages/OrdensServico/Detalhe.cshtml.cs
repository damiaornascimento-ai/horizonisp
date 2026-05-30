using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using horizonisp.Models;
using horizonisp.Models.Enums;
using horizonisp.Services;

namespace horizonisp.Pages.OrdensServico
{
    public class DetalheModel(IOrdemServicoService ordemServicoService) : PageModel
    {
        public OrdemServico Ordem { get; private set; } = null!;

        [BindProperty]
        public StatusOrdemServico NovoStatus { get; set; }

        [BindProperty]
        public string? TecnicoResponsavel { get; set; }

        [BindProperty]
        public DateTime? DataAgendada { get; set; }

        [BindProperty]
        public string? ObservacaoConclusao { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            var ordem = await ordemServicoService.ObterAsync(id.Value);
            if (ordem is null)
            {
                return NotFound();
            }

            Ordem = ordem;
            NovoStatus = ordem.Status;
            TecnicoResponsavel = ordem.TecnicoResponsavel;
            DataAgendada = ordem.DataAgendada?.ToLocalTime();
            return Page();
        }

        public async Task<IActionResult> OnPostAtualizarAsync(int id)
        {
            var ordem = await ordemServicoService.ObterAsync(id);
            if (ordem is null)
            {
                return NotFound();
            }

            ordem.Status = NovoStatus;
            ordem.TecnicoResponsavel = TecnicoResponsavel;
            ordem.DataAgendada = DataAgendada?.ToUniversalTime();

            if (NovoStatus == StatusOrdemServico.Concluida && !string.IsNullOrWhiteSpace(ObservacaoConclusao))
            {
                ordem.ObservacaoConclusao = ObservacaoConclusao.Trim();
                ordem.DataConclusao = DateTime.UtcNow;
            }

            await ordemServicoService.SalvarAsync(ordem);
            return RedirectToPage(new { id });
        }
    }
}
