using System.ComponentModel.DataAnnotations;
using horizonisp.Models.Enums;

namespace horizonisp.Models
{
    public class NotaFiscalServico
    {
        public int Id { get; set; }

        [Required]
        public int FaturaId { get; set; }

        [MaxLength(20)]
        public string Numero { get; set; } = string.Empty;

        [MaxLength(50)]
        public string CodigoVerificacao { get; set; } = string.Empty;

        [Range(0.01, 99999)]
        public decimal Valor { get; set; }

        public StatusNfse Status { get; set; } = StatusNfse.Pendente;

        public DateTime? DataEmissao { get; set; }

        [MaxLength(500)]
        public string Discriminacao { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? MensagemErro { get; set; }

        [MaxLength(300)]
        public string? LinkPdf { get; set; }

        public Fatura Fatura { get; set; } = null!;
    }
}
