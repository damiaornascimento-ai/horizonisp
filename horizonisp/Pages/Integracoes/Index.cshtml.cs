using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using horizonisp.Configuration;

namespace horizonisp.Pages.Integracoes
{
    public class IndexModel(IOptions<HorizonIspOptions> options) : PageModel
    {
        public HorizonIspOptions Configuracao => options.Value;
    }
}
