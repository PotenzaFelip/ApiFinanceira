using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiFinanceira.Application.DTOs.Requests
{
    public class CreateCartaoRequest
    {
        [Required(ErrorMessage = "O tipo do cartão é obrigatório.")]
        [RegularExpression("^(physical|virtual)$", ErrorMessage = "O tipo do cartão deve ser 'physical' ou 'virtual'.")]
        public string Type { get; set; } = string.Empty;

        [Required(ErrorMessage = "O número do cartão é obrigatório.")]
       // [CreditCard(ErrorMessage = "O número do cartão não é válido.")]
        [StringLength(19, MinimumLength = 16, ErrorMessage = "O número do cartão deve ter entre 16 e 19 dígitos (incluindo espaços).")]
        public string Number { get; set; } = string.Empty;

        [Required(ErrorMessage = "O CVV é obrigatório.")]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "O CVV deve conter exatos 3 dígitos.")]
        [RegularExpression(@"^\d{3}$", ErrorMessage = "O CVV deve conter apenas dígitos numéricos.")]
        public string Cvv { get; set; } = string.Empty;
    }
}
