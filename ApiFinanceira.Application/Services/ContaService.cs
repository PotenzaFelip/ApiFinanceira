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
    public class ContaService : IContaService
    {
        private readonly IContaRepository _contaRepository;
        private readonly IPessoaRepository _pessoaRepository;

        public ContaService(IContaRepository contaRepository, IPessoaRepository pessoaRepository)
        {
            _contaRepository = contaRepository;
            _pessoaRepository = pessoaRepository;
        }

        public async Task<ContaResponse?> CreateContaAsync(Guid pessoaId, CreateContaRequest request)
        {
            var pessoa = await _pessoaRepository.GetByIdAsync(pessoaId);
            if (pessoa == null)
            {
                return null;
            }

            var novaConta = new Conta
            {
                PessoaId = pessoaId,
                Branch = request.Branch,
                Account = request.Account
            };

            await _contaRepository.AddAsync(novaConta);
            await _contaRepository.SaveChangesAsync();

            return new ContaResponse
            {
                Id = novaConta.Id,
                Branch = novaConta.Branch,
                Account = novaConta.Account,
                CreatedAt = novaConta.CreatedAt,
                UpdatedAt = novaConta.UpdatedAt
            };
        }

        public async Task<IEnumerable<ContaResponse>> GetContasByPessoaIdAsync(Guid pessoaId)
        {
            var contas = await _contaRepository.GetByPessoaIdAsync(pessoaId);

            return contas.Select(c => new ContaResponse
            {
                Id = c.Id,
                Branch = c.Branch,
                Account = c.Account,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            }).ToList();
        }
    }
}
