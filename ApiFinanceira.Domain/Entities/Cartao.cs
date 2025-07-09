using ApiFinanceira.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiFinanceira.Domain.Entities
{
    public class Cartao : BaseEntity
    {
        public string Type { get; set; } = string.Empty;
        public string Number { get; set; } = string.Empty;
        public string Cvv { get; set; } = string.Empty;
        public Guid ContaId { get; set; }
        public Conta? Conta { get; set; }

    }
}
