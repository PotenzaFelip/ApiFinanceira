using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiFinanceira.Application.ExternalServices
{
    public class ComplianceApiSettings
    {
        public string BaseUrlCpf { get; set; } = string.Empty;
        public string BaseUrlCnpj { get; set; } = string.Empty;
    }
}
