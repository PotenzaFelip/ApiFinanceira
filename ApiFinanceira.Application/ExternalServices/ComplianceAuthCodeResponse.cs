using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiFinanceira.Application.ExternalServices
{
    public class ComplianceAuthCodeResponse
    {
        public bool Success { get; set; }
        public AuthCodeData? Data { get; set; }
    }

    public class AuthCodeData
    {
        public string UserId { get; set; } = string.Empty;
        public string AuthCode { get; set; } = string.Empty;
    }
}
