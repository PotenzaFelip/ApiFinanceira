using ApiFinanceira.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiFinanceira.Domain.Interfaces
{
    public interface ICartaoRepository : IGenericRepository<Cartao>
    {
        Task<Cartao?> GetByCardNumberAsync(string cardNumber);
        Task<IEnumerable<Cartao>> GetPhysicalCardsByContaIdAsync(Guid contaId);
        Task<IEnumerable<Cartao>> GetByContaIdAsync(Guid contaId);
        Task<IEnumerable<Cartao>> GetAllCardsByPessoaIdAsync(Guid pessoaId);
        Task<IEnumerable<Cartao>> GetPagedCardsByPessoaIdAsync(Guid pessoaId, int skip, int take);
        Task<int> CountCardsByPessoaIdAsync(Guid pessoaId);
        Task<Cartao?> GetPhysicalCardByContaIdAsync(Guid contaId);
    }
}