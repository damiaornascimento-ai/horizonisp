using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using horizonisp.Helpers;
using horizonisp.Models;
using horizonisp.Models.Enums;
using horizonisp.Services;

namespace horizonisp.Pages.Portal.Chamados
{
    public class DetalheModel(IChamadoService chamadoService) : PageModel
    {
        public Chamado Chamado { get; private set; } = null!;

        [BindProperty]
        public string NovaMensagem { get; set; } = string.Empty;

        public string? Erro { get; set; }

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

            var chamado = await chamadoService.ObterAsync(id.Value, clienteId.Value);
            if (chamado is null)
            {
                return NotFound();
            }

            Chamado = chamado;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var clienteId = User.ObterClienteId();
            if (clienteId is null)
            {
                return RedirectToPage("/Login");
            }

            if (string.IsNullOrWhiteSpace(NovaMensagem))
            {
                Erro = "Digite uma mensagem.";
                return await OnGetAsync(id);
            }

            try
            {
                await chamadoService.ResponderAsync(
                    id,
                    NovaMensagem,
                    AutorMensagemChamado.Cliente,
                    User.Identity?.Name ?? "Cliente",
                    clienteId.Value);
            }
            catch (InvalidOperationException ex)
            {
                Erro = ex.Message;
                return await OnGetAsync(id);
            }

            return RedirectToPage(new { id });
        }
    }
}
