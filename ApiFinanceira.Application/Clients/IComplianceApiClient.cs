using ApiFinanceira.Application.DTOs.Requests;
using ApiFinanceira.Application.DTOs.Responses;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks;

namespace ApiFinanceira.Application.Clients
{
       public interface IComplianceApiClient
    {
        
        [Post("/api/validate-document")]
        Task<ComplianceValidationResponse> ValidateDocumentAsync([Body] DocumentValidationRequest request);

    }
}
