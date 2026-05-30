using System.Security.Claims;
using horizonisp.Auth;

namespace horizonisp.Helpers
{
    public static class ClienteUserExtensions
    {
        public static int? ObterClienteId(this ClaimsPrincipal user)
        {
            if (!user.IsInRole(AuthRoles.Cliente))
            {
                return null;
            }

            var id = user.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(id, out var clienteId) ? clienteId : null;
        }
    }
}
