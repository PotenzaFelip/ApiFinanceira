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
    public class TransacaoRepository : ITransacaoRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<Transacao> _dbSet;

        public TransacaoRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<Transacao>(); 
        }

        public async Task<Transacao?> GetByIdAsync(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<IEnumerable<Transacao>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }
        public IQueryable<Transacao> GetQueryable()
        {
            return _context.Transacoes.AsQueryable();
        }

        public async Task AddAsync(Transacao entity)
        {
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
            await _dbSet.AddAsync(entity);
        }

        public async Task UpdateAsync(Transacao entity)
        {
            entity.UpdatedAt = DateTime.UtcNow;
            _dbSet.Update(entity);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
            }
            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Transacao>> GetPagedTransactionsByContaIdAsync(Guid contaId, int skip, int take, string? type = null)
        {
            var query = _dbSet.Where(t => t.ContaId == contaId);

            if (!string.IsNullOrEmpty(type))
            {
                if (type.Equals("credit", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(t => t.Value > 0);
                }
                else if (type.Equals("debit", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(t => t.Value < 0);
                }
            }

            return await query
                        .OrderByDescending(t => t.CreatedAt) // Ordem decrescente
                        .Skip(skip)
                        .Take(take)
                        .ToListAsync();
        }

        public async Task<int> CountTransactionsByContaIdAsync(Guid contaId, string? type = null)
        {
            var query = _dbSet.Where(t => t.ContaId == contaId);

            if (!string.IsNullOrEmpty(type))
            {
                if (type.Equals("credit", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(t => t.Value > 0);
                }
                else if (type.Equals("debit", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(t => t.Value < 0);
                }
            }

            return await query.CountAsync();
        }
    }
}
