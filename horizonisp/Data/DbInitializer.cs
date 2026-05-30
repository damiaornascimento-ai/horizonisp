using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using horizonisp.Context;
using horizonisp.Models;
using horizonisp.Models.Enums;

namespace horizonisp.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(
            AppDbContext db,
            PasswordHasher<Usuario> passwordHasher,
            PasswordHasher<Cliente> clientePasswordHasher)
        {
            await db.Database.MigrateAsync();

            if (!await db.Usuarios.AnyAsync())
            {
                await SeedDadosIniciaisAsync(db, passwordHasher, clientePasswordHasher);
            }

            await GarantirSenhasPortalDemoAsync(db, clientePasswordHasher);
        }

        private static async Task SeedDadosIniciaisAsync(
            AppDbContext db,
            PasswordHasher<Usuario> passwordHasher,
            PasswordHasher<Cliente> clientePasswordHasher)
        {
            var admin = new Usuario
            {
                Nome = "Administrador",
                Email = "admin@horizonisp.local",
                Perfil = PerfilUsuario.Admin,
                Ativo = true
            };
            admin.SenhaHash = passwordHasher.HashPassword(admin, "admin123");
            db.Usuarios.Add(admin);

            var planos = new[]
            {
                new Plano
                {
                    Nome = "100 Mega",
                    VelocidadeDownloadMbps = 100,
                    VelocidadeUploadMbps = 50,
                    PrecoMensal = 79.90m,
                    Tipo = TipoPlano.PPPoE,
                    Descricao = "Plano residencial básico"
                },
                new Plano
                {
                    Nome = "300 Mega",
                    VelocidadeDownloadMbps = 300,
                    VelocidadeUploadMbps = 150,
                    PrecoMensal = 119.90m,
                    Tipo = TipoPlano.PPPoE,
                    Descricao = "Plano residencial intermediário"
                },
                new Plano
                {
                    Nome = "600 Mega",
                    VelocidadeDownloadMbps = 600,
                    VelocidadeUploadMbps = 300,
                    PrecoMensal = 159.90m,
                    Tipo = TipoPlano.PPPoE,
                    Descricao = "Plano residencial premium"
                }
            };
            db.Planos.AddRange(planos);

            var clientes = new[]
            {
                CriarClienteDemo("João Silva", "123.456.789-00", "joao.silva@email.com", clientePasswordHasher),
                CriarClienteDemo("Maria Oliveira", "987.654.321-00", "maria.oliveira@email.com", clientePasswordHasher)
            };
            db.Clientes.AddRange(clientes);

            await db.SaveChangesAsync();

            var assinaturas = new[]
            {
                new Assinatura
                {
                    ClienteId = clientes[0].Id,
                    PlanoId = planos[0].Id,
                    LoginPppoe = "joao.silva",
                    SenhaPppoe = "senha123",
                    EnderecoInstalacao = clientes[0].Endereco,
                    DataInicio = DateTime.UtcNow.AddMonths(-3),
                    Status = StatusAssinatura.Ativa
                },
                new Assinatura
                {
                    ClienteId = clientes[1].Id,
                    PlanoId = planos[1].Id,
                    LoginPppoe = "maria.oliveira",
                    SenhaPppoe = "senha456",
                    EnderecoInstalacao = clientes[1].Endereco,
                    DataInicio = DateTime.UtcNow.AddMonths(-1),
                    Status = StatusAssinatura.Ativa
                }
            };
            db.Assinaturas.AddRange(assinaturas);

            await db.SaveChangesAsync();

            var referenciaAtual = DateTime.UtcNow.ToString("yyyy-MM");
            var vencimento = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 10, 0, 0, 0, DateTimeKind.Utc);

            db.Faturas.AddRange(
                new Fatura
                {
                    AssinaturaId = assinaturas[0].Id,
                    Referencia = referenciaAtual,
                    Valor = planos[0].PrecoMensal,
                    DataVencimento = vencimento,
                    Status = StatusFatura.Pendente
                },
                new Fatura
                {
                    AssinaturaId = assinaturas[1].Id,
                    Referencia = referenciaAtual,
                    Valor = planos[1].PrecoMensal,
                    DataVencimento = vencimento.AddDays(-5),
                    Status = StatusFatura.Atrasada
                });

            await db.SaveChangesAsync();
        }

        private static Cliente CriarClienteDemo(
            string nome,
            string documento,
            string email,
            PasswordHasher<Cliente> clientePasswordHasher)
        {
            var cliente = new Cliente
            {
                Nome = nome,
                Documento = documento,
                Email = email,
                Telefone = "(11) 90000-0000",
                Endereco = "Endereço de demonstração",
                Cidade = "São Paulo",
                Estado = "SP",
                Cep = "01000-000",
                Status = StatusCliente.Ativo,
                PortalAtivo = true
            };

            cliente.SenhaPortalHash = clientePasswordHasher.HashPassword(cliente, "cliente123");
            return cliente;
        }

        private static async Task GarantirSenhasPortalDemoAsync(
            AppDbContext db,
            PasswordHasher<Cliente> clientePasswordHasher)
        {
            var demos = new Dictionary<string, string>
            {
                ["joao.silva@email.com"] = "cliente123",
                ["maria.oliveira@email.com"] = "cliente123"
            };

            foreach (var (email, senha) in demos)
            {
                var cliente = await db.Clientes.FirstOrDefaultAsync(c => c.Email == email);
                if (cliente is null || !string.IsNullOrEmpty(cliente.SenhaPortalHash))
                {
                    continue;
                }

                cliente.PortalAtivo = true;
                cliente.SenhaPortalHash = clientePasswordHasher.HashPassword(cliente, senha);
            }

            await db.SaveChangesAsync();
        }
    }
}
