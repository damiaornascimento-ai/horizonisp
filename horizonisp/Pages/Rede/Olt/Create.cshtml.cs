using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using horizonisp.Models;
using horizonisp.Services;

namespace horizonisp.Pages.Rede.Olt
{
    public class CreateModel(IRedeService redeService) : PageModel
    {
        [BindProperty]
        public Models.Olt Olt { get; set; } = new();

        public void OnGet() => Olt.Ativo = true;

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();
            await redeService.SalvarOltAsync(Olt);
            return RedirectToPage("/Rede/Index");
        }
    }
}
