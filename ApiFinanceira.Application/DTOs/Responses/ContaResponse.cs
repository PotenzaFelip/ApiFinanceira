using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiFinanceira.Application.DTOs.Responses
{
    public class ContaResponse
    {
        public Guid Id { get; set; }
        public string Branch { get; set; } = string.Empty;
        public string Account { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
