using ApiFinanceira.Application.DTOs.Requests;
using ApiFinanceira.Application.DTOs.Responses;
using ApiFinanceira.Domain.Entities;
using ApiFinanceira.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiFinanceira.Application.Services
{
    public class TransacaoService : ITransacaoService
    {
        private readonly ITransacaoRepository _transacaoRepository;
        private readonly IContaRepository _contaRepository;

        public TransacaoService(ITransacaoRepository transacaoRepository, IContaRepository contaRepository)
        {
            _transacaoRepository = transacaoRepository;
            _contaRepository = contaRepository;
        }

        public async Task<TransacaoResponse?> CreateTransactionAsync(Guid pessoaId, Guid accountId, CreateTransacaoRequest request)
        {
            var conta = await _contaRepository.GetByIdAsync(accountId);
            if (conta == null || conta.PessoaId != pessoaId)
            {
                throw new UnauthorizedAccessException("Conta não encontrada ou você não tem permissão para acessá-la.");
            }

            if (request.Value < 0)
            {
                if (conta.Saldo + request.Value < 0)
                {
                    throw new InvalidOperationException("Saldo insuficiente para realizar esta transação de débito.");
                }
            }

            var novaTransacao = new Transacao
            {
                ContaId = accountId,
                Value = request.Value,
                Description = request.Description,
                Type = request.Value>=0 ? "credit" :"debit"
            };

            conta.Saldo += request.Value;

            await _transacaoRepository.AddAsync(novaTransacao);
            await _contaRepository.UpdateAsync(conta);
            await _transacaoRepository.SaveChangesAsync();

            return new TransacaoResponse
            {
                Id = novaTransacao.Id,
                Value = novaTransacao.Value,
                Description = novaTransacao.Description,
                CreatedAt = novaTransacao.CreatedAt,
                UpdatedAt = novaTransacao.UpdatedAt
            };
        }

        public async Task<TransacaoResponse?> CreateInternalTransferAsync(Guid pessoaId, Guid senderAccountId, CreateTransferenciaRequest request)
        {
            var senderAccount = await _contaRepository.GetByIdAsync(senderAccountId);
            if (senderAccount == null || senderAccount.PessoaId != pessoaId)
            {
                throw new UnauthorizedAccessException("Conta de origem não encontrada ou você não tem permissão para acessá-la.");
            }

            var receiverAccount = await _contaRepository.GetByIdAsync(request.ReceiverAccountId);
            if (receiverAccount == null)
            {
                throw new InvalidOperationException("Conta de destino não encontrada.");
            }

            if (senderAccountId == request.ReceiverAccountId)
            {
                throw new InvalidOperationException("Não é permitido transferir para a mesma conta.");
            }

            if (senderAccount.Saldo - request.Value < 0)
            {
                throw new InvalidOperationException("Saldo insuficiente na conta de origem para realizar a transferência.");
            }
            var debitoTransacao = new Transacao
            {
                ContaId = senderAccountId,
                Value = -request.Value,
                Description = $"Transferência para {receiverAccount.Account}: {request.Description}",
                Type = "debit"
            };

            var creditoTransacao = new Transacao
            {
                ContaId = request.ReceiverAccountId,
                Value = request.Value,
                Description = $"Transferência de {senderAccount.Account}: {request.Description}",
                Type = "credit"
            };

            senderAccount.Saldo -= request.Value;
            receiverAccount.Saldo += request.Value;


            await _transacaoRepository.AddAsync(debitoTransacao);
            await _transacaoRepository.AddAsync(creditoTransacao);
            await _contaRepository.UpdateAsync(senderAccount);
            await _contaRepository.UpdateAsync(receiverAccount);
            await _transacaoRepository.SaveChangesAsync();


            return new TransacaoResponse
            {
                Id = debitoTransacao.Id,
                Value = debitoTransacao.Value,
                Description = debitoTransacao.Description,
                CreatedAt = debitoTransacao.CreatedAt,
                UpdatedAt = debitoTransacao.UpdatedAt
            };
        }

        public async Task<PagedTransacaoResponse> GetTransactionsByContaIdAsync(
            Guid pessoaId,
            Guid accountId,
            int itemsPerPage = 10,
            int currentPage = 1,
            string? type = null)
        {    
            var conta = await _contaRepository.GetByIdAsync(accountId);
            if (conta == null || conta.PessoaId != pessoaId)
            {
                throw new UnauthorizedAccessException("Conta não encontrada ou você não tem permissão para acessá-la.");
            }

            if (itemsPerPage <= 0) itemsPerPage = 10;
            if (currentPage <= 0) currentPage = 1;

            var totalItems = await _transacaoRepository.CountTransactionsByContaIdAsync(accountId, type);

            var skip = (currentPage - 1) * itemsPerPage;
            if (skip < 0) skip = 0;

            var transacoes = await _transacaoRepository.GetPagedTransactionsByContaIdAsync(accountId, skip, itemsPerPage, type);

            var transacaoResponses = transacoes.Select(t => new TransacaoResponse
            {
                Id = t.Id,
                Value = t.Value,
                Description = t.Description,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            }).ToList();

            var paginationInfo = new PaginationInfo
            {
                ItemsPerPage = itemsPerPage,
                CurrentPage = currentPage,
                TotalItems = totalItems
            };

            return new PagedTransacaoResponse
            {
                Transactions = transacaoResponses,
                Pagination = paginationInfo
            };
        }
    }
}