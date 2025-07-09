using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ApiFinanceira.Application.ExternalServices
{
    public class ComplianceService : IComplianceService
    {
        private readonly HttpClient _httpClient;
        private readonly ComplianceApiSettings _complianceApiSettings; 
        private readonly string? _complianceApiAuthToken;
        
        public ComplianceService(HttpClient httpClient, IOptions<ComplianceApiSettings> complianceApiSettings, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _complianceApiSettings = complianceApiSettings.Value;
           
            _complianceApiAuthToken = configuration["ExternalApis:ComplianceApi:AuthToken"];
          
            if (!string.IsNullOrEmpty(_complianceApiAuthToken))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _complianceApiAuthToken);
            }
        }
        
        public async Task<ComplianceResponse?> ValidaDocumentComplianceAsync(string document, string documentType)
        {
            var request = new ComplianceRequest { Document = document };

            string requestUri;
            
            if (documentType.Equals("cpf", StringComparison.OrdinalIgnoreCase))
            {
                requestUri = _complianceApiSettings.BaseUrlCpf;
            }
            else if (documentType.Equals("cnpj", StringComparison.OrdinalIgnoreCase))
            {
                requestUri = _complianceApiSettings.BaseUrlCnpj;
            }
            else
            {
                throw new ArgumentException("Tipo de documento inválido. Deve ser 'cpf' ou 'cnpj'.", nameof(documentType));
            }

            try
            {                
                var response = await _httpClient.PostAsJsonAsync(requestUri, request);
                var responseContent = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"[ComplianceService] Status Code da Resposta ({requestUri}): {response.StatusCode}");
                Console.WriteLine($"[ComplianceService] Conteúdo Bruto da Resposta ({requestUri}): {responseContent}");

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    try
                    {
                        var errorResponse = JsonSerializer.Deserialize<ComplianceResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (errorResponse != null && !string.IsNullOrEmpty(errorResponse.Error) && errorResponse.Error.Contains("Missing authorization header"))
                        {
                            return new ComplianceResponse { Status = -1, Message = "Erro de autenticação: Token de autorização ausente ou inválido." };
                        }
                    }
                    catch (JsonException) {}
                    return new ComplianceResponse { Status = -1, Message = $"Erro de autenticação (401): {responseContent}" };
                }

                if (!response.IsSuccessStatusCode)
                {
                    try
                    {
                        var errorResponse = JsonSerializer.Deserialize<ComplianceResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (errorResponse != null && (!string.IsNullOrEmpty(errorResponse.Reason) || !string.IsNullOrEmpty(errorResponse.Message)))
                        {
                            return new ComplianceResponse { Status = errorResponse.Status, Message = errorResponse.Reason ?? errorResponse.Message ?? $"Erro desconhecido ({response.StatusCode})." };
                        }
                    }
                    catch (JsonException) {}
                    return new ComplianceResponse { Status = -1, Message = $"Erro na API de Compliance: {response.StatusCode} - {responseContent}" };
                }

                var complianceResult = await response.Content.ReadFromJsonAsync<ComplianceResponse>();
                if (complianceResult == null)
                {
                    return new ComplianceResponse { Status = -1, Message = "Resposta vazia ou inválida da API de Compliance." };
                }

                return complianceResult;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Erro de requisição HTTP ao chamar a API de Compliance ({requestUri}): {ex.Message}");
                return new ComplianceResponse { Status = -1, Message = $"Falha de conexão com a API de Compliance: {ex.Message}" };
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Erro de JSON ao desserializar a resposta da API de Compliance ({requestUri}): {ex.Message}. Conteúdo recebido:");
                return new ComplianceResponse { Status = -1, Message = $"Resposta da API de Compliance não é um JSON válido. Erro: {ex.Message}" };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Um erro inesperado ocorreu ao chamar a API de Compliance ({requestUri}): {ex.Message}");
                return new ComplianceResponse { Status = -1, Message = $"Erro inesperado ao verificar compliance: {ex.Message}" };
            }
        }
    }
}