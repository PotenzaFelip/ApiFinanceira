using ApiFinanceira.Application.ExternalServices;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace ApiFinanceira.Infrastructure.HttpHandlers
{
    public class AuthHeaderHandler : DelegatingHandler
    {
        private readonly IServiceProvider _serviceProvider;

        public AuthHeaderHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {           
            using (var scope = _serviceProvider.CreateScope())
            {
                var complianceService = scope.ServiceProvider.GetRequiredService<IComplianceService>();
                string? accessToken = await complianceService.GetCachedAccessTokenAsync();

                if (!string.IsNullOrEmpty(accessToken))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                }

                var response = await base.SendAsync(request, cancellationToken);
               
                if (response.StatusCode == HttpStatusCode.Unauthorized && !request.Headers.Contains("X-Retry-Auth"))
                {                  
                    request.Headers.Add("X-Retry-Auth", "true");
                    complianceService.ClearCachedToken();
                    
                }

                return response;
            }
        }
    }
}