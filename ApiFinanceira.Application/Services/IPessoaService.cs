using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApiFinanceira.Application.DTOs.Requests;
using ApiFinanceira.Application.DTOs.Responses;
using System.Threading.Tasks;

namespace ApiFinanceira.Application.Services
{
    public interface IPessoaService
    {
        Task<PessoaResponse?> RegisterPessoaAsync(CreatePessoaRequest request);
        Task<PessoaResponse?> GetPessoaByIdAsync(Guid id);
        Task<PessoaResponse?> GetPessoaByDocumentAsync(string document);
    }
}