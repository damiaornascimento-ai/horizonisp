using System.ComponentModel.DataAnnotations;

namespace horizonisp.Api
{
    public class ChamadoStatusRequest
    {
        [Required]
        public string Status { get; set; } = string.Empty;
    }

    public class ChamadoMensagemRequest
    {
        [Required, MaxLength(4000)]
        public string Conteudo { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string AutorNome { get; set; } = string.Empty;
    }
}
