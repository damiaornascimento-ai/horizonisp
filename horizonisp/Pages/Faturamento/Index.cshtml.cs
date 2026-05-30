using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using horizonisp.Configuration;
using horizonisp.Services;

namespace horizonisp.Pages.Faturamento
{
    public class IndexModel(
        IFaturamentoService faturamentoService,
        IOptions<HorizonIspOptions> options) : PageModel
    {
        public HorizonIspOptions Configuracao => options.Value;
        public FaturamentoResult? UltimoResultado { get; private set; }
        public string? Mensagem { get; private set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostExecutarAsync()
        {
            UltimoResultado = await faturamentoService.ExecutarRotinaDiariaAsync();
            Mensagem = "Rotina executada com sucesso.";
            return Page();
        }
    }
}
