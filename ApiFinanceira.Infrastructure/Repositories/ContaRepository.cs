using ApiFinanceira.Domain.Entities;
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
    public class ContaRepository : GenericRepository<Conta>, IContaRepository
    {
        public ContaRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Conta?> GetByAccountAndBranchAsync(string branch, string account)
        {
            return await _dbSet.FirstOrDefaultAsync(c => c.Branch == branch && c.Account == account);
        }

        public async Task<IEnumerable<Conta>> GetByPessoaIdAsync(Guid pessoaId)
        {
            return await _dbSet.Where(c => c.PessoaId == pessoaId).ToListAsync();
        }

        public async Task<Conta?> GetByPessoaIdAndAccountIdAsync(Guid pessoaId, Guid contaId)
        {
            return await _dbSet.FirstOrDefaultAsync(c => c.PessoaId == pessoaId && c.Id == contaId);
        }

        public Task<Conta?> GetByAccountAsync(string account)
        {
            throw new NotImplementedException();
        }
    }
}