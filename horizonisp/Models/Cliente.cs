using System.ComponentModel.DataAnnotations;
using horizonisp.Models.Enums;

namespace horizonisp.Models
{
    public class Cliente
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Informe o nome do cliente.")]
        [MaxLength(150, ErrorMessage = "O nome deve ter no máximo 150 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe o CPF ou CNPJ.")]
        [MaxLength(18, ErrorMessage = "O documento deve ter no máximo 18 caracteres.")]
        public string Documento { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe o e-mail.")]
        [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
        [MaxLength(150, ErrorMessage = "O e-mail deve ter no máximo 150 caracteres.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe o telefone.")]
        [MaxLength(20, ErrorMessage = "O telefone deve ter no máximo 20 caracteres.")]
        public string Telefone { get; set; } = string.Empty;

        [MaxLength(200, ErrorMessage = "O endereço deve ter no máximo 200 caracteres.")]
        public string Endereco { get; set; } = string.Empty;

        [MaxLength(100, ErrorMessage = "A cidade deve ter no máximo 100 caracteres.")]
        public string Cidade { get; set; } = string.Empty;

        [MaxLength(2, ErrorMessage = "Use a sigla do estado com 2 letras (ex.: SP).")]
        public string Estado { get; set; } = string.Empty;

        [MaxLength(10, ErrorMessage = "O CEP deve ter no máximo 10 caracteres.")]
        public string Cep { get; set; } = string.Empty;

        public StatusCliente Status { get; set; } = StatusCliente.Ativo;

        public DateTime DataCadastro { get; set; } = DateTime.UtcNow;

        [MaxLength(256)]
        public string? SenhaPortalHash { get; set; }

        [MaxLength(64)]
        public string? RecuperacaoSenhaToken { get; set; }

        public DateTime? RecuperacaoSenhaExpiraEm { get; set; }

        public bool PortalAtivo { get; set; } = true;

        public ICollection<Assinatura> Assinaturas { get; set; } = [];
        public ICollection<Chamado> Chamados { get; set; } = [];
    }
}
