using System.ComponentModel.DataAnnotations;
using horizonisp.Models.Enums;

namespace horizonisp.Models
{
    public class ChamadoMensagem
    {
        public int Id { get; set; }

        [Required]
        public int ChamadoId { get; set; }

        public AutorMensagemChamado AutorTipo { get; set; }

        [Required, MaxLength(100)]
        public string AutorNome { get; set; } = string.Empty;

        [Required, MaxLength(4000)]
        public string Conteudo { get; set; } = string.Empty;

        public DateTime DataEnvio { get; set; } = DateTime.UtcNow;

        public Chamado Chamado { get; set; } = null!;
    }
}
