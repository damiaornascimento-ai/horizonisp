using Microsoft.AspNetCore.Authentication;

namespace horizonisp.Auth
{
    public class AmbienteAcessoMiddleware(RequestDelegate next)
    {
        private static readonly HashSet<string> CaminhosPublicos = new(StringComparer.OrdinalIgnoreCase)
        {
            "/Login",
            "/Portal/Login",
            "/Portal/EsqueciSenha",
            "/Portal/RedefinirSenha",
            "/Error",
            "/Privacy"
        };

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value ?? "/";

            if (DeveIgnorar(path))
            {
                await next(context);
                return;
            }

            var admin = await context.AuthenticateAsync(AuthSchemes.Admin);
            var cliente = await context.AuthenticateAsync(AuthSchemes.Cliente);
            var isPortal = path.StartsWith("/Portal", StringComparison.OrdinalIgnoreCase);

            if (isPortal && admin.Succeeded)
            {
                context.Response.Redirect("/Index");
                return;
            }

            if (!isPortal && cliente.Succeeded && !admin.Succeeded)
            {
                context.Response.Redirect("/Portal/Index");
                return;
            }

            await next(context);
        }

        private static bool DeveIgnorar(string path)
        {
            if (CaminhosPublicos.Contains(path))
            {
                return true;
            }

            return path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("/css/", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("/js/", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("/lib/", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("/images/", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("/favicon", StringComparison.OrdinalIgnoreCase);
        }
    }
}
