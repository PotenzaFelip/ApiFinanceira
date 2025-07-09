using ApiFinanceira.Domain.Entities;
using ApiFinanceira.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiFinanceira.Domain.Interfaces
{
    public interface ITransacaoRepository : IGenericRepository<Transacao>
    {
        Task<IEnumerable<Transacao>> GetByContaIdAsync(Guid contaId, int itemsPerPage, int currentPage, TipoTransacao? tipo = null);
        Task<int> CountTransactionsByContaIdAsync(Guid contaId, TipoTransacao? tipo = null);
        Task<Transacao?> GetTransactionByIdAndContaIdAsync(Guid transactionId, Guid contaId);
    }
}
