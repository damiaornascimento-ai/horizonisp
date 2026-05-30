using Microsoft.Extensions.Options;
using horizonisp.Configuration;

namespace horizonisp.Api
{
    public class ApiKeyMiddleware(RequestDelegate next, IOptions<HorizonIspOptions> options)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.Request.Path.StartsWithSegments("/api/v1"))
            {
                await next(context);
                return;
            }

            if (!options.Value.Api.Habilitado)
            {
                context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                await context.Response.WriteAsJsonAsync(new { erro = "API desabilitada." });
                return;
            }

            var apiKey = context.Request.Headers["X-Api-Key"].FirstOrDefault();
            if (!string.Equals(apiKey, options.Value.Api.Chave, StringComparison.Ordinal))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new { erro = "API key inválida." });
                return;
            }

            await next(context);
        }
    }
}
