using ApiFinanceira.Application.DTOs.Requests;
using ApiFinanceira.Application.DTOs.Responses;
using ApiFinanceira.Domain.Entities;
using ApiFinanceira.Domain.Interfaces;
using ApiFinanceira.Application.ExternalServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiFinanceira.Application.Services
{
    public class PessoaService : IPessoaService
    {
        private readonly IPessoaRepository _pessoaRepository;
        private readonly IComplianceService _complianceService;

        public PessoaService(IPessoaRepository pessoaRepository, IComplianceService complService)
        {
            _pessoaRepository = pessoaRepository;
            _complianceService = complService;
        }

        public async Task<PessoaResponse?> RegisterPessoaAsync(CreatePessoaRequest request)
        {          
            var cleanDocument = request.Document.Replace(".", "").Replace("-", "").Replace("/", "");
          
            var existingPessoa = await _pessoaRepository.GetByDocumentAsync(cleanDocument);
            if (existingPessoa != null)
            {               
                return null;
            }

            string documentType;
            if (cleanDocument.Length == 11)
            {
                documentType = "cpf";
            }
            else if (cleanDocument.Length == 14)
            {
                documentType = "cnpj";
            }
            else
            {
                throw new ArgumentException("Documento inválido. Deve ser um CPF (11 dígitos) ou CNPJ (14 dígitos).", nameof(request.Document));
            }
           
            try
            {
                var complianceResponse = await _complianceService.ValidaDocumentComplianceAsync(cleanDocument, documentType);

                if (complianceResponse == null || complianceResponse.Data.Status != 1)
                {
                    throw new InvalidOperationException($"Documento não aprovado pela API de Compliance. Status retornado: {complianceResponse?.Status ?? 0}. Mensagem: {complianceResponse?.Message ?? "N/A"}");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Falha na validação do documento na API de Compliance: {ex.Message}", ex);
            }


            var pessoa = new Pessoa
            {
                Nome = request.Name,
                Documento = cleanDocument,
                SenhaHash = HashPassword(request.Password),                
            };
          
            await _pessoaRepository.AddAsync(pessoa);
            await _pessoaRepository.SaveChangesAsync();

            return new PessoaResponse
            {
                Id = pessoa.Id,
                Name = pessoa.Nome,
                Document = pessoa.Documento,
                CreatedAt = pessoa.CreatedAt,
                UpdatedAt = pessoa.UpdatedAt
            };
        }

        public async Task<PessoaResponse?> GetPessoaByIdAsync(Guid id)
        {
            var pessoa = await _pessoaRepository.GetByIdAsync(id);
            if (pessoa == null)
            {
                return null;
            }

            return new PessoaResponse
            {
                Id = pessoa.Id,
                Name = pessoa.Nome,
                Document = pessoa.Documento,
                CreatedAt = pessoa.CreatedAt,
                UpdatedAt = pessoa.UpdatedAt
            };
        }

        public async Task<PessoaResponse?> GetPessoaByDocumentAsync(string document)
        {
            var cleanDocument = document.Replace(".", "").Replace("-", "").Replace("/", "");
            var pessoa = await _pessoaRepository.GetByDocumentAsync(cleanDocument);
            if (pessoa == null)
            {
                return null;
            }

            return new PessoaResponse
            {
                Id = pessoa.Id,
                Name = pessoa.Nome,
                Document = pessoa.Documento,
                CreatedAt = pessoa.CreatedAt,
                UpdatedAt = pessoa.UpdatedAt
            };
        }

        private string HashPassword(string password)
        {
            return Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(password)));
        }
    }
}
