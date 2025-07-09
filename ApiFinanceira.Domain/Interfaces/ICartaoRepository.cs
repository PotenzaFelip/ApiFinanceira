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
        Task<IEnumerable<Cartao>> GetByContaIdAsync(Guid contaId);
        Task<bool> HasPhysicalCardByContaIdAsync(Guid contaId);
        Task<IEnumerable<Cartao>> GetCardsByPessoaIdAsync(Guid pessoaId, int itemsPerPage, int currentPage, bool maskNumber = true);
        Task<int> CountCardsByPessoaIdAsync(Guid pessoaId);
    }
}