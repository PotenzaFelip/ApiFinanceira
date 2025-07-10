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
   public interface IComplianceAuthClient
    {
        [Post("/auth/code")] 
        Task<ApiResponse<ComplianceAuthCodeResponse>> GetAuthCodeAsync([Body] ComplianceAuthCodeRequest request);

        [Post("/auth/token")]
        Task<ApiResponse<ComplianceTokenResponse>> GetAccessTokenAsync([Body] ComplianceTokenRequest request);
    }
}
