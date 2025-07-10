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
    namespace ApiFinanceira.Application.Services
    {
        public class CartaoService : ICartaoService
        {
            private readonly ICartaoRepository _cartaoRepository;
            private readonly IContaRepository _contaRepository;
            private readonly IPessoaRepository _pessoaRepository;

            public CartaoService(ICartaoRepository cartaoRepository, IContaRepository contaRepository, IPessoaRepository pessoaRepository)
            {
                _cartaoRepository = cartaoRepository;
                _contaRepository = contaRepository;
                _pessoaRepository = pessoaRepository;
            }

           
            public async Task<CartaoResponse?> CreateCardAsync(Guid pessoaId, Guid contaId, CreateCartaoRequest request)
            {
                var conta = await _contaRepository.GetByIdAsync(contaId);
                if (conta == null || conta.PessoaId != pessoaId)
                {
                    return null;
                }

                var cleanCardNumber = request.Number.Replace(" ", "");
                if (cleanCardNumber.Length != 16 || !long.TryParse(cleanCardNumber, out _))
                {
                    throw new ArgumentException("O número do cartão deve conter 16 dígitos.");
                }
                if (request.Cvv.Length != 3 || !int.TryParse(request.Cvv, out _))
                {
                    throw new ArgumentException("O CVV deve conter exatos 3 dígitos numéricos.");
                }

                if (request.Type.ToLower() == "physical")
                {
                    var existingPhysicalCard = await _cartaoRepository.GetPhysicalCardByContaIdAsync(contaId);
                    if (existingPhysicalCard != null)
                    {
                        throw new InvalidOperationException("Uma conta só pode ter um cartão físico.");
                    }
                }

                var existingCardWithNumber = await _cartaoRepository.GetByCardNumberAsync(cleanCardNumber);
                if (existingCardWithNumber != null)
                {
                    throw new InvalidOperationException("Este número de cartão já está em uso.");
                }

                var novoCartao = new Cartao
                {
                    ContaId = contaId,
                    Type = request.Type.ToLower(),
                    Number = cleanCardNumber,
                    Cvv = request.Cvv
                };

                await _cartaoRepository.AddAsync(novoCartao);
                await _cartaoRepository.SaveChangesAsync();

                return new CartaoResponse
                {
                    Id = novoCartao.Id,
                    Type = novoCartao.Type,
                    Number = MaskLastFourDigits(novoCartao.Number),
                    Cvv = novoCartao.Cvv,
                    CreatedAt = novoCartao.CreatedAt,
                    UpdatedAt = novoCartao.UpdatedAt
                };
            }

          
            public async Task<IEnumerable<CartaoResponse>> GetCardsByContaIdAsync(Guid pessoaId, Guid contaId)
            {
                var conta = await _contaRepository.GetByIdAsync(contaId);
                if (conta == null || conta.PessoaId != pessoaId)
                {
                    return Enumerable.Empty<CartaoResponse>();
                }

                var cartoes = await _cartaoRepository.GetByContaIdAsync(contaId);

                return cartoes.Select(c => new CartaoResponse
                {
                    Id = c.Id,
                    Type = c.Type,
                    Number = c.Number,
                    Cvv = c.Cvv,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                }).ToList();
            }

            
            public async Task<PagedCardResponse> GetCardsByPessoaIdAsync(Guid pessoaId, int itemsPerPage = 10, int currentPage = 1)
            {
               
                if (itemsPerPage <= 0) itemsPerPage = 10;
                if (currentPage <= 0) currentPage = 1;

                var totalItems = await _cartaoRepository.CountCardsByPessoaIdAsync(pessoaId);

               
                var skip = (currentPage - 1) * itemsPerPage;
                if (skip < 0) skip = 0;

                var cartoes = await _cartaoRepository.GetPagedCardsByPessoaIdAsync(pessoaId, skip, itemsPerPage);

                var cardResponses = cartoes.Select(c => new CartaoResponse
                {
                    Id = c.Id,
                    Type = c.Type,
                    Number = MaskLastFourDigits(c.Number),
                    Cvv = c.Cvv,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                }).ToList();

                var paginationInfo = new PaginationInfo
                {
                    ItemsPerPage = itemsPerPage,
                    CurrentPage = currentPage,
                    TotalItems = totalItems
                };

                return new PagedCardResponse
                {
                    Cards = cardResponses,
                    Pagination = paginationInfo
                };
            }

          
            public async Task<CartaoResponse?> GetCardByIdAndContaIdAndPessoaIdAsync(Guid cardId, Guid contaId, Guid pessoaId)
            {
                var cartao = await _cartaoRepository.GetByIdAsync(cardId);
                if (cartao == null || cartao.ContaId != contaId) return null;

                var conta = await _contaRepository.GetByIdAsync(contaId);
                if (conta == null || conta.PessoaId != pessoaId) return null;

                return new CartaoResponse
                {
                    Id = cartao.Id,
                    Type = cartao.Type,
                    Number = MaskLastFourDigits(cartao.Number),
                    Cvv = cartao.Cvv,
                    CreatedAt = cartao.CreatedAt,
                    UpdatedAt = cartao.UpdatedAt
                };
            }

          
            private string MaskLastFourDigits(string fullNumber)
            {
                if (string.IsNullOrEmpty(fullNumber) || fullNumber.Length < 4)
                {
                    return fullNumber;
                }
                return fullNumber.Substring(fullNumber.Length - 4);
            }
        }
    }
}