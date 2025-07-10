using ApiFinanceira.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiFinanceira.Domain.Entities
{
    public class Transacao : BaseEntity
    {
        public Guid ContaId { get; set; }

        public decimal Value { get; set; }
        public string Description { get; set; } = string.Empty;

        public string Type { get; set; }
        public bool IsReverted { get; set; } = false;
        public Guid? OriginalTransactionId { get; set; }
        public Transacao? OriginalTransaction { get; set; }
    }
}