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
    public class CartaoRepository : GenericRepository<Cartao>, ICartaoRepository
    {
        public CartaoRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Cartao>> GetByContaIdAsync(Guid contaId)
        {
            return await _dbSet.Where(c => c.ContaId == contaId).ToListAsync();
        }

        public async Task<bool> HasPhysicalCardByContaIdAsync(Guid contaId)
        {
            return await _dbSet.AnyAsync(c => c.ContaId == contaId && c.Type == TipoCartao.Physical);
        }

        public async Task<IEnumerable<Cartao>> GetCardsByPessoaIdAsync(Guid pessoaId, int itemsPerPage, int currentPage, bool maskNumber = true)
        {
            var query = _dbSet.Where(c => c.Conta.PessoaId == pessoaId)
                              .OrderByDescending(c => c.CreatedAt)
                              .Skip((currentPage - 1) * itemsPerPage)
                              .Take(itemsPerPage)
                              .AsQueryable();

            var cards = await query.ToListAsync();

            if (maskNumber)
            {
                foreach (var card in cards)
                {             
                    if (card.NumeroCompleto != null && card.NumeroCompleto.Length > 4)
                    {
                        card.NumeroCompleto = card.NumeroCompleto.Substring(card.NumeroCompleto.Length - 4);
                    }
                }
            }

            return cards;
        }

        public async Task<int> CountCardsByPessoaIdAsync(Guid pessoaId)
        {
            return await _dbSet.CountAsync(c => c.Conta.PessoaId == pessoaId);
        }
    }
}
