using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiFinanceira.Application.ExternalServices
{
    public class ComplianceTokenRequest
    {
        public string AuthCode { get; set; } = string.Empty;
    }
}
