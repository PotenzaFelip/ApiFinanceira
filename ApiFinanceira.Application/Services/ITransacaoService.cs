using ApiFinanceira.Application.DTOs.Requests;
using ApiFinanceira.Application.DTOs.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiFinanceira.Application.Services
{
    public interface ITransacaoService
    {
        Task<TransacaoResponse?> CreateTransactionAsync(Guid pessoaId, Guid accountId, CreateTransacaoRequest request);
        Task<TransacaoResponse?> CreateInternalTransferAsync(Guid pessoaId, Guid senderAccountId, CreateTransferenciaRequest request);
        Task<PagedTransacaoResponse> GetTransactionsByContaIdAsync(
            Guid pessoaId,
            Guid accountId,
            int itemsPerPage = 10,
            int currentPage = 1,
            string? type = null);
        Task<TransacaoResponse?> RevertTransactionAsync(Guid pessoaId, Guid accountId, Guid transactionId, string description);
    }
}
