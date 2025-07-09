using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiFinanceira.Application.DTOs.Requests
{
    public class CreateTransferenciaRequest
    {
        [Required(ErrorMessage = "O ID da conta recebedora é obrigatório.")]
        public Guid ReceiverAccountId { get; set; }

        [Required(ErrorMessage = "O valor da transferência é obrigatório.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "O valor da transferência deve ser positivo.")]
        public decimal Value { get; set; }

        [Required(ErrorMessage = "A descrição da transferência é obrigatória.")]
        [StringLength(200, ErrorMessage = "A descrição não pode exceder 200 caracteres.")]
        public string Description { get; set; } = string.Empty;
    }
}
