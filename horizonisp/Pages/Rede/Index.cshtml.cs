using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using horizonisp.Models;
using horizonisp.Services;

namespace horizonisp.Pages.Rede
{
    public class IndexModel(
        IRedeService redeService,
        IRedeSincronizacaoService sincronizacaoService) : PageModel
    {
        public IReadOnlyList<Models.Olt> Olts { get; private set; } = [];
        public IReadOnlyList<Models.Onu> Onus { get; private set; } = [];
        public int OnusOffline { get; private set; }
        public string? MensagemSync { get; private set; }

        public async Task OnGetAsync()
        {
            await CarregarAsync();
        }

        public async Task<IActionResult> OnPostSincronizarAsync()
        {
            var resultado = await sincronizacaoService.SincronizarTodasAsync();
            MensagemSync = string.Join(" ", resultado.Mensagens);
            if (resultado.Erros > 0)
            {
                TempData["SyncErro"] = MensagemSync;
            }
            else
            {
                TempData["SyncOk"] = $"Sincronização concluída: {resultado.OnusAtualizadas} ONU(s) atualizada(s).";
            }

            return RedirectToPage();
        }

        private async Task CarregarAsync()
        {
            Olts = await redeService.ListarOltsAsync();
            Onus = await redeService.ListarOnusAsync();
            OnusOffline = await redeService.ContarOnusOfflineAsync();
        }
    }
}
