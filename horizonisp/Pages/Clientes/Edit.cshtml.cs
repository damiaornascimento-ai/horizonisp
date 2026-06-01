using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using horizonisp.Context;
using horizonisp.Helpers;
using horizonisp.Models;
using horizonisp.Services;

namespace horizonisp.Pages.Clientes
{
    public class EditModel(AppDbContext db, PasswordHasher<Cliente> passwordHasher, IClienteBloqueioService bloqueioService) : PageModel
    {
        [BindProperty]
        public Cliente Cliente { get; set; } = new();

        [BindProperty]
        public string? SenhaPortal { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            var cliente = await db.Clientes.FirstOrDefaultAsync(c => c.Id == id);
            if (cliente is null)
            {
                return NotFound();
            }

            Cliente = cliente;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            TryValidateModel(Cliente, nameof(Cliente));

            if (!string.IsNullOrWhiteSpace(Cliente.Documento) && !DocumentoValidator.EhValido(Cliente.Documento))
            {
                ModelState.AddModelError($"{nameof(Cliente)}.{nameof(Cliente.Documento)}", "CPF ou CNPJ inválido.");
            }

            var existente = await db.Clientes.FirstOrDefaultAsync(c => c.Id == Cliente.Id);
            if (existente is null)
            {
                return NotFound();
            }

            var documentoFormatado = DocumentoValidator.Formatar(Cliente.Documento);
            if (await db.Clientes.AnyAsync(c => c.Documento == documentoFormatado && c.Id != Cliente.Id))
            {
                ModelState.AddModelError($"{nameof(Cliente)}.{nameof(Cliente.Documento)}", "Documento já cadastrado.");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            existente.Nome = Cliente.Nome;
            existente.Documento = documentoFormatado;
            existente.Email = Cliente.Email;
            existente.Telefone = Cliente.Telefone;
            existente.Endereco = Cliente.Endereco;
            existente.Cidade = Cliente.Cidade;
            existente.Estado = Cliente.Estado;
            existente.Cep = Cliente.Cep;
            existente.Status = Cliente.Status;
            existente.PortalAtivo = Cliente.PortalAtivo;

            if (!string.IsNullOrWhiteSpace(SenhaPortal))
            {
                existente.SenhaPortalHash = passwordHasher.HashPassword(existente, SenhaPortal);
            }

            await db.SaveChangesAsync();
            return RedirectToPage("Index");
        }

        public async Task<IActionResult> OnPostBloquearAsync(int id)
        {
            var resultado = await bloqueioService.BloquearManualmenteAsync(id);
            TempData[resultado.Sucesso ? "Sucesso" : "Erro"] = resultado.Mensagem;
            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostAtivarAsync(int id)
        {
            var resultado = await bloqueioService.AtivarManualmenteAsync(id);
            TempData[resultado.Sucesso ? "Sucesso" : "Erro"] = resultado.Mensagem;
            return RedirectToPage(new { id });
        }
    }
}
