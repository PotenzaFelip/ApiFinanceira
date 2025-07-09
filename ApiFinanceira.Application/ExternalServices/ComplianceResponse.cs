using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiFinanceira.Application.ExternalServices
{
    public class ComplianceResponse
    {
        public string? Document { get; set; }
        public int Status { get; set; } 
        public string? Reason { get; set; }
        public string? Message { get; set; }
        public string? Error { get; set; }
    }
}
