using System.ComponentModel.DataAnnotations;
using horizonisp.Models.Enums;

namespace horizonisp.Models
{
    public class Onu
    {
        public int Id { get; set; }

        [Required]
        public int OltId { get; set; }

        public int? AssinaturaId { get; set; }

        [Required, MaxLength(50)]
        public string Serial { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Mac { get; set; }

        [MaxLength(30)]
        public string PonPorta { get; set; } = string.Empty;

        public StatusOnu Status { get; set; } = StatusOnu.Desconhecido;

        public int? SinalDbm { get; set; }

        public DateTime? UltimaAtualizacao { get; set; }

        public Olt Olt { get; set; } = null!;

        public Assinatura? Assinatura { get; set; }
    }
}
