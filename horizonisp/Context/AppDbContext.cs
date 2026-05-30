using Microsoft.EntityFrameworkCore;
using horizonisp.Models;
using horizonisp.Models.Enums;

namespace horizonisp.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Usuario> Usuarios => Set<Usuario>();
        public DbSet<Cliente> Clientes => Set<Cliente>();
        public DbSet<Plano> Planos => Set<Plano>();
        public DbSet<Assinatura> Assinaturas => Set<Assinatura>();
        public DbSet<Fatura> Faturas => Set<Fatura>();
        public DbSet<Chamado> Chamados => Set<Chamado>();
        public DbSet<ChamadoMensagem> ChamadoMensagens => Set<ChamadoMensagem>();
        public DbSet<PagamentoPix> PagamentosPix => Set<PagamentoPix>();
        public DbSet<Olt> Olts => Set<Olt>();
        public DbSet<Onu> Onus => Set<Onu>();
        public DbSet<OrdemServico> OrdensServico => Set<OrdemServico>();
        public DbSet<NotaFiscalServico> NotasFiscaisServico => Set<NotaFiscalServico>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Cliente>()
                .HasIndex(c => c.Documento)
                .IsUnique();

            modelBuilder.Entity<Cliente>()
                .HasIndex(c => c.Email);

            modelBuilder.Entity<Plano>()
                .HasIndex(p => p.Nome);

            modelBuilder.Entity<Assinatura>()
                .HasIndex(a => a.LoginPppoe)
                .IsUnique();

            modelBuilder.Entity<Assinatura>()
                .HasOne(a => a.Cliente)
                .WithMany(c => c.Assinaturas)
                .HasForeignKey(a => a.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Assinatura>()
                .HasOne(a => a.Plano)
                .WithMany(p => p.Assinaturas)
                .HasForeignKey(a => a.PlanoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Fatura>()
                .HasOne(f => f.Assinatura)
                .WithMany(a => a.Faturas)
                .HasForeignKey(f => f.AssinaturaId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Fatura>()
                .HasIndex(f => new { f.AssinaturaId, f.Referencia })
                .IsUnique();

            modelBuilder.Entity<Fatura>()
                .HasIndex(f => f.PixTxId);

            modelBuilder.Entity<PagamentoPix>()
                .HasOne(p => p.Fatura)
                .WithMany(f => f.PagamentosPix)
                .HasForeignKey(p => p.FaturaId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PagamentoPix>()
                .HasIndex(p => p.TxId);

            modelBuilder.Entity<PagamentoPix>()
                .HasIndex(p => p.EndToEndId);

            modelBuilder.Entity<Chamado>()
                .HasOne(c => c.Cliente)
                .WithMany(cl => cl.Chamados)
                .HasForeignKey(c => c.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Chamado>()
                .HasIndex(c => c.Status);

            modelBuilder.Entity<ChamadoMensagem>()
                .HasOne(m => m.Chamado)
                .WithMany(c => c.Mensagens)
                .HasForeignKey(m => m.ChamadoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Olt>()
                .HasIndex(o => o.Nome);

            modelBuilder.Entity<Onu>()
                .HasIndex(o => o.Serial)
                .IsUnique();

            modelBuilder.Entity<Onu>()
                .HasOne(o => o.Olt)
                .WithMany(olt => olt.Onus)
                .HasForeignKey(o => o.OltId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Onu>()
                .HasOne(o => o.Assinatura)
                .WithMany()
                .HasForeignKey(o => o.AssinaturaId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<OrdemServico>()
                .HasOne(o => o.Cliente)
                .WithMany()
                .HasForeignKey(o => o.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrdemServico>()
                .HasOne(o => o.Assinatura)
                .WithMany()
                .HasForeignKey(o => o.AssinaturaId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<OrdemServico>()
                .HasOne(o => o.Chamado)
                .WithMany()
                .HasForeignKey(o => o.ChamadoId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<OrdemServico>()
                .HasIndex(o => o.Status);

            modelBuilder.Entity<NotaFiscalServico>()
                .HasOne(n => n.Fatura)
                .WithOne(f => f.NotaFiscalServico)
                .HasForeignKey<NotaFiscalServico>(n => n.FaturaId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<NotaFiscalServico>()
                .HasIndex(n => n.FaturaId)
                .IsUnique();

            modelBuilder.Entity<NotaFiscalServico>()
                .HasIndex(n => n.Status);

            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Email)
                .IsUnique();
        }
    }
}
