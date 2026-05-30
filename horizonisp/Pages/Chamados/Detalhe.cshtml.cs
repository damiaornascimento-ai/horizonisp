using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using horizonisp.Helpers;
using horizonisp.Models;
using horizonisp.Models.Enums;
using horizonisp.Services;

namespace horizonisp.Pages.Chamados
{
    public class DetalheModel(IChamadoService chamadoService) : PageModel
    {
        public Chamado Chamado { get; private set; } = null!;

        [BindProperty]
        public string NovaMensagem { get; set; } = string.Empty;

        [BindProperty]
        public StatusChamado NovoStatus { get; set; }

        public string? Erro { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            var chamado = await chamadoService.ObterAsync(id.Value);
            if (chamado is null)
            {
                return NotFound();
            }

            Chamado = chamado;
            NovoStatus = chamado.Status;
            return Page();
        }

        public async Task<IActionResult> OnPostResponderAsync(int id)
        {
            if (string.IsNullOrWhiteSpace(NovaMensagem))
            {
                Erro = "Digite uma resposta.";
                return await OnGetAsync(id);
            }

            try
            {
                await chamadoService.ResponderAsync(
                    id,
                    NovaMensagem,
                    AutorMensagemChamado.Operador,
                    User.Identity?.Name ?? "Operador");
            }
            catch (InvalidOperationException ex)
            {
                Erro = ex.Message;
                return await OnGetAsync(id);
            }

            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostAtualizarStatusAsync(int id)
        {
            await chamadoService.AtualizarStatusAsync(id, NovoStatus);
            return RedirectToPage(new { id });
        }
    }
}
