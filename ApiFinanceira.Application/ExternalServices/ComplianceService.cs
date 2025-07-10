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
using System.Net.Http.Json;

namespace ApiFinanceira.Application.ExternalServices
{
    public class ComplianceService : IComplianceService
    {
        private readonly HttpClient _httpClient;
        private readonly ComplianceApiSettings _complianceApiSettings;
        private string? _cachedAccessToken;
        private DateTime _tokenExpiry;

        public ComplianceService(HttpClient httpClient, IOptions<ComplianceApiSettings> complianceApiSettings)
        {
            _httpClient = httpClient;
            _complianceApiSettings = complianceApiSettings.Value;
        }

        private async Task GenerateAndCacheAccessTokenAsync()
        {           
            var authCodeRequestUri = _complianceApiSettings.AuthCodeUrl;
            var authCodeRequestBody = new ComplianceAuthCodeRequest
            {
                Email = _complianceApiSettings.AuthEmail,
                Password = _complianceApiSettings.AuthPassword
            };

            var authCodeResponse = await _httpClient.PostAsJsonAsync(authCodeRequestUri, authCodeRequestBody);

            if (!authCodeResponse.IsSuccessStatusCode)
            {
                var errorContent = await authCodeResponse.Content.ReadAsStringAsync();               
                throw new HttpRequestException($"Falha ao obter AuthCode da API de Compliance. Status: {authCodeResponse.StatusCode}. Detalhes: {errorContent}");
            }

            var authCodeResult = await authCodeResponse.Content.ReadFromJsonAsync<ComplianceAuthCodeResponse>();
            if (authCodeResult?.Data == null || !authCodeResult.Success || string.IsNullOrEmpty(authCodeResult.Data.AuthCode))
            {
                throw new InvalidOperationException($"[ComplianceService] Resposta inválida ao obter AuthCode: {JsonSerializer.Serialize(authCodeResult)}");
            }
            
            var authTokenRequestUri = _complianceApiSettings.AuthTokenUrl;
            var tokenRequest = new ComplianceTokenRequest
            {
                AuthCode = authCodeResult.Data.AuthCode
            };

            var tokenResponse = await _httpClient.PostAsJsonAsync(authTokenRequestUri, tokenRequest);

            if (!tokenResponse.IsSuccessStatusCode)
            {
                var errorContent = await tokenResponse.Content.ReadAsStringAsync();                
                throw new HttpRequestException($"Falha ao obter AccessToken da API de Compliance. Status: {tokenResponse.StatusCode}. Detalhes: {errorContent}");
            }

            var tokenResult = await tokenResponse.Content.ReadFromJsonAsync<ComplianceTokenResponse>();
            if (tokenResult?.Data == null || !tokenResult.Success || string.IsNullOrEmpty(tokenResult.Data.AccessToken))
            {
                throw new InvalidOperationException($"[ComplianceService] Resposta inválida ao obter AccessToken: {JsonSerializer.Serialize(tokenResult)}");
            }

            _cachedAccessToken = tokenResult.Data.AccessToken;
            _tokenExpiry = DateTime.UtcNow.AddSeconds(tokenResult.Data.ExpiresIn - 60);            
        }


        public async Task<ComplianceResponse?> ValidaDocumentComplianceAsync(string document, string documentType)
        {
            if (string.IsNullOrEmpty(_cachedAccessToken) || _tokenExpiry <= DateTime.UtcNow)
            {
                await GenerateAndCacheAccessTokenAsync();
            }
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _cachedAccessToken);

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

                var complianceApiResult = JsonSerializer.Deserialize<ComplianceResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (complianceApiResult == null)
                {
                    return new ComplianceResponse { Error = "Resposta vazia ou inválida da API de Compliance.", Success = false };
                }

                if (response.IsSuccessStatusCode)
                {
                    if (complianceApiResult.Success && complianceApiResult.Data != null)
                    {

                        return new ComplianceResponse
                        {
                            Success = true,
                            Data = complianceApiResult.Data,
                        };
                    }
                    else if (!complianceApiResult.Success && !string.IsNullOrEmpty(complianceApiResult.Error))
                    {
                        return new ComplianceResponse
                        {
                            Success = false,
                            Error = complianceApiResult.Error,
                            Reason = complianceApiResult.Reason,
                            Message = $"Falha na validação do documento: {complianceApiResult.Error}"
                        };
                    }
                    else
                    {
                        return new ComplianceResponse
                        {
                            Success = false,
                            Error = "Formato de sucesso inesperado da API de Compliance.",
                            Message = $"API de Compliance retornou 200 OK com formato de dados desconhecido: {responseContent}"
                        };
                    }
                }
                else
                {
                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        _cachedAccessToken = null;
                        _tokenExpiry = DateTime.MinValue;
                        return new ComplianceResponse { Success = false, Error = complianceApiResult.Error ?? "Autenticação falhou.", Message = $"Erro de autenticação (401): {responseContent}" };
                    }

                    return new ComplianceResponse
                    {
                        Success = false,
                        Error = complianceApiResult.Error ?? $"Erro na API de Compliance: {response.StatusCode}",
                        Reason = complianceApiResult.Reason,
                        Message = $"API de Compliance retornou {response.StatusCode} - {response.ReasonPhrase}. Detalhes: {complianceApiResult.Error ?? responseContent}"
                    };
                }
            }
            catch (HttpRequestException ex)
            {                
                return new ComplianceResponse { Success = false, Error = ex.Message, Message = $"Falha de conexão com a API de Compliance: {ex.Message}" };
            }
            catch (JsonException ex)
            {               
                return new ComplianceResponse { Success = false, Error = ex.Message, Message = $"Resposta da API de Compliance não é um JSON válido. Erro: {ex.Message}" };
            }
            catch (Exception ex)
            {               
                return new ComplianceResponse { Success = false, Error = ex.Message, Message = $"Erro inesperado ao verificar compliance: {ex.Message}" };
            }
        }
    }
}