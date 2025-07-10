using ApiFinanceira.Application.DTOs.Requests;
using ApiFinanceira.Application.DTOs.Responses;
using ApiFinanceira.Domain.Entities;
using ApiFinanceira.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
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
                Type = novaTransacao.Type,
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
                Description = $"Transferência para {receiverAccount.Id}: {request.Description}",
                Type = "debit"
            };

            var creditoTransacao = new Transacao
            {
                ContaId = request.ReceiverAccountId,
                Value = request.Value,
                Description = $"Transferência de {senderAccount.Id}: {request.Description}",
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
                Type = t.Type,
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
        public async Task<TransacaoResponse?> RevertTransactionAsync(Guid pessoaId, Guid accountId, Guid transactionId, string description)
        {
            var contaQuePediuReversao = await _contaRepository.GetByIdAsync(accountId);
            if (contaQuePediuReversao == null || contaQuePediuReversao.PessoaId != pessoaId)
            {
                throw new UnauthorizedAccessException("Conta não encontrada ou você não tem permissão para acessá-la.");
            }

            var originalTransaction = await _transacaoRepository.GetByIdAsync(transactionId);
            if (originalTransaction == null || originalTransaction.ContaId != accountId)
            {
                throw new InvalidOperationException("Transação não encontrada para a conta especificada.");
            }

            if (originalTransaction.IsReverted)
            {
                throw new InvalidOperationException("Esta transação já foi revertida.");
            }
            if (originalTransaction.OriginalTransactionId.HasValue)
            {
                throw new InvalidOperationException("Não é permitido reverter uma transação que já é uma reversão.");
            }

            var reversalTransactions = new List<Transacao>();
            var accountsToUpdate = new List<Conta>();
            bool isTransferTransaction = originalTransaction.Description.Contains("Transferência para ") ||
                                         originalTransaction.Description.Contains("Transferência de");

            if (isTransferTransaction)
            {                
                Guid? partnerAccountId = null;
                
                var descriptionParts = originalTransaction.Description.Split(new[] { " para ", " de " }, StringSplitOptions.RemoveEmptyEntries);

                if (descriptionParts.Length >= 2)
                {                   
                    var accountIdPart = descriptionParts[1];                    
                    if (accountIdPart.Contains(":"))
                    {
                        accountIdPart = accountIdPart.Split(':')[0].Trim();
                    }

                    if (Guid.TryParse(accountIdPart, out Guid parsedPartnerAccountId))
                    {
                        partnerAccountId = parsedPartnerAccountId;
                    }
                }

                if (!partnerAccountId.HasValue)
                {
                    throw new InvalidOperationException("Não foi possível extrair o ID da conta parceira da descrição da transação original para reversão. A reversão manual de ambas as partes pode ser necessária.");
                }
               
                var relatedTransaction = await _transacaoRepository.GetQueryable()
                    .Where(t =>
                        t.ContaId == partnerAccountId.Value &&
                        t.Value == -originalTransaction.Value &&
                        t.OriginalTransactionId == null &&
                        t.IsReverted == false &&
                                                
                        t.CreatedAt >= originalTransaction.CreatedAt.AddSeconds(-5) &&
                        t.CreatedAt <= originalTransaction.CreatedAt.AddSeconds(5)
                    )
                    .OrderByDescending(t => t.CreatedAt)
                    .FirstOrDefaultAsync();

                if (relatedTransaction == null)
                {
                    throw new InvalidOperationException("Não foi possível encontrar a transação parceira correspondente para a reversão da transferência interna. A reversão manual de ambas as partes pode ser necessária ou implementar um ID de relacionamento.");
                }

                var contaParceira = await _contaRepository.GetByIdAsync(relatedTransaction.ContaId);
                if (contaParceira == null)
                {
                    throw new InvalidOperationException("Conta parceira da transferência não encontrada. Contate o suporte.");
                }
                
                decimal reversalValueOriginal = -originalTransaction.Value;
                string reversalTypeOriginal = originalTransaction.Type.Contains("in") ? "debit_reversal_transfer" : "credit_reversal_transfer";

                if (reversalValueOriginal < 0 && (contaQuePediuReversao.Saldo + reversalValueOriginal) < 0)
                {
                    throw new InvalidOperationException($"Saldo insuficiente na conta {contaQuePediuReversao.Account} para reverter a transação de {originalTransaction.Type}.");
                }
                contaQuePediuReversao.Saldo += reversalValueOriginal;
                accountsToUpdate.Add(contaQuePediuReversao);

                var reversalOriginal = new Transacao
                {
                    ContaId = contaQuePediuReversao.Id,
                    Value = reversalValueOriginal,
                    Description = $"Reversão da {originalTransaction.Type} original ({originalTransaction.Id}): {description}",
                    Type = reversalTypeOriginal,
                    OriginalTransactionId = originalTransaction.Id
                };
                reversalTransactions.Add(reversalOriginal);

                decimal reversalValueRelated = -relatedTransaction.Value;
                string reversalTypeRelated = relatedTransaction.Type.Contains("in") ? "debit_reversal_transfer" : "credit_reversal_transfer";

                if (reversalValueRelated < 0 && (contaParceira.Saldo + reversalValueRelated) < 0)
                {
                    throw new InvalidOperationException($"Saldo insuficiente na conta {contaParceira.Account} para reverter a transação parceira.");
                }
                contaParceira.Saldo += reversalValueRelated;
                accountsToUpdate.Add(contaParceira);

                var reversalRelated = new Transacao
                {
                    ContaId = contaParceira.Id,
                    Value = reversalValueRelated,
                    Description = $"Reversão da {relatedTransaction.Type} parceira ({relatedTransaction.Id}): {description}",
                    Type = reversalTypeRelated,
                    OriginalTransactionId = relatedTransaction.Id
                };
                reversalTransactions.Add(reversalRelated);

                originalTransaction.IsReverted = true;
                relatedTransaction.IsReverted = true;
                await _transacaoRepository.UpdateAsync(relatedTransaction);
            }
            else
            {
                decimal reversalValue;
                string reversalType;

                reversalValue = -originalTransaction.Value;

                if (originalTransaction.Type == "credit")
                {
                    reversalType = "debit_reversal";
                }
                else if (originalTransaction.Type == "debit")
                {
                    reversalType = "credit_reversal";
                }
                else
                {
                    throw new InvalidOperationException("Tipo de transação original desconhecido para reversão.");
                }

                if (reversalValue < 0 && (contaQuePediuReversao.Saldo + reversalValue) < 0)
                {
                    throw new InvalidOperationException("Saldo insuficiente para realizar a reversão desta transação.");
                }

                var reversalSingle = new Transacao
                {
                    ContaId = accountId,
                    Value = reversalValue,
                    Description = description,
                    Type = reversalType,
                    OriginalTransactionId = originalTransaction.Id
                };
                reversalTransactions.Add(reversalSingle);

                contaQuePediuReversao.Saldo += reversalValue;
                accountsToUpdate.Add(contaQuePediuReversao);
            }

            originalTransaction.IsReverted = true;

            foreach (var transacao in reversalTransactions)
            {
                await _transacaoRepository.AddAsync(transacao);
            }
            await _transacaoRepository.UpdateAsync(originalTransaction);
            foreach (var contaAtualizar in accountsToUpdate.DistinctBy(c => c.Id))
            {
                await _contaRepository.UpdateAsync(contaAtualizar);
            }
            await _transacaoRepository.SaveChangesAsync();

            return new TransacaoResponse
            {
                Id = reversalTransactions.First().Id,
                Value = reversalTransactions.First().Value,
                Description = reversalTransactions.First().Description,
                CreatedAt = reversalTransactions.First().CreatedAt,
                UpdatedAt = reversalTransactions.First().UpdatedAt
            };
        }

    }
}