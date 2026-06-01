using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using horizonisp.Auth;
using horizonisp.Context;
using horizonisp.Models;
using horizonisp.Models.Enums;

namespace horizonisp.Services
{
    public interface IClienteAuthService
    {
        Task<Cliente?> ValidarLoginAsync(string identificador, string senha);
        Task EntrarAsync(Cliente cliente);
        Task SairAsync();
    }

    public class ClienteAuthService(
        AppDbContext db,
        IHttpContextAccessor httpContextAccessor,
        PasswordHasher<Cliente> passwordHasher) : IClienteAuthService
    {
        public async Task<Cliente?> ValidarLoginAsync(string identificador, string senha)
        {
            var identificadorNormalizado = identificador.Trim();

            if (identificadorNormalizado.Contains('@')
                && await db.Usuarios.AnyAsync(u => u.Email == identificadorNormalizado && u.Ativo))
            {
                return null;
            }

            var cliente = await db.Clientes.FirstOrDefaultAsync(c =>
                c.PortalAtivo
                && c.Status != StatusCliente.Cancelado
                && (c.Email == identificadorNormalizado || c.Documento == identificadorNormalizado));

            if (cliente is null || string.IsNullOrEmpty(cliente.SenhaPortalHash))
            {
                return null;
            }

            var result = passwordHasher.VerifyHashedPassword(cliente, cliente.SenhaPortalHash, senha);
            return result == PasswordVerificationResult.Success ? cliente : null;
        }

        public async Task EntrarAsync(Cliente cliente)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, cliente.Id.ToString()),
                new(ClaimTypes.Name, cliente.Nome),
                new(ClaimTypes.Email, cliente.Email),
                new(ClaimTypes.Role, AuthRoles.Cliente)
            };

            var identity = new ClaimsIdentity(claims, AuthSchemes.Cliente);
            var principal = new ClaimsPrincipal(identity);

            var httpContext = httpContextAccessor.HttpContext
                ?? throw new InvalidOperationException("HttpContext indisponível.");

            await httpContext.SignOutAsync(AuthSchemes.Admin);

            await httpContext.SignInAsync(
                AuthSchemes.Cliente,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(12)
                });
        }

        public async Task SairAsync()
        {
            var httpContext = httpContextAccessor.HttpContext
                ?? throw new InvalidOperationException("HttpContext indisponível.");

            await httpContext.SignOutAsync(AuthSchemes.Cliente);
        }
    }
}
