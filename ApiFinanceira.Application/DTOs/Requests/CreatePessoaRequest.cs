using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ApiFinanceira.Application.DTOs.Requests
{
    public class CreatePessoaRequest
    {
        [Required(ErrorMessage = "O nome é obrigatório.")]
        [StringLength(255, ErrorMessage = "O nome não pode exceder 255 caracteres.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "O documento é obrigatório.")]
        [StringLength(14, MinimumLength = 11, ErrorMessage = "O documento deve ter entre 11 e 14 caracteres.")]
        public string Document { get; set; } = string.Empty;

        [Required(ErrorMessage = "A senha é obrigatória.")]
        [MinLength(6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres.")]
        public string Password { get; set; } = string.Empty;
    }
}