using System.ComponentModel.DataAnnotations;
using horizonisp.Models.Enums;

namespace horizonisp.Models
{
    public class Chamado
    {
        public int Id { get; set; }

        [Required]
        public int ClienteId { get; set; }

        [Required, MaxLength(150)]
        public string Assunto { get; set; } = string.Empty;

        public CategoriaChamado Categoria { get; set; } = CategoriaChamado.Outros;

        public PrioridadeChamado Prioridade { get; set; } = PrioridadeChamado.Normal;

        public StatusChamado Status { get; set; } = StatusChamado.Aberto;

        public DateTime DataAbertura { get; set; } = DateTime.UtcNow;

        public DateTime DataAtualizacao { get; set; } = DateTime.UtcNow;

        public Cliente Cliente { get; set; } = null!;

        public ICollection<ChamadoMensagem> Mensagens { get; set; } = [];
    }
}
