using ApiFinanceira.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiFinanceira.Domain.Entities
{
    public class Cartao
    {
        public Guid Id { get; set; }
        public Guid ContaId { get; set; }
        public TipoCartao Type { get; set; }
        public string NumeroCompleto { get; set; }
        public string UltimosQuatroDigitos { get; set; }
        public string CVVHash { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Conta Conta { get; set; }

        public Cartao()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
