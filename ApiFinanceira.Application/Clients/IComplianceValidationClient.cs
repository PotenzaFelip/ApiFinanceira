using ApiFinanceira.Application.DTOs.Requests;
using ApiFinanceira.Application.DTOs.Responses;
using ApiFinanceira.Application.ExternalServices;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks;

namespace ApiFinanceira.Application.Clients
{  
    public interface IComplianceValidationClient
    {
        [Post("/cpf/validate")]
        Task<ApiResponse<ComplianceResponse>> ValidateCpfAsync([Body] ComplianceRequest request);

        [Post("/cnpj/validate")]
        Task<ApiResponse<ComplianceResponse>> ValidateCnpjAsync([Body] ComplianceRequest request);
    }
}
