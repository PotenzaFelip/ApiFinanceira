using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiFinanceira.Application.DTOs.Responses
{
    public class PagedTransacaoResponse
    {
        public IEnumerable<TransacaoResponse> Transactions { get; set; } = new List<TransacaoResponse>();
        public PaginationInfo Pagination { get; set; } = new PaginationInfo();
    }
}
