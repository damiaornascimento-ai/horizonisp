using System.ComponentModel.DataAnnotations;
using horizonisp.Models.Enums;

namespace horizonisp.Models
{
    public class Cliente
    {
        public int Id { get; set; }

        [Required, MaxLength(150)]
        public string Nome { get; set; } = string.Empty;

        [Required, MaxLength(18)]
        public string Documento { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string Telefone { get; set; } = string.Empty;

        [MaxLength(200)]
        public string Endereco { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Cidade { get; set; } = string.Empty;

        [MaxLength(2)]
        public string Estado { get; set; } = string.Empty;

        [MaxLength(10)]
        public string Cep { get; set; } = string.Empty;

        public StatusCliente Status { get; set; } = StatusCliente.Ativo;

        public DateTime DataCadastro { get; set; } = DateTime.UtcNow;

        [MaxLength(256)]
        public string? SenhaPortalHash { get; set; }

        public bool PortalAtivo { get; set; } = true;

        public ICollection<Assinatura> Assinaturas { get; set; } = [];
    }
}
