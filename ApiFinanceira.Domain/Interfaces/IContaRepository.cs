using ApiFinanceira.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiFinanceira.Domain.Interfaces
{
    public interface IContaRepository : IGenericRepository<Conta>
    {
        Task<Conta?> GetByAccountAndBranchAsync(string branch, string account);
        Task<IEnumerable<Conta>> GetByPessoaIdAsync(Guid pessoaId);
        Task<Conta?> GetByPessoaIdAndAccountIdAsync(Guid pessoaId, Guid contaId);
        Task UpdateAsync(Conta entity);
    }
}