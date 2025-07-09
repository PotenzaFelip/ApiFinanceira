using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiFinanceira.Application.DTOs.Requests
{
    public class CreateContaRequest
    {
        [Required(ErrorMessage = "A agência é obrigatória.")]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "A agência deve possuir exatos 3 dígitos.")]
        [RegularExpression(@"^\d{3}$", ErrorMessage = "A agência deve conter apenas dígitos numéricos.")]
        public string Branch { get; set; } = string.Empty;

        [Required(ErrorMessage = "O número da conta é obrigatório.")]
        [RegularExpression(@"^\d{7}-\d{1}$", ErrorMessage = "A conta deve possuir a máscara XXXXXXX-X.")]
        public string Account { get; set; } = string.Empty;
    }
}
