using Microsoft.EntityFrameworkCore;
using horizonisp.Context;
using horizonisp.Models;
using horizonisp.Models.Enums;

namespace horizonisp.Services
{
    public interface IChamadoService
    {
        Task<IReadOnlyList<Chamado>> ListarPorClienteAsync(int clienteId);
        Task<IReadOnlyList<Chamado>> ListarTodosAsync(StatusChamado? status = null);
        Task<Chamado?> ObterAsync(int chamadoId, int? clienteId = null);
        Task<Chamado> AbrirAsync(int clienteId, string assunto, CategoriaChamado categoria, PrioridadeChamado prioridade, string mensagem, string autorNome);
        Task<ChamadoMensagem> ResponderAsync(int chamadoId, string conteudo, AutorMensagemChamado autorTipo, string autorNome, int? clienteId = null);
        Task AtualizarStatusAsync(int chamadoId, StatusChamado status);
        Task<int> ContarAbertosAsync();
        Task<IReadOnlyList<Chamado>> ListarParaTecnicoAsync();
    }

    public class ChamadoService(AppDbContext db) : IChamadoService
    {
        public async Task<IReadOnlyList<Chamado>> ListarPorClienteAsync(int clienteId)
        {
            return await db.Chamados
                .Include(c => c.Mensagens)
                .Where(c => c.ClienteId == clienteId)
                .OrderByDescending(c => c.DataAtualizacao)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Chamado>> ListarTodosAsync(StatusChamado? status = null)
        {
            var query = db.Chamados
                .Include(c => c.Cliente)
                .Include(c => c.Mensagens)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(c => c.Status == status.Value);
            }

            return await query
                .OrderByDescending(c => c.DataAtualizacao)
                .ToListAsync();
        }

        public async Task<Chamado?> ObterAsync(int chamadoId, int? clienteId = null)
        {
            var query = db.Chamados
                .Include(c => c.Cliente)
                .Include(c => c.Mensagens.OrderBy(m => m.DataEnvio))
                .Where(c => c.Id == chamadoId);

            if (clienteId.HasValue)
            {
                query = query.Where(c => c.ClienteId == clienteId.Value);
            }

            return await query.FirstOrDefaultAsync();
        }

        public async Task<Chamado> AbrirAsync(
            int clienteId,
            string assunto,
            CategoriaChamado categoria,
            PrioridadeChamado prioridade,
            string mensagem,
            string autorNome)
        {
            var chamado = new Chamado
            {
                ClienteId = clienteId,
                Assunto = assunto.Trim(),
                Categoria = categoria,
                Prioridade = prioridade,
                Status = StatusChamado.Aberto,
                DataAbertura = DateTime.UtcNow,
                DataAtualizacao = DateTime.UtcNow
            };

            chamado.Mensagens.Add(new ChamadoMensagem
            {
                AutorTipo = AutorMensagemChamado.Cliente,
                AutorNome = autorNome,
                Conteudo = mensagem.Trim(),
                DataEnvio = DateTime.UtcNow
            });

            db.Chamados.Add(chamado);
            await db.SaveChangesAsync();
            return chamado;
        }

        public async Task<ChamadoMensagem> ResponderAsync(
            int chamadoId,
            string conteudo,
            AutorMensagemChamado autorTipo,
            string autorNome,
            int? clienteId = null)
        {
            var chamado = await ObterAsync(chamadoId, clienteId)
                ?? throw new InvalidOperationException("Chamado não encontrado.");

            if (chamado.Status is StatusChamado.Fechado)
            {
                throw new InvalidOperationException("Chamado encerrado.");
            }

            var mensagem = new ChamadoMensagem
            {
                ChamadoId = chamadoId,
                AutorTipo = autorTipo,
                AutorNome = autorNome,
                Conteudo = conteudo.Trim(),
                DataEnvio = DateTime.UtcNow
            };

            chamado.DataAtualizacao = DateTime.UtcNow;

            if (autorTipo == AutorMensagemChamado.Operador && chamado.Status == StatusChamado.Aberto)
            {
                chamado.Status = StatusChamado.EmAndamento;
            }

            db.ChamadoMensagens.Add(mensagem);
            await db.SaveChangesAsync();
            return mensagem;
        }

        public async Task AtualizarStatusAsync(int chamadoId, StatusChamado status)
        {
            var chamado = await db.Chamados.FindAsync(chamadoId)
                ?? throw new InvalidOperationException("Chamado não encontrado.");

            chamado.Status = status;
            chamado.DataAtualizacao = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }

        public Task<int> ContarAbertosAsync()
        {
            return db.Chamados.CountAsync(c =>
                c.Status == StatusChamado.Aberto || c.Status == StatusChamado.EmAndamento);
        }

        public async Task<IReadOnlyList<Chamado>> ListarParaTecnicoAsync()
        {
            return await db.Chamados
                .Include(c => c.Cliente)
                .Include(c => c.Mensagens)
                .Where(c =>
                    (c.Categoria == CategoriaChamado.Tecnico || c.Categoria == CategoriaChamado.Outros)
                    && (c.Status == StatusChamado.Aberto || c.Status == StatusChamado.EmAndamento))
                .OrderByDescending(c => c.Prioridade)
                .ThenByDescending(c => c.DataAtualizacao)
                .ToListAsync();
        }
    }
}
