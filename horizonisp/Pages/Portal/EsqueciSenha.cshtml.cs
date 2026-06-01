using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using horizonisp.Services;

namespace horizonisp.Pages.Portal
{
    [AllowAnonymous]
    public class EsqueciSenhaModel(IRecuperacaoSenhaPortalService recuperacaoSenhaService) : PageModel
    {
        [BindProperty]
        public InputModel Input { get; set; } = new();

        public bool Enviado { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Informe o e-mail ou CPF/CNPJ.")]
            [Display(Name = "E-mail ou documento")]
            public string Identificador { get; set; } = string.Empty;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var urlBase = $"{Request.Scheme}://{Request.Host}";
            await recuperacaoSenhaService.SolicitarAsync(Input.Identificador, urlBase);

            Enviado = true;
            return Page();
        }
    }
}
