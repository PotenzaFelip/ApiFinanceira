using ApiFinanceira.Domain.Entities;
using ApiFinanceira.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiFinanceira.Domain.Interfaces
{
    public interface ITransacaoRepository
    {
        Task<Transacao?> GetByIdAsync(Guid id);
        Task<IEnumerable<Transacao>> GetAllAsync();
        Task AddAsync(Transacao entity);
        Task UpdateAsync(Transacao entity);
        Task DeleteAsync(Guid id);
        Task SaveChangesAsync();

        IQueryable<Transacao> GetQueryable();

        Task<IEnumerable<Transacao>> GetPagedTransactionsByContaIdAsync(Guid contaId, int skip, int take, string? type = null);
        Task<int> CountTransactionsByContaIdAsync(Guid contaId, string? type = null);
    }
}
