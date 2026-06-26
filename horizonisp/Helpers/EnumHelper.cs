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

        public static string ClasseBadgeStatusCliente(StatusCliente status) => status switch
        {
            StatusCliente.Ativo => "bg-success",
            StatusCliente.Inadimplente => "bg-warning text-dark",
            StatusCliente.Suspenso => "bg-secondary",
            StatusCliente.Cancelado => "bg-danger",
            _ => "bg-secondary"
        };

        public static string ObterCategoriaConexaoCliente(CategoriaConexaoCliente categoria) => categoria switch
        {
            CategoriaConexaoCliente.Online => "Online",
            CategoriaConexaoCliente.Offline => "Offline",
            CategoriaConexaoCliente.Bloqueado => "Bloqueado",
            _ => categoria.ToString()
        };

        public static string ClasseBadgeCategoriaConexao(CategoriaConexaoCliente categoria) => categoria switch
        {
            CategoriaConexaoCliente.Online => "bg-success",
            CategoriaConexaoCliente.Offline => "bg-warning text-dark",
            CategoriaConexaoCliente.Bloqueado => "bg-danger",
            _ => "bg-secondary"
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

        public static string ObterStatusChamado(StatusChamado status) => status switch
        {
            StatusChamado.Aberto => "Aberto",
            StatusChamado.EmAndamento => "Em andamento",
            StatusChamado.Resolvido => "Resolvido",
            StatusChamado.Fechado => "Fechado",
            _ => status.ToString()
        };

        public static string ObterPrioridadeChamado(PrioridadeChamado prioridade) => prioridade switch
        {
            PrioridadeChamado.Baixa => "Baixa",
            PrioridadeChamado.Normal => "Normal",
            PrioridadeChamado.Alta => "Alta",
            PrioridadeChamado.Urgente => "Urgente",
            _ => prioridade.ToString()
        };

        public static string ObterCategoriaChamado(CategoriaChamado categoria) => categoria switch
        {
            CategoriaChamado.Financeiro => "Financeiro",
            CategoriaChamado.Tecnico => "Técnico",
            CategoriaChamado.Comercial => "Comercial",
            CategoriaChamado.Outros => "Outros",
            _ => categoria.ToString()
        };

        public static string ObterStatusOnu(StatusOnu status) => status switch
        {
            StatusOnu.Online => "Online",
            StatusOnu.Offline => "Offline",
            StatusOnu.Desconhecido => "Desconhecido",
            _ => status.ToString()
        };

        public static string ObterPerfilUsuario(PerfilUsuario perfil) => perfil switch
        {
            PerfilUsuario.Admin => "Administrador",
            PerfilUsuario.Operador => "Operador",
            _ => perfil.ToString()
        };

        public static string ObterStatusOrdemServico(StatusOrdemServico status) => status switch
        {
            StatusOrdemServico.Aberta => "Aberta",
            StatusOrdemServico.Agendada => "Agendada",
            StatusOrdemServico.EmCampo => "Em campo",
            StatusOrdemServico.Concluida => "Concluída",
            StatusOrdemServico.Cancelada => "Cancelada",
            _ => status.ToString()
        };

        public static string ObterTipoOrdemServico(TipoOrdemServico tipo) => tipo switch
        {
            TipoOrdemServico.Instalacao => "Instalação",
            TipoOrdemServico.Manutencao => "Manutenção",
            TipoOrdemServico.Retirada => "Retirada",
            TipoOrdemServico.Vistoria => "Vistoria",
            _ => tipo.ToString()
        };

        public static string ObterStatusNfse(StatusNfse status) => status switch
        {
            StatusNfse.Pendente => "Pendente",
            StatusNfse.Processando => "Processando",
            StatusNfse.Autorizada => "Autorizada",
            StatusNfse.Rejeitada => "Rejeitada",
            StatusNfse.Cancelada => "Cancelada",
            _ => status.ToString()
        };
    }
}
