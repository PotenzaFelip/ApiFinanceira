using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiFinanceira.Application.ExternalServices
{
    public interface IAuthTokenProvider
    {
        Task<string?> GetAccessTokenAsync();
        void ClearAccessToken();
    }
}
