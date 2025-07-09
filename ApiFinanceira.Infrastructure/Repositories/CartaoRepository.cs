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
using Microsoft.Extensions.Configuration;

namespace ApiFinanceira.Infrastructure.Repositories
{
    public class CartaoRepository : GenericRepository<Cartao>, ICartaoRepository
    {
        public CartaoRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Cartao?> GetByCardNumberAsync(string cardNumber)
        {
            return await _context.Cartoes.FirstOrDefaultAsync(c => c.Number == cardNumber);
        }

        public async Task<IEnumerable<Cartao>> GetPhysicalCardsByContaIdAsync(Guid contaId)
        {
            return await _context.Cartoes
                                 .Where(c => c.ContaId == contaId && c.Type == "physical")
                                 .ToListAsync();
        }

        public async Task<IEnumerable<Cartao>> GetByContaIdAsync(Guid contaId)
        {
            return await _context.Cartoes
                                 .Where(c => c.ContaId == contaId)
                                 .ToListAsync();
        }

        public async Task<IEnumerable<Cartao>> GetAllCardsByPessoaIdAsync(Guid pessoaId)
        {
            return await _context.Cartoes
                                 .Include(c => c.Conta)
                                 .Where(c => c.Conta != null && c.Conta.PessoaId == pessoaId)
                                 .OrderByDescending(c => c.CreatedAt)
                                 .ToListAsync();
        }

        public async Task<IEnumerable<Cartao>> GetPagedCardsByPessoaIdAsync(Guid pessoaId, int skip, int take)
        {
            return await _context.Cartoes
                                 .Include(c => c.Conta)
                                 .Where(c => c.Conta != null && c.Conta.PessoaId == pessoaId)
                                 .OrderByDescending(c => c.CreatedAt)
                                 .Skip(skip)
                                 .Take(take)
                                 .ToListAsync();
        }

        public async Task<int> CountCardsByPessoaIdAsync(Guid pessoaId)
        {
            return await _context.Cartoes
                                 .Include(c => c.Conta)
                                 .CountAsync(c => c.Conta != null && c.Conta.PessoaId == pessoaId);
        }
        public async Task<Cartao?> GetPhysicalCardByContaIdAsync(Guid contaId)
        {
            return await _context.Cartoes
                                 .Where(c => c.ContaId == contaId && c.Type.ToLower() == "physical")
                                 .FirstOrDefaultAsync();
        }

    }
}
