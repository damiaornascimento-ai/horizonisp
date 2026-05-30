using Microsoft.EntityFrameworkCore;
using horizonisp.Context;
using horizonisp.Models.Enums;
using horizonisp.Services;

namespace horizonisp.Api
{
    public static class V1Endpoints
    {
        public static RouteGroupBuilder MapV1Endpoints(this WebApplication app)
        {
            var api = app.MapGroup("/api/v1");

            api.MapGet("/clientes", async (AppDbContext db) =>
            {
                var clientes = await db.Clientes
                    .OrderBy(c => c.Nome)
                    .Select(c => new
                    {
                        c.Id,
                        c.Nome,
                        c.Documento,
                        c.Email,
                        c.Telefone,
                        Status = c.Status.ToString()
                    })
                    .ToListAsync();

                return Results.Ok(clientes);
            });

            api.MapGet("/clientes/{id:int}", async (int id, AppDbContext db) =>
            {
                var cliente = await db.Clientes
                    .Where(c => c.Id == id)
                    .Select(c => new
                    {
                        c.Id,
                        c.Nome,
                        c.Documento,
                        c.Email,
                        c.Telefone,
                        c.Cidade,
                        c.Estado,
                        c.Endereco,
                        c.Cep,
                        Status = c.Status.ToString(),
                        c.DataCadastro
                    })
                    .FirstOrDefaultAsync();

                return cliente is null ? Results.NotFound() : Results.Ok(cliente);
            });

            api.MapGet("/assinaturas", async (AppDbContext db) =>
            {
                var itens = await db.Assinaturas
                    .Include(a => a.Cliente)
                    .Include(a => a.Plano)
                    .OrderByDescending(a => a.DataInicio)
                    .Select(a => new
                    {
                        a.Id,
                        Cliente = a.Cliente.Nome,
                        a.ClienteId,
                        Plano = a.Plano.Nome,
                        a.LoginPppoe,
                        a.EnderecoInstalacao,
                        Status = a.Status.ToString(),
                        a.DataInicio
                    })
                    .ToListAsync();

                return Results.Ok(itens);
            });

            api.MapGet("/faturas", async (StatusFatura? status, AppDbContext db) =>
            {
                var query = db.Faturas
                    .Include(f => f.Assinatura)
                        .ThenInclude(a => a.Cliente)
                    .AsQueryable();

                if (status.HasValue)
                {
                    query = query.Where(f => f.Status == status.Value);
                }

                var faturas = await query
                    .OrderByDescending(f => f.DataVencimento)
                    .Select(f => new
                    {
                        f.Id,
                        f.Referencia,
                        Cliente = f.Assinatura.Cliente.Nome,
                        f.Valor,
                        f.DataVencimento,
                        f.DataPagamento,
                        Status = f.Status.ToString(),
                        f.PixTxId
                    })
                    .ToListAsync();

                return Results.Ok(faturas);
            });

            api.MapPost("/faturas/{id:int}/pagar", async (int id, IFaturamentoService faturamento) =>
            {
                await faturamento.RegistrarPagamentoAsync(id);
                return Results.Ok(new { mensagem = "Pagamento registrado.", faturaId = id });
            });

            api.MapGet("/chamados", async (StatusChamado? status, AppDbContext db) =>
            {
                var query = db.Chamados.Include(c => c.Cliente).AsQueryable();
                if (status.HasValue)
                {
                    query = query.Where(c => c.Status == status.Value);
                }

                var chamados = await query
                    .OrderByDescending(c => c.DataAtualizacao)
                    .Select(c => new
                    {
                        c.Id,
                        c.Assunto,
                        Cliente = c.Cliente.Nome,
                        Categoria = c.Categoria.ToString(),
                        Prioridade = c.Prioridade.ToString(),
                        Status = c.Status.ToString(),
                        c.DataAbertura,
                        c.DataAtualizacao
                    })
                    .ToListAsync();

                return Results.Ok(chamados);
            });

            api.MapGet("/chamados/{id:int}", async (int id, IChamadoService chamados) =>
            {
                var chamado = await chamados.ObterAsync(id);
                if (chamado is null)
                {
                    return Results.NotFound();
                }

                return Results.Ok(MapearChamadoDetalhe(chamado));
            });

            api.MapPatch("/chamados/{id:int}", async (
                int id,
                ChamadoStatusRequest request,
                IChamadoService chamados) =>
            {
                if (!Enum.TryParse<StatusChamado>(request.Status, ignoreCase: true, out var status))
                {
                    return Results.BadRequest(new { erro = "Status inválido." });
                }

                try
                {
                    await chamados.AtualizarStatusAsync(id, status);
                    return Results.Ok(new { mensagem = "Status atualizado.", chamadoId = id, status = status.ToString() });
                }
                catch (InvalidOperationException ex)
                {
                    return Results.NotFound(new { erro = ex.Message });
                }
            });

            api.MapPost("/chamados/{id:int}/mensagens", async (
                int id,
                ChamadoMensagemRequest request,
                IChamadoService chamados) =>
            {
                try
                {
                    var mensagem = await chamados.ResponderAsync(
                        id,
                        request.Conteudo,
                        AutorMensagemChamado.Operador,
                        request.AutorNome);

                    return Results.Ok(new
                    {
                        mensagemId = mensagem.Id,
                        mensagem.DataEnvio,
                        mensagem.AutorNome
                    });
                }
                catch (InvalidOperationException ex)
                {
                    return Results.BadRequest(new { erro = ex.Message });
                }
            });

            api.MapGet("/tecnico/chamados", async (IChamadoService chamados, AppDbContext db) =>
            {
                var lista = await chamados.ListarParaTecnicoAsync();
                var assinaturas = await db.Assinaturas
                    .Where(a => lista.Select(c => c.ClienteId).Contains(a.ClienteId))
                    .Select(a => new { a.ClienteId, a.EnderecoInstalacao, a.LoginPppoe })
                    .ToListAsync();

                var porCliente = assinaturas
                    .GroupBy(a => a.ClienteId)
                    .ToDictionary(g => g.Key, g => g.First());

                return Results.Ok(lista.Select(c =>
                {
                    porCliente.TryGetValue(c.ClienteId, out var assinatura);
                    return new
                    {
                        c.Id,
                        c.Assunto,
                        Categoria = c.Categoria.ToString(),
                        Prioridade = c.Prioridade.ToString(),
                        Status = c.Status.ToString(),
                        c.DataAbertura,
                        c.DataAtualizacao,
                        Cliente = new
                        {
                            c.Cliente.Id,
                            c.Cliente.Nome,
                            c.Cliente.Telefone,
                            c.Cliente.Endereco,
                            c.Cliente.Cidade,
                            c.Cliente.Estado,
                            c.Cliente.Cep,
                            EnderecoInstalacao = assinatura?.EnderecoInstalacao,
                            LoginPppoe = assinatura?.LoginPppoe
                        },
                        UltimaMensagem = c.Mensagens.OrderByDescending(m => m.DataEnvio).FirstOrDefault()?.Conteudo
                    };
                }));
            });

            api.MapGet("/olts", async (IRedeService rede) =>
            {
                var olts = await rede.ListarOltsAsync();
                return Results.Ok(olts.Select(o => new
                {
                    o.Id,
                    o.Nome,
                    o.Host,
                    o.Fabricante,
                    o.Localizacao,
                    o.Ativo,
                    o.UltimaSincronizacao,
                    TotalOnus = o.Onus.Count,
                    OnusOffline = o.Onus.Count(onu => onu.Status == StatusOnu.Offline)
                }));
            });

            api.MapGet("/onus", async (int? oltId, IRedeService rede) =>
            {
                var onus = await rede.ListarOnusAsync(oltId);
                return Results.Ok(onus.Select(o => new
                {
                    o.Id,
                    Olt = o.Olt.Nome,
                    o.OltId,
                    o.Serial,
                    o.Mac,
                    o.PonPorta,
                    Status = o.Status.ToString(),
                    o.SinalDbm,
                    o.UltimaAtualizacao,
                    Cliente = o.Assinatura?.Cliente.Nome,
                    o.AssinaturaId
                }));
            });

            api.MapGet("/rede/resumo", async (IRedeService rede) =>
            {
                var onus = await rede.ListarOnusAsync();
                var olts = await rede.ListarOltsAsync();

                return Results.Ok(new
                {
                    TotalOlts = olts.Count,
                    OltsAtivas = olts.Count(o => o.Ativo),
                    TotalOnus = onus.Count,
                    OnusOnline = onus.Count(o => o.Status == StatusOnu.Online),
                    OnusOffline = onus.Count(o => o.Status == StatusOnu.Offline),
                    UltimaSincronizacao = olts.Max(o => o.UltimaSincronizacao)
                });
            });

            api.MapPost("/rede/sincronizar", async (int? oltId, IRedeSincronizacaoService sincronizacao) =>
            {
                var resultado = oltId.HasValue
                    ? await sincronizacao.SincronizarOltAsync(oltId.Value)
                    : await sincronizacao.SincronizarTodasAsync();

                return Results.Ok(resultado);
            });

            api.MapGet("/ordens-servico", async (StatusOrdemServico? status, IOrdemServicoService ordens) =>
            {
                var lista = await ordens.ListarAsync(status);
                return Results.Ok(lista.Select(MapearOrdemServicoResumo));
            });

            api.MapGet("/tecnico/ordens-servico", async (IOrdemServicoService ordens) =>
            {
                var lista = await ordens.ListarParaTecnicoAsync();
                return Results.Ok(lista.Select(o => new
                {
                    o.Id,
                    o.Titulo,
                    Tipo = o.Tipo.ToString(),
                    Status = o.Status.ToString(),
                    o.TecnicoResponsavel,
                    o.Endereco,
                    o.DataAgendada,
                    o.DataAbertura,
                    o.Descricao,
                    o.ChamadoId,
                    Cliente = new
                    {
                        o.Cliente.Id,
                        o.Cliente.Nome,
                        o.Cliente.Telefone,
                        o.Cliente.Endereco,
                        o.Cliente.Cidade,
                        o.Cliente.Estado,
                        o.Cliente.Cep
                    },
                    LoginPppoe = o.Assinatura?.LoginPppoe
                }));
            });

            api.MapGet("/ordens-servico/{id:int}", async (int id, IOrdemServicoService ordens) =>
            {
                var ordem = await ordens.ObterAsync(id);
                return ordem is null ? Results.NotFound() : Results.Ok(MapearOrdemServicoDetalhe(ordem));
            });

            api.MapPatch("/ordens-servico/{id:int}", async (
                int id,
                OrdemServicoStatusRequest request,
                IOrdemServicoService ordens) =>
            {
                if (!Enum.TryParse<StatusOrdemServico>(request.Status, ignoreCase: true, out var status))
                {
                    return Results.BadRequest(new { erro = "Status inválido." });
                }

                var ordem = await ordens.ObterAsync(id);
                if (ordem is null)
                {
                    return Results.NotFound();
                }

                ordem.Status = status;
                if (!string.IsNullOrWhiteSpace(request.TecnicoResponsavel))
                {
                    ordem.TecnicoResponsavel = request.TecnicoResponsavel.Trim();
                }

                if (request.DataAgendada.HasValue)
                {
                    ordem.DataAgendada = request.DataAgendada.Value.ToUniversalTime();
                }

                if (status == StatusOrdemServico.Concluida)
                {
                    ordem.DataConclusao = DateTime.UtcNow;
                    if (!string.IsNullOrWhiteSpace(request.ObservacaoConclusao))
                    {
                        ordem.ObservacaoConclusao = request.ObservacaoConclusao.Trim();
                    }
                }

                await ordens.SalvarAsync(ordem);
                return Results.Ok(new { mensagem = "O.S. atualizada.", ordemId = id, status = status.ToString() });
            });

            api.MapGet("/nfse", async (StatusNfse? status, INfseService nfse) =>
            {
                var notas = await nfse.ListarAsync(status);
                return Results.Ok(notas.Select(n => new
                {
                    n.Id,
                    n.FaturaId,
                    Cliente = n.Fatura.Assinatura.Cliente.Nome,
                    n.Fatura.Referencia,
                    n.Numero,
                    n.CodigoVerificacao,
                    n.Valor,
                    Status = n.Status.ToString(),
                    n.DataEmissao,
                    n.LinkPdf
                }));
            });

            api.MapGet("/nfse/fatura/{faturaId:int}", async (int faturaId, INfseService nfse) =>
            {
                var nota = await nfse.ObterPorFaturaAsync(faturaId);
                return nota is null ? Results.NotFound() : Results.Ok(nota);
            });

            return api;
        }

