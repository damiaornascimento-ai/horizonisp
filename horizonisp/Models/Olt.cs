using System.ComponentModel.DataAnnotations;

namespace horizonisp.Models
{
    public class Olt
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Nome { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Host { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Fabricante { get; set; } = string.Empty;

        [MaxLength(150)]
        public string Localizacao { get; set; } = string.Empty;

        public bool Ativo { get; set; } = true;

        [MaxLength(50)]
        public string UsuarioApi { get; set; } = string.Empty;

        [MaxLength(100)]
        public string SenhaApi { get; set; } = string.Empty;

        public int PortaApi { get; set; } = 80;

        public DateTime? UltimaSincronizacao { get; set; }

        public ICollection<Onu> Onus { get; set; } = [];
    }
}
