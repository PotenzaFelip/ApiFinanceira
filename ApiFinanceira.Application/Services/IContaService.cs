using ApiFinanceira.Application.DTOs.Requests;
using ApiFinanceira.Application.DTOs.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiFinanceira.Application.Services
{
    public interface IContaService
    {
        Task<ContaResponse?> CreateContaAsync(Guid pessoaId, CreateContaRequest request);
        Task<IEnumerable<ContaResponse>> GetContasByPessoaIdAsync(Guid pessoaId);
        Task<decimal> GetAccountBalanceAsync(Guid pessoaId, Guid accountId);
    }
}