        private static object MapearOrdemServicoResumo(Models.OrdemServico ordem) =>
            new
            {
                ordem.Id,
                ordem.Titulo,
                Tipo = ordem.Tipo.ToString(),
                Status = ordem.Status.ToString(),
                ordem.TecnicoResponsavel,
                ordem.Endereco,
                ordem.DataAgendada,
                ordem.DataAbertura,
                Cliente = ordem.Cliente.Nome,
                ordem.ClienteId,
                ordem.ChamadoId
            };

        private static object MapearOrdemServicoDetalhe(Models.OrdemServico ordem) =>
            new
            {
                ordem.Id,
                ordem.Titulo,
                ordem.Descricao,
                Tipo = ordem.Tipo.ToString(),
                Status = ordem.Status.ToString(),
                ordem.TecnicoResponsavel,
                ordem.Endereco,
                ordem.DataAgendada,
                ordem.DataAbertura,
                ordem.DataConclusao,
                ordem.ObservacaoConclusao,
                ordem.ChamadoId,
                Cliente = new
                {
                    ordem.Cliente.Id,
                    ordem.Cliente.Nome,
                    ordem.Cliente.Telefone,
                    ordem.Cliente.Email,
                    ordem.Cliente.Endereco
                },
                Assinatura = ordem.Assinatura is null ? null : new
                {
                    ordem.Assinatura.Id,
                    ordem.Assinatura.LoginPppoe,
                    ordem.Assinatura.EnderecoInstalacao
                }
            };

        private static object MapearChamadoDetalhe(Models.Chamado chamado) =>
            new
            {
                chamado.Id,
                chamado.Assunto,
                Categoria = chamado.Categoria.ToString(),
                Prioridade = chamado.Prioridade.ToString(),
                Status = chamado.Status.ToString(),
                chamado.DataAbertura,
                chamado.DataAtualizacao,
                Cliente = new
                {
                    chamado.Cliente.Id,
                    chamado.Cliente.Nome,
                    chamado.Cliente.Telefone,
                    chamado.Cliente.Email,
                    chamado.Cliente.Endereco,
                    chamado.Cliente.Cidade,
                    chamado.Cliente.Estado
                },
                Mensagens = chamado.Mensagens.Select(m => new
                {
                    m.Id,
                    m.Conteudo,
                    AutorTipo = m.AutorTipo.ToString(),
                    m.AutorNome,
                    m.DataEnvio
                })
            };
    }
}

