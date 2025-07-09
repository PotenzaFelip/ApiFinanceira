using ApiFinanceira.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiFinanceira.Domain.Entities
{
    public class Transacao
    {
        public Guid Id { get; set; }
        public Guid ContaId { get; set; }
        public decimal Valor { get; set; }
        public string Descricao { get; set; }
        public TipoTransacao Tipo { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool Revertida { get; set; } = false;
        public Guid? TransacaoOriginalId { get; set; }
        public Transacao? TransacaoOriginal { get; set; }

        public Guid? ReceiverAccountId { get; set; }
        public Conta? ReceiverAccount { get; set; }

        public Conta Conta { get; set; }

        public Transacao()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}