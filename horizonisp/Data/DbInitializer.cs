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

            await GarantirColunasOltAsync(db);

            if (!await db.Usuarios.AnyAsync())
            {
                await SeedDadosIniciaisAsync(db, passwordHasher, clientePasswordHasher);
            }

            await GarantirSenhasPortalDemoAsync(db, clientePasswordHasher);
            await GarantirChamadoTecnicoDemoAsync(db);
            await GarantirOrdemServicoDemoAsync(db);
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
                CriarClienteDemo("João Silva", "111.444.777-35", "joao.silva@email.com", clientePasswordHasher),
                CriarClienteDemo("Maria Oliveira", "529.982.247-25", "maria.oliveira@email.com", clientePasswordHasher)
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

            var olt = new Olt
            {
                Nome = "OLT Central",
                Host = "10.0.0.1",
                Fabricante = "Huawei",
                Localizacao = "POP Principal",
                Ativo = true
            };
            db.Olts.Add(olt);
            await db.SaveChangesAsync();

            db.Onus.AddRange(
                new Onu
                {
                    OltId = olt.Id,
                    AssinaturaId = assinaturas[0].Id,
                    Serial = "HWTC12345678",
                    Mac = "AA:BB:CC:DD:EE:01",
                    PonPorta = "0/1/1",
                    Status = StatusOnu.Online,
                    SinalDbm = -22,
                    UltimaAtualizacao = DateTime.UtcNow
                },
                new Onu
                {
                    OltId = olt.Id,
                    AssinaturaId = assinaturas[1].Id,
                    Serial = "HWTC87654321",
                    Mac = "AA:BB:CC:DD:EE:02",
                    PonPorta = "0/1/2",
                    Status = StatusOnu.Offline,
                    SinalDbm = -28,
                    UltimaAtualizacao = DateTime.UtcNow
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

        private static async Task GarantirColunasOltAsync(AppDbContext db)
        {
            await db.Database.ExecuteSqlRawAsync("""
                IF NOT EXISTS (
                    SELECT 1 FROM sys.columns
                    WHERE object_id = OBJECT_ID(N'Olts') AND name = N'PortaApi')
                BEGIN
                    ALTER TABLE Olts ADD PortaApi int NOT NULL CONSTRAINT DF_Olts_PortaApi DEFAULT 80;
                    ALTER TABLE Olts ADD SenhaApi nvarchar(100) NOT NULL CONSTRAINT DF_Olts_SenhaApi DEFAULT N'';
                    ALTER TABLE Olts ADD UltimaSincronizacao datetime2 NULL;
                    ALTER TABLE Olts ADD UsuarioApi nvarchar(50) NOT NULL CONSTRAINT DF_Olts_UsuarioApi DEFAULT N'';
                END
                """);
        }

        private static async Task GarantirChamadoTecnicoDemoAsync(AppDbContext db)
        {
            var existeTecnico = await db.Chamados.AnyAsync(c => c.Categoria == CategoriaChamado.Tecnico);
            if (existeTecnico)
            {
                return;
            }

            var cliente = await db.Clientes.FirstOrDefaultAsync(c => c.Email == "maria.oliveira@email.com");
            if (cliente is null)
            {
                return;
            }

            db.Chamados.Add(new Chamado
            {
                ClienteId = cliente.Id,
                Assunto = "Sem conexão — ONU offline",
                Categoria = CategoriaChamado.Tecnico,
                Prioridade = PrioridadeChamado.Alta,
                Status = StatusChamado.Aberto,
                DataAbertura = DateTime.UtcNow.AddHours(-2),
                DataAtualizacao = DateTime.UtcNow.AddHours(-2),
                Mensagens =
                [
                    new ChamadoMensagem
                    {
                        AutorTipo = AutorMensagemChamado.Cliente,
                        AutorNome = cliente.Nome,
                        Conteudo = "A internet caiu desde ontem à noite. Luz vermelha na ONU.",
                        DataEnvio = DateTime.UtcNow.AddHours(-2)
                    }
                ]
            });

            await db.SaveChangesAsync();
        }

        private static async Task GarantirOrdemServicoDemoAsync(AppDbContext db)
        {
            if (await db.OrdensServico.AnyAsync())
            {
                return;
            }

            var chamado = await db.Chamados
                .Include(c => c.Cliente)
                .Include(c => c.Mensagens)
                .FirstOrDefaultAsync(c => c.Categoria == CategoriaChamado.Tecnico);

            if (chamado is null)
            {
                return;
            }

            var assinatura = await db.Assinaturas
                .FirstOrDefaultAsync(a => a.ClienteId == chamado.ClienteId);

            db.OrdensServico.Add(new OrdemServico
            {
                ClienteId = chamado.ClienteId,
                AssinaturaId = assinatura?.Id,
                ChamadoId = chamado.Id,
                Titulo = "Visita técnica — ONU offline",
                Descricao = chamado.Mensagens.OrderBy(m => m.DataEnvio).FirstOrDefault()?.Conteudo ?? chamado.Assunto,
                Tipo = TipoOrdemServico.Manutencao,
                Status = StatusOrdemServico.Aberta,
                Endereco = assinatura?.EnderecoInstalacao ?? chamado.Cliente.Endereco,
                DataAbertura = DateTime.UtcNow.AddHours(-1),
                DataAtualizacao = DateTime.UtcNow.AddHours(-1)
            });

            await db.SaveChangesAsync();
        }
    }
}
