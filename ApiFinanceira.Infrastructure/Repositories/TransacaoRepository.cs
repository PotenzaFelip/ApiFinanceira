using ApiFinanceira.Domain.Entities;
using ApiFinanceira.Domain.Enums;
using ApiFinanceira.Domain.Interfaces;
using ApiFinanceira.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiFinanceira.Infrastructure.Repositories
{
    public class TransacaoRepository : GenericRepository<Transacao>, ITransacaoRepository
    {
        public TransacaoRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Transacao>> GetByContaIdAsync(Guid contaId, int itemsPerPage, int currentPage, TipoTransacao? tipo = null)
        {
            var query = _dbSet.Where(t => t.ContaId == contaId);

            if (tipo.HasValue)
            {
                query = query.Where(t => t.Tipo == tipo.Value);
            }

            return await query.OrderByDescending(t => t.CreatedAt)
                              .Skip((currentPage - 1) * itemsPerPage)
                              .Take(itemsPerPage)
                              .ToListAsync();
        }

        public async Task<int> CountTransactionsByContaIdAsync(Guid contaId, TipoTransacao? tipo = null)
        {
            var query = _dbSet.Where(t => t.ContaId == contaId);

            if (tipo.HasValue)
            {
                query = query.Where(t => t.Tipo == tipo.Value);
            }

            return await query.CountAsync();
        }

        public async Task<Transacao?> GetTransactionByIdAndContaIdAsync(Guid transactionId, Guid contaId)
        {
            return await _dbSet.FirstOrDefaultAsync(t => t.Id == transactionId && t.ContaId == contaId);
        }
    }
}
