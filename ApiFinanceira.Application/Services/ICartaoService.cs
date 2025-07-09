using ApiFinanceira.Application.DTOs.Requests;
using ApiFinanceira.Application.DTOs.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiFinanceira.Application.Services
{
    public interface ICartaoService
    {
        Task<CartaoResponse?> CreateCardAsync(Guid pessoaId, Guid contaId, CreateCartaoRequest request);
        Task<IEnumerable<CartaoResponse>> GetCardsByContaIdAsync(Guid pessoaId, Guid contaId);
        Task<PagedCardResponse> GetCardsByPessoaIdAsync(Guid pessoaId, int itemsPerPage = 10, int currentPage = 1);
    }
}
