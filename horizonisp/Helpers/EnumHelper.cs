using System.Globalization;
using horizonisp.Models.Enums;

namespace horizonisp.Helpers
{
    public static class EnumHelper
    {
        public static string ObterStatusCliente(StatusCliente status) => status switch
        {
            StatusCliente.Ativo => "Ativo",
            StatusCliente.Inadimplente => "Inadimplente",
            StatusCliente.Suspenso => "Suspenso",
            StatusCliente.Cancelado => "Cancelado",
            _ => status.ToString()
        };

        public static string ObterStatusAssinatura(StatusAssinatura status) => status switch
        {
            StatusAssinatura.Ativa => "Ativa",
            StatusAssinatura.Suspensa => "Suspensa",
            StatusAssinatura.Cancelada => "Cancelada",
            _ => status.ToString()
        };

        public static string ObterStatusFatura(StatusFatura status) => status switch
        {
            StatusFatura.Pendente => "Pendente",
            StatusFatura.Paga => "Paga",
            StatusFatura.Atrasada => "Atrasada",
            StatusFatura.Cancelada => "Cancelada",
            _ => status.ToString()
        };

        public static string ObterTipoPlano(TipoPlano tipo) => tipo switch
        {
            TipoPlano.PPPoE => "PPPoE",
            TipoPlano.Hotspot => "Hotspot",
            TipoPlano.IpFixo => "IP Fixo",
            _ => tipo.ToString()
        };

        public static string Moeda(decimal valor) =>
            valor.ToString("C", new CultureInfo("pt-BR"));
    }
}
