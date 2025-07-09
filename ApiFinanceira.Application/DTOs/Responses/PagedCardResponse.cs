using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiFinanceira.Application.DTOs.Responses
{
    public class PagedCardResponse
    {
        public IEnumerable<CartaoResponse> Cards { get; set; } = new List<CartaoResponse>();
        public PaginationInfo Pagination { get; set; } = new PaginationInfo();
    }
}
