using System.ComponentModel.DataAnnotations;
using horizonisp.Models.Enums;

namespace horizonisp.Models
{
    public class OrdemServico
    {
        public int Id { get; set; }

        [Required]
        public int ClienteId { get; set; }

        public int? AssinaturaId { get; set; }

        public int? ChamadoId { get; set; }

        [Required, MaxLength(150)]
        public string Titulo { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string Descricao { get; set; } = string.Empty;

        public TipoOrdemServico Tipo { get; set; } = TipoOrdemServico.Manutencao;

        public StatusOrdemServico Status { get; set; } = StatusOrdemServico.Aberta;

        [MaxLength(100)]
        public string? TecnicoResponsavel { get; set; }

        [MaxLength(250)]
        public string Endereco { get; set; } = string.Empty;

        public DateTime? DataAgendada { get; set; }

        public DateTime? DataConclusao { get; set; }

        [MaxLength(2000)]
        public string? ObservacaoConclusao { get; set; }

        public DateTime DataAbertura { get; set; } = DateTime.UtcNow;

        public DateTime DataAtualizacao { get; set; } = DateTime.UtcNow;

        public Cliente Cliente { get; set; } = null!;

        public Assinatura? Assinatura { get; set; }

        public Chamado? Chamado { get; set; }
    }
}
