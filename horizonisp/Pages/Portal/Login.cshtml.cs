using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace horizonisp.Pages.Portal
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        public IActionResult OnGet()
        {
            return RedirectToPage("/Login");
        }

        public IActionResult OnPost()
        {
            return RedirectToPage("/Login");
        }
    }
}
