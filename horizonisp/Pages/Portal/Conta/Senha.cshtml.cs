using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using horizonisp.Context;
using horizonisp.Helpers;
using horizonisp.Models;
using horizonisp.Services;

namespace horizonisp.Pages.Portal.Conta
{
    public class SenhaModel(IPortalService portalService, AppDbContext db) : PageModel
    {
        [BindProperty]
        public SenhaPortalInput SenhaPortal { get; set; } = new();

        [BindProperty]
        public SenhaPppoeInput SenhaPppoe { get; set; } = new();

        public IList<Assinatura> Assinaturas { get; private set; } = [];

        public string? MensagemSucesso { get; set; }
        public string? MensagemErro { get; set; }

        public class SenhaPortalInput
        {
            [Required(ErrorMessage = "Informe a senha atual.")]
            [DataType(DataType.Password)]
            [Display(Name = "Senha atual do portal")]
            public string Atual { get; set; } = string.Empty;

            [Required(ErrorMessage = "Informe a nova senha.")]
            [StringLength(100, MinimumLength = 6, ErrorMessage = "Use entre 6 e 100 caracteres.")]
            [DataType(DataType.Password)]
            [Display(Name = "Nova senha")]
            public string Nova { get; set; } = string.Empty;

            [Required(ErrorMessage = "Confirme a nova senha.")]
            [Compare(nameof(Nova), ErrorMessage = "A confirmação não confere.")]
            [DataType(DataType.Password)]
            [Display(Name = "Confirmar nova senha")]
            public string Confirmacao { get; set; } = string.Empty;
        }

        public class SenhaPppoeInput
        {
            [Required(ErrorMessage = "Selecione a assinatura.")]
            [Display(Name = "Assinatura")]
            public int AssinaturaId { get; set; }

            [Required(ErrorMessage = "Informe a senha PPPoE atual.")]
            [DataType(DataType.Password)]
            [Display(Name = "Senha PPPoE atual")]
            public string Atual { get; set; } = string.Empty;

            [Required(ErrorMessage = "Informe a nova senha PPPoE.")]
            [StringLength(100, MinimumLength = 4, ErrorMessage = "Use entre 4 e 100 caracteres.")]
            [DataType(DataType.Password)]
            [Display(Name = "Nova senha PPPoE")]
            public string Nova { get; set; } = string.Empty;

            [Required(ErrorMessage = "Confirme a nova senha.")]
            [Compare(nameof(Nova), ErrorMessage = "A confirmação não confere.")]
            [DataType(DataType.Password)]
            [Display(Name = "Confirmar nova senha")]
            public string Confirmacao { get; set; } = string.Empty;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            return await CarregarPaginaAsync();
        }

        public async Task<IActionResult> OnPostAlterarSenhaPortalAsync()
        {
            var clienteId = User.ObterClienteId();
            if (clienteId is null)
            {
                return RedirectToPage("/Portal/Login");
            }

            if (!TryValidateModel(SenhaPortal, nameof(SenhaPortal)))
            {
                await CarregarAssinaturasAsync(clienteId.Value);
                return Page();
            }

            var ok = await portalService.AlterarSenhaPortalAsync(clienteId.Value, SenhaPortal.Atual, SenhaPortal.Nova);
            if (!ok)
            {
                MensagemErro = "Senha atual do portal incorreta.";
                await CarregarAssinaturasAsync(clienteId.Value);
                return Page();
            }

            MensagemSucesso = "Senha do portal alterada com sucesso.";
            SenhaPortal = new SenhaPortalInput();
            return await CarregarPaginaAsync();
        }

        public async Task<IActionResult> OnPostAlterarSenhaPppoeAsync()
        {
            var clienteId = User.ObterClienteId();
            if (clienteId is null)
            {
                return RedirectToPage("/Portal/Login");
            }

            if (!TryValidateModel(SenhaPppoe, nameof(SenhaPppoe)))
            {
                await CarregarAssinaturasAsync(clienteId.Value);
                return Page();
            }

            var ok = await portalService.AlterarSenhaPppoeAsync(
                clienteId.Value,
                SenhaPppoe.AssinaturaId,
                SenhaPppoe.Atual,
                SenhaPppoe.Nova);

            if (!ok)
            {
                MensagemErro = "Assinatura ou senha PPPoE atual incorreta.";
                await CarregarAssinaturasAsync(clienteId.Value);
                return Page();
            }

            MensagemSucesso = "Senha PPPoE alterada com sucesso.";
            SenhaPppoe = new SenhaPppoeInput();
            return await CarregarPaginaAsync();
        }

        private async Task<IActionResult> CarregarPaginaAsync()
        {
            var clienteId = User.ObterClienteId();
            if (clienteId is null)
            {
                return RedirectToPage("/Portal/Login");
            }

            await CarregarAssinaturasAsync(clienteId.Value);
            return Page();
        }

        private async Task CarregarAssinaturasAsync(int clienteId)
        {
            Assinaturas = await db.Assinaturas
                .Include(a => a.Plano)
                .Where(a => a.ClienteId == clienteId)
                .OrderByDescending(a => a.DataInicio)
                .ToListAsync();
        }
    }
}
