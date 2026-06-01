using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using horizonisp.Auth;
using horizonisp.Context;
using horizonisp.Models;

namespace horizonisp.Services
{
    public interface IAuthService
    {
        Task<Usuario?> ValidarLoginAsync(string email, string senha);
        Task EntrarAsync(Usuario usuario);
        Task SairAsync();
    }

    public class AuthService(
        AppDbContext db,
        IHttpContextAccessor httpContextAccessor,
        PasswordHasher<Usuario> passwordHasher) : IAuthService
    {
        public async Task<Usuario?> ValidarLoginAsync(string email, string senha)
        {
            var usuario = await db.Usuarios
                .FirstOrDefaultAsync(u => u.Email == email && u.Ativo);

            if (usuario is null)
            {
                return null;
            }

            var result = passwordHasher.VerifyHashedPassword(usuario, usuario.SenhaHash, senha);
            return result == PasswordVerificationResult.Success ? usuario : null;
        }

        public async Task EntrarAsync(Usuario usuario)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new(ClaimTypes.Name, usuario.Nome),
                new(ClaimTypes.Email, usuario.Email),
                new(ClaimTypes.Role, usuario.Perfil.ToString())
            };

            var identity = new ClaimsIdentity(claims, AuthSchemes.Admin);
            var principal = new ClaimsPrincipal(identity);

            var httpContext = httpContextAccessor.HttpContext
                ?? throw new InvalidOperationException("HttpContext indisponível.");

            await httpContext.SignOutAsync(AuthSchemes.Cliente);

            await httpContext.SignInAsync(
                AuthSchemes.Admin,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                });
        }

        public async Task SairAsync()
        {
            var httpContext = httpContextAccessor.HttpContext
                ?? throw new InvalidOperationException("HttpContext indisponível.");

            await httpContext.SignOutAsync(AuthSchemes.Admin);
            await httpContext.SignOutAsync(AuthSchemes.Cliente);
        }
    }
}
