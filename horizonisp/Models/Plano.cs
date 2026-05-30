using System.ComponentModel.DataAnnotations;
using horizonisp.Models.Enums;

namespace horizonisp.Models
{
    public class Plano
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Nome { get; set; } = string.Empty;

        [Range(1, 10000)]
        public int VelocidadeDownloadMbps { get; set; }

        [Range(1, 10000)]
        public int VelocidadeUploadMbps { get; set; }

        [Range(0.01, 99999)]
        public decimal PrecoMensal { get; set; }

        public TipoPlano Tipo { get; set; } = TipoPlano.PPPoE;

        public bool Ativo { get; set; } = true;

        [MaxLength(500)]
        public string? Descricao { get; set; }

        public ICollection<Assinatura> Assinaturas { get; set; } = [];
    }
}
