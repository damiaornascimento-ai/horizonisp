using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using horizonisp.Services;

namespace horizonisp.Pages
{
    [AllowAnonymous]
    public class LoginModel(IAuthService authService) : PageModel
    {
        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? Erro { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Informe o e-mail.")]
            [EmailAddress(ErrorMessage = "E-mail inválido.")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Informe a senha.")]
            [DataType(DataType.Password)]
            public string Senha { get; set; } = string.Empty;
        }

        public IActionResult OnGet()
        {
            if (User.Identity?.IsAuthenticated ?? false)
            {
                return RedirectToPage("/Index");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var usuario = await authService.ValidarLoginAsync(Input.Email, Input.Senha);
            if (usuario is null)
            {
                Erro = "E-mail ou senha inválidos.";
                return Page();
            }

            await authService.EntrarAsync(usuario);
            return RedirectToPage("/Index");
        }

        public async Task<IActionResult> OnPostLogoutAsync()
        {
            await authService.SairAsync();
            return RedirectToPage("/Login");
        }
    }
}
