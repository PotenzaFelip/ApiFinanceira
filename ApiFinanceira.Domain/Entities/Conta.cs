using ApiFinanceira.Domain.Entities;
using ApiFinanceira.Domain.Enums;
using System;
using System.Collections.Generic;

namespace ApiFinanceira.Domain.Entities
{
    public class Conta
    {
        

        public Guid Id { get; set; }
        public Guid PessoaId { get; set; }
        public string Branch { get; set; }
        public string Account { get; set; }
        public decimal Saldo { get; set; }
        public decimal Limite { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Pessoa Pessoa { get; set; }
        public ICollection<Cartao> Cartoes { get; set; } = new List<Cartao>();
        public ICollection<Transacao> Transacoes { get; set; } = new List<Transacao>();

        public Conta()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow; 
            Saldo = 0;
        }
    }
}