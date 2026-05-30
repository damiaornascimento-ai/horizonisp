using System.ComponentModel.DataAnnotations;
using horizonisp.Models.Enums;

namespace horizonisp.Models
{
    public class Fatura
    {
        public int Id { get; set; }

        [Required]
        public int AssinaturaId { get; set; }

        [Required, MaxLength(7)]
        public string Referencia { get; set; } = string.Empty;

        [Range(0.01, 99999)]
        public decimal Valor { get; set; }

        public DateTime DataVencimento { get; set; }

        public DateTime? DataPagamento { get; set; }

        public StatusFatura Status { get; set; } = StatusFatura.Pendente;

        [MaxLength(512)]
        public string? PixCopiaCola { get; set; }

        [MaxLength(25)]
        public string? PixTxId { get; set; }

        [MaxLength(100)]
        public string? PixGatewayRef { get; set; }

        public DateTime? PixExpiracaoEm { get; set; }

        public DateTime? LembreteVencimentoEnviadoEm { get; set; }

        public DateTime? AvisoAtrasoEnviadoEm { get; set; }

        [MaxLength(54)]
        public string? BoletoLinhaDigitavel { get; set; }

        [MaxLength(44)]
        public string? BoletoCodigoBarras { get; set; }

        [MaxLength(20)]
        public string? BoletoNossoNumero { get; set; }

        public Assinatura Assinatura { get; set; } = null!;

        public ICollection<PagamentoPix> PagamentosPix { get; set; } = [];

        public NotaFiscalServico? NotaFiscalServico { get; set; }
    }
}
