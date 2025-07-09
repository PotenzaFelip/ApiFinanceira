using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiFinanceira.Application.DTOs.Requests
{
    public class LoginRequest
    {
        [Required]
        public string Document { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
