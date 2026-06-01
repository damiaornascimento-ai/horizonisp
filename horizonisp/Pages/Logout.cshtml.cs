using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using horizonisp.Services;

namespace horizonisp.Pages
{
    [AllowAnonymous]
    public class LogoutModel(IAuthService authService, IClienteAuthService clienteAuthService) : PageModel
    {
        public async Task<IActionResult> OnPostAsync()
        {
            await authService.SairAsync();
            await clienteAuthService.SairAsync();
            return Redirect("/Login");
        }

        public async Task<IActionResult> OnGetAsync()
        {
            await authService.SairAsync();
            await clienteAuthService.SairAsync();
            return Redirect("/Login");
        }
    }
}
