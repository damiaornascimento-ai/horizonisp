using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using horizonisp.Helpers;
using horizonisp.Models.Enums;
using horizonisp.Services;

namespace horizonisp.Pages.Portal.Chamados
{
    public class CreateModel(IChamadoService chamadoService) : PageModel
    {
        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required(ErrorMessage = "Informe o assunto.")]
            [StringLength(150)]
            [Display(Name = "Assunto")]
            public string Assunto { get; set; } = string.Empty;

            [Display(Name = "Categoria")]
            public CategoriaChamado Categoria { get; set; } = CategoriaChamado.Tecnico;

            [Display(Name = "Prioridade")]
            public PrioridadeChamado Prioridade { get; set; } = PrioridadeChamado.Normal;

            [Required(ErrorMessage = "Descreva o problema.")]
            [StringLength(4000)]
            [Display(Name = "Mensagem")]
            public string Mensagem { get; set; } = string.Empty;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var clienteId = User.ObterClienteId();
            if (clienteId is null)
            {
                return RedirectToPage("/Login");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var chamado = await chamadoService.AbrirAsync(
                clienteId.Value,
                Input.Assunto,
                Input.Categoria,
                Input.Prioridade,
                Input.Mensagem,
                User.Identity?.Name ?? "Cliente");

            return RedirectToPage("Detalhe", new { id = chamado.Id });
        }
    }
}
