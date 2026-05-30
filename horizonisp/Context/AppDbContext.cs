using Microsoft.EntityFrameworkCore;
using horizonisp.Models;

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

            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Email)
                .IsUnique();
        }
    }
}
