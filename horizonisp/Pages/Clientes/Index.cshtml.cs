using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using horizonisp.Context;
using horizonisp.Helpers;
using horizonisp.Models;
using horizonisp.Models.Enums;

namespace horizonisp.Pages.Clientes
{
    public class ClienteListagemItem
    {
        public Cliente Cliente { get; init; } = null!;
        public CategoriaConexaoCliente CategoriaConexao { get; init; }
    }

    public class IndexModel(AppDbContext db) : PageModel
    {
        public IReadOnlyList<ClienteListagemItem> Clientes { get; private set; } = [];

        [BindProperty(SupportsGet = true)]
        public string? Busca { get; set; }

        [BindProperty(SupportsGet = true)]
        public CategoriaConexaoCliente? Categoria { get; set; }

        public int TotalOnline { get; private set; }
        public int TotalOffline { get; private set; }
        public int TotalBloqueados { get; private set; }

        public async Task OnGetAsync()
        {
            var query = db.Clientes
                .Include(c => c.Assinaturas)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(Busca))
            {
                var termo = $"%{Busca.Trim()}%";
                query = query.Where(c =>
                    EF.Functions.Like(c.Nome, termo)
                    || EF.Functions.Like(c.Documento, termo)
                    || EF.Functions.Like(c.Email, termo)
                    || EF.Functions.Like(c.Telefone, termo)
                    || EF.Functions.Like(c.Cidade, termo));
            }

            var clientes = await query
                .OrderBy(c => c.Nome)
                .ToListAsync();

            var assinaturaIds = clientes
                .SelectMany(c => c.Assinaturas.Select(a => a.Id))
                .ToHashSet();

            var onusPorAssinatura = await db.Onus
                .Where(o => o.AssinaturaId.HasValue && assinaturaIds.Contains(o.AssinaturaId.Value))
                .GroupBy(o => o.AssinaturaId!.Value)
                .ToDictionaryAsync(g => g.Key, g => g.ToList());

            var itens = clientes
                .Select(c => new ClienteListagemItem
                {
                    Cliente = c,
                    CategoriaConexao = ClienteConexaoClassifier.Classificar(c, onusPorAssinatura)
                })
                .ToList();

            TotalOnline = itens.Count(i => i.CategoriaConexao == CategoriaConexaoCliente.Online);
            TotalOffline = itens.Count(i => i.CategoriaConexao == CategoriaConexaoCliente.Offline);
            TotalBloqueados = itens.Count(i => i.CategoriaConexao == CategoriaConexaoCliente.Bloqueado);

            Clientes = Categoria.HasValue
                ? itens.Where(i => i.CategoriaConexao == Categoria.Value).ToList()
                : itens;
        }

    }
}
