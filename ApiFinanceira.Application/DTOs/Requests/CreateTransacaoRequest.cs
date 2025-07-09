using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiFinanceira.Application.DTOs.Requests
{
    public class CreateTransacaoRequest
    {
        [Required(ErrorMessage = "O valor da transação é obrigatório.")]
        [Range(double.MinValue, double.MaxValue, ErrorMessage = "Valor inválido.")]
        public decimal Value { get; set; }

        [Required(ErrorMessage = "A descrição da transação é obrigatória.")]
        [StringLength(200, ErrorMessage = "A descrição não pode exceder 200 caracteres.")]
        public string Description { get; set; } = string.Empty;
    }
}
