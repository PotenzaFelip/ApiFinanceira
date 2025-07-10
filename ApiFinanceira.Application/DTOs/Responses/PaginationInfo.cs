using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiFinanceira.Application.DTOs.Responses
{
    public class PaginationInfo
    {
        public int ItemsPerPage { get; set; }
        public int CurrentPage { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages
        {
            get
            {
                if (TotalItems == 0)
                {
                    return 1;
                }
                return (int)Math.Ceiling((double)TotalItems / ItemsPerPage);
            }
        }
    }
}

