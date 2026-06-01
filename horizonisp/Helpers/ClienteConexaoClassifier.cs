using horizonisp.Models;
using horizonisp.Models.Enums;

namespace horizonisp.Helpers
{
    public static class ClienteConexaoClassifier
    {
        public static CategoriaConexaoCliente Classificar(
            Cliente cliente,
            IReadOnlyDictionary<int, List<Onu>> onusPorAssinatura)
        {
            if (cliente.Status is StatusCliente.Suspenso
                or StatusCliente.Inadimplente
                or StatusCliente.Cancelado)
            {
                return CategoriaConexaoCliente.Bloqueado;
            }

            var onus = cliente.Assinaturas
                .Where(a => onusPorAssinatura.ContainsKey(a.Id))
                .SelectMany(a => onusPorAssinatura[a.Id])
                .ToList();

            if (onus.Count == 0)
            {
                return CategoriaConexaoCliente.Online;
            }

            if (onus.Any(o => o.Status == StatusOnu.Online))
            {
                return CategoriaConexaoCliente.Online;
            }

            if (onus.Any(o => o.Status == StatusOnu.Offline))
            {
                return CategoriaConexaoCliente.Offline;
            }

            return CategoriaConexaoCliente.Online;
        }
    }
}
