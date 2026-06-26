using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using horizonisp.Context;
using horizonisp.Helpers;
using horizonisp.Models.Enums;

namespace horizonisp.Pages.Clientes
{
    public record ClienteMapaPonto(
        int Id,
        string Nome,
        string Endereco,
        string Cidade,
        double Latitude,
        double Longitude,
        StatusCliente Status,
        int? SinalDbm,
        StatusOnu? StatusOnu,
        string? OnuSerial);

    public class MapaModel(AppDbContext db) : PageModel
    {
        private static readonly JsonSerializerOptions JsonOpcoes = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

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
                    c.Longitude,
                    c.Status
                })
                .ToListAsync();

            TotalComLocalizacao = clientes.Count(c => c.Latitude.HasValue && c.Longitude.HasValue);
            TotalSemLocalizacao = clientes.Count - TotalComLocalizacao;

            var comLocalizacao = clientes
                .Where(c => c.Latitude.HasValue && c.Longitude.HasValue)
                .ToList();

            var onusPorCliente = await ClienteOnuResumo.CarregarPorClienteAsync(
                db,
                comLocalizacao.Select(c => c.Id));

            Clientes = comLocalizacao
                .Select(c =>
                {
                    onusPorCliente.TryGetValue(c.Id, out var onu);
                    onu ??= new ClienteOnuResumo.Dados(null, null, null);

                    return new ClienteMapaPonto(
                        c.Id,
                        c.Nome,
                        c.Endereco,
                        c.Cidade,
                        c.Latitude!.Value,
                        c.Longitude!.Value,
                        c.Status,
                        onu.SinalDbm,
                        onu.StatusOnu,
                        onu.Serial);
                })
                .ToList();

            ClientesJson = JsonSerializer.Serialize(Clientes, JsonOpcoes);
        }
    }
}
