using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using horizonisp.Context;
using horizonisp.Models;
using horizonisp.Models.Enums;
using horizonisp.Services;

namespace horizonisp.Pages.OrdensServico
{
    public class CreateModel(AppDbContext db, IOrdemServicoService ordemServicoService) : PageModel
    {
        [BindProperty]
        public OrdemServico Ordem { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int? chamadoId)
        {
            await CarregarListasAsync();
            Ordem.Status = StatusOrdemServico.Aberta;
            Ordem.Tipo = TipoOrdemServico.Manutencao;

            if (!chamadoId.HasValue)
            {
                return Page();
            }

            var chamado = await db.Chamados
                .Include(c => c.Cliente)
                .Include(c => c.Mensagens)
                .FirstOrDefaultAsync(c => c.Id == chamadoId.Value);

            if (chamado is null)
            {
                return Page();
            }

            Ordem.ClienteId = chamado.ClienteId;
            Ordem.ChamadoId = chamado.Id;
            Ordem.Titulo = chamado.Assunto;
            Ordem.Descricao = chamado.Mensagens.OrderBy(m => m.DataEnvio).FirstOrDefault()?.Conteudo ?? chamado.Assunto;
            Ordem.Endereco = chamado.Cliente.Endereco;
            Ordem.Tipo = chamado.Categoria == CategoriaChamado.Tecnico
                ? TipoOrdemServico.Manutencao
                : TipoOrdemServico.Vistoria;

            var assinatura = await db.Assinaturas
                .Where(a => a.ClienteId == chamado.ClienteId && a.Status == StatusAssinatura.Ativa)
                .OrderByDescending(a => a.DataInicio)
                .FirstOrDefaultAsync();

            if (assinatura is not null)
            {
                Ordem.AssinaturaId = assinatura.Id;
                if (!string.IsNullOrWhiteSpace(assinatura.EnderecoInstalacao))
                {
                    Ordem.Endereco = assinatura.EnderecoInstalacao;
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await CarregarListasAsync();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            await ordemServicoService.CriarAsync(Ordem);
            return RedirectToPage("Detalhe", new { id = Ordem.Id });
        }

        private async Task CarregarListasAsync()
        {
            ViewData["Clientes"] = new SelectList(
                await db.Clientes.OrderBy(c => c.Nome).Select(c => new { c.Id, c.Nome }).ToListAsync(),
                "Id", "Nome", Ordem.ClienteId);

            ViewData["Assinaturas"] = new SelectList(
                await db.Assinaturas
                    .Include(a => a.Cliente)
                    .OrderBy(a => a.Cliente.Nome)
                    .Select(a => new { a.Id, Nome = $"{a.Cliente.Nome} — {a.LoginPppoe}" })
                    .ToListAsync(),
                "Id", "Nome", Ordem.AssinaturaId);
        }
    }
}
