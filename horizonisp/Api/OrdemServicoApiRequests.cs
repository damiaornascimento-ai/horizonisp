using System.ComponentModel.DataAnnotations;

namespace horizonisp.Api
{
    public class OrdemServicoStatusRequest
    {
        [Required]
        public string Status { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? ObservacaoConclusao { get; set; }

        [MaxLength(100)]
        public string? TecnicoResponsavel { get; set; }

        public DateTime? DataAgendada { get; set; }
    }
}
