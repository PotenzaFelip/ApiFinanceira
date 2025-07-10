using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiFinanceira.Application.ExternalServices
{
    public class ComplianceResponse
    {
        public bool Success { get; set; }      
        public ComplianceData? Data { get; set; }
       
        public string? Error { get; set; }
        public string? Reason { get; set; }
        public string? Message { get; set; }
        public int Status { get; internal set; }
    }
   
    public class ComplianceData
    {
        public string Document { get; set; } = string.Empty;
        public int Status { get; set; }
        public string? Reason { get; set; }
    }
}
