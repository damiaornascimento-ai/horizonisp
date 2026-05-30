using System.ComponentModel.DataAnnotations;
using horizonisp.Models.Enums;

namespace horizonisp.Models
{
    public class Assinatura
    {
        public int Id { get; set; }

        [Required]
        public int ClienteId { get; set; }

        [Required]
        public int PlanoId { get; set; }

        [Required, MaxLength(50)]
        public string LoginPppoe { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string SenhaPppoe { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? EnderecoInstalacao { get; set; }

        public DateTime DataInicio { get; set; } = DateTime.UtcNow;

        public DateTime? DataFim { get; set; }

        public StatusAssinatura Status { get; set; } = StatusAssinatura.Ativa;

        [MaxLength(500)]
        public string? Observacoes { get; set; }

        public Cliente Cliente { get; set; } = null!;

        public Plano Plano { get; set; } = null!;

        public ICollection<Fatura> Faturas { get; set; } = [];
    }
}
