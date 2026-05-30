using System.ComponentModel.DataAnnotations;

namespace horizonisp.Models
{
    public class PagamentoPix
    {
        public int Id { get; set; }

        [Required]
        public int FaturaId { get; set; }

        [Required, MaxLength(25)]
        public string TxId { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? EndToEndId { get; set; }

        [Range(0.01, 99999)]
        public decimal Valor { get; set; }

        [Required, MaxLength(20)]
        public string Origem { get; set; } = "Webhook";

        public DateTime RecebidoEm { get; set; } = DateTime.UtcNow;

        public Fatura Fatura { get; set; } = null!;
    }
}
