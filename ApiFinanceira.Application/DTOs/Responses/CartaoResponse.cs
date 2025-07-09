using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiFinanceira.Application.DTOs.Responses
{
    public class CartaoResponse
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Number { get; set; } = string.Empty;
        public string Cvv { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
