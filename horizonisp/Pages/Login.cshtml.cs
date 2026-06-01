using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using horizonisp.Auth;
using horizonisp.Services;

namespace horizonisp.Pages
{
    [AllowAnonymous]
    public class LoginModel(IAuthService authService, IClienteAuthService clienteAuthService) : PageModel
    {
        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? Erro { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Informe o e-mail ou CPF/CNPJ.")]
            [Display(Name = "E-mail ou documento")]
            public string Identificador { get; set; } = string.Empty;

            [Required(ErrorMessage = "Informe a senha.")]
            [DataType(DataType.Password)]
            [Display(Name = "Senha")]
            public string Senha { get; set; } = string.Empty;
        }

        public async Task<IActionResult> OnGet()
        {
            if ((await HttpContext.AuthenticateAsync(AuthSchemes.Admin)).Succeeded)
            {
                return RedirectToPage("/Index");
            }

            if ((await HttpContext.AuthenticateAsync(AuthSchemes.Cliente)).Succeeded)
            {
                return RedirectToPage("/Portal/Index");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var usuario = await authService.ValidarLoginAsync(Input.Identificador, Input.Senha);
            if (usuario is not null)
            {
                await authService.EntrarAsync(usuario);
                return RedirectToPage("/Index");
            }

            var cliente = await clienteAuthService.ValidarLoginAsync(Input.Identificador, Input.Senha);
            if (cliente is not null)
            {
                await clienteAuthService.EntrarAsync(cliente);
                return RedirectToPage("/Portal/Index");
            }

            Erro = "Credenciais inválidas ou acesso não disponível para este usuário.";
            return Page();
        }
    }
}
