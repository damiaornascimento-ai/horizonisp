using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using horizonisp.Services;

namespace horizonisp.Pages.Rede.Olt
{
    public class EditModel(IRedeService redeService) : PageModel
    {
        [BindProperty]
        public Models.Olt Olt { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id is null) return NotFound();
            var olt = await redeService.ObterOltAsync(id.Value);
            if (olt is null) return NotFound();
            Olt = olt;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();
            await redeService.SalvarOltAsync(Olt);
            return RedirectToPage("/Rede/Index");
        }
    }
}
