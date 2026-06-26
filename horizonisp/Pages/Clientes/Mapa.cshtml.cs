using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using horizonisp.Context;
using horizonisp.Models;

namespace horizonisp.Pages.Clientes
{
    public record ClienteMapaPonto(int Id, string Nome, string Endereco, string Cidade, double Latitude, double Longitude);

    public class MapaModel(AppDbContext db) : PageModel
    {
        public IList<ClienteMapaPonto> Clientes { get; private set; } = [];
        public int TotalComLocalizacao { get; private set; }
        public int TotalSemLocalizacao { get; private set; }
        public string ClientesJson { get; private set; } = "[]";

        public async Task OnGetAsync()
        {
            var clientes = await db.Clientes
                .AsNoTracking()
                .OrderBy(c => c.Nome)
                .Select(c => new
                {
                    c.Id,
                    c.Nome,
                    c.Endereco,
                    c.Cidade,
                    c.Latitude,
                    c.Longitude
                })
                .ToListAsync();

            TotalComLocalizacao = clientes.Count(c => c.Latitude.HasValue && c.Longitude.HasValue);
            TotalSemLocalizacao = clientes.Count - TotalComLocalizacao;

            Clientes = clientes
                .Where(c => c.Latitude.HasValue && c.Longitude.HasValue)
                .Select(c => new ClienteMapaPonto(
                    c.Id,
                    c.Nome,
                    c.Endereco,
                    c.Cidade,
                    c.Latitude!.Value,
                    c.Longitude!.Value))
                .ToList();

            ClientesJson = JsonSerializer.Serialize(Clientes);
        }
    }
}
