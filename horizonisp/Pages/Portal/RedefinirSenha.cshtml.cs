using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using horizonisp.Services;

namespace horizonisp.Pages.Portal
{
    [AllowAnonymous]
    public class RedefinirSenhaModel(IRecuperacaoSenhaPortalService recuperacaoSenhaService) : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string? Token { get; set; }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public bool TokenValido { get; set; }
        public bool Concluido { get; set; }
        public string? Erro { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Informe a nova senha.")]
            [StringLength(100, MinimumLength = 6, ErrorMessage = "Use entre 6 e 100 caracteres.")]
            [DataType(DataType.Password)]
            [Display(Name = "Nova senha")]
            public string NovaSenha { get; set; } = string.Empty;

            [Required(ErrorMessage = "Confirme a nova senha.")]
            [Compare(nameof(NovaSenha), ErrorMessage = "A confirmação não confere.")]
            [DataType(DataType.Password)]
            [Display(Name = "Confirmar nova senha")]
            public string Confirmacao { get; set; } = string.Empty;
        }

        public IActionResult OnGet()
        {
            TokenValido = !string.IsNullOrWhiteSpace(Token);
            if (!TokenValido)
            {
                Erro = "Link inválido ou incompleto. Solicite uma nova recuperação de senha.";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                TokenValido = true;
                return Page();
            }

            var resultado = await recuperacaoSenhaService.RedefinirAsync(Token ?? string.Empty, Input.NovaSenha);
            switch (resultado)
            {
                case ResultadoRedefinicaoSenhaPortal.Sucesso:
                    Concluido = true;
                    TokenValido = false;
                    return Page();

                case ResultadoRedefinicaoSenhaPortal.TokenExpirado:
                    Erro = "Este link expirou. Solicite uma nova recuperação de senha.";
                    TokenValido = false;
                    return Page();

                default:
                    Erro = "Link inválido. Solicite uma nova recuperação de senha.";
                    TokenValido = false;
                    return Page();
            }
        }
    }
}
