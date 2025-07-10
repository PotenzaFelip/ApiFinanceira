using Microsoft.Extensions.Options;
using System;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Refit; // Para ApiException e ApiResponse
using ApiFinanceira.Application.Clients;
using ApiFinanceira.Application.DTOs.Requests;
using ApiFinanceira.Application.DTOs.Responses;

namespace ApiFinanceira.Application.ExternalServices
{
    // A interface IComplianceService deve herdar de IAuthTokenProvider
    // para unificar os contratos e evitar métodos duplicados.
    public interface IComplianceService : IAuthTokenProvider
    {
        void ClearCachedToken();
        Task<string?> GetCachedAccessTokenAsync();
        Task<ComplianceResponse?> ValidaDocumentComplianceAsync(string document, string documentType);
        // Os métodos GetAccessTokenAsync e ClearAccessToken vêm de IAuthTokenProvider.
        // Remova quaisquer declarações antigas como GetCachedAccessTokenAsync ou ClearCachedToken aqui.
    }

    public class ComplianceService : IComplianceService
    {
        private readonly IComplianceAuthClient _authClient; // Cliente Refit para autenticação
        private readonly IComplianceValidationClient _validationClient; // Cliente Refit para validação
        private readonly ComplianceApiSettings _complianceApiSettings; // Apenas para credenciais
        private string? _cachedAccessToken;
        private DateTime _tokenExpiry;
        private readonly SemaphoreSlim _tokenGenerationLock = new SemaphoreSlim(1, 1); // Para garantir geração única do token

        public ComplianceService(
            IComplianceAuthClient authClient,
            IComplianceValidationClient validationClient,
            IOptions<ComplianceApiSettings> complianceApiSettings)
        {
            _authClient = authClient;
            _validationClient = validationClient;
            _complianceApiSettings = complianceApiSettings.Value;
        }

        // --- Implementação de IAuthTokenProvider ---

        // Este método é chamado pelo AuthHeaderHandler
        public async Task<string?> GetAccessTokenAsync()
        {
            // Verifica se o token expirou ou não existe
            if (string.IsNullOrEmpty(_cachedAccessToken) || _tokenExpiry <= DateTime.UtcNow)
            {
                await GenerateAndCacheAccessTokenAsync();
            }
            return _cachedAccessToken;
        }

        // Este método é chamado pelo AuthHeaderHandler para invalidar o token
        public void ClearAccessToken()
        {
            _cachedAccessToken = null;
            _tokenExpiry = DateTime.MinValue;
        }

        private async Task GenerateAndCacheAccessTokenAsync()
        {
            await _tokenGenerationLock.WaitAsync(); // Garante que apenas uma thread gere o token por vez
            try
            {
                // Dupla verificação para evitar geração redundante se o token foi gerado por outra thread
                if (!string.IsNullOrEmpty(_cachedAccessToken) && _tokenExpiry > DateTime.UtcNow)
                {
                    return;
                }

                // 1. Obter Auth Code usando Refit
                var authCodeRequest = new ComplianceAuthCodeRequest
                {
                    Email = _complianceApiSettings.AuthEmail,
                    Password = _complianceApiSettings.AuthPassword
                };

                ApiResponse<ComplianceAuthCodeResponse> authCodeResponse;
                try
                {
                    authCodeResponse = await _authClient.GetAuthCodeAsync(authCodeRequest);
                }
                catch (ApiException ex)
                {
                    Console.WriteLine($"[ComplianceService] Erro Refit ao obter AuthCode: Status {ex.StatusCode} - Detalhes: {ex.Content}");
                    throw new HttpRequestException($"Falha ao obter AuthCode da API de Compliance. Status: {ex.StatusCode}. Detalhes: {ex.Content}", ex);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ComplianceService] Erro geral ao obter AuthCode: {ex.Message}");
                    throw;
                }

                if (!authCodeResponse.IsSuccessStatusCode || authCodeResponse.Content?.Data == null || string.IsNullOrEmpty(authCodeResponse.Content.Data.AuthCode))
                {
                    var errorContent = authCodeResponse.Content != null ? JsonSerializer.Serialize(authCodeResponse.Content) : "No content";
                    throw new InvalidOperationException($"[ComplianceService] Resposta inválida ao obter AuthCode. Status: {authCodeResponse.StatusCode}. Detalhes: {errorContent}");
                }

                // 2. Obter Access Token usando Refit
                var tokenRequest = new ComplianceTokenRequest
                {
                    AuthCode = authCodeResponse.Content.Data.AuthCode
                };

                ApiResponse<ComplianceTokenResponse> tokenResponse;
                try
                {
                    tokenResponse = await _authClient.GetAccessTokenAsync(tokenRequest);
                }
                catch (ApiException ex)
                {
                    Console.WriteLine($"[ComplianceService] Erro Refit ao obter AccessToken: Status {ex.StatusCode} - Detalhes: {ex.Content}");
                    throw new HttpRequestException($"Falha ao obter AccessToken da API de Compliance. Status: {ex.StatusCode}. Detalhes: {ex.Content}", ex);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ComplianceService] Erro geral ao obter AccessToken: {ex.Message}");
                    throw;
                }

                if (!tokenResponse.IsSuccessStatusCode || tokenResponse.Content?.Data == null || string.IsNullOrEmpty(tokenResponse.Content.Data.AccessToken))
                {
                    var errorContent = tokenResponse.Content != null ? JsonSerializer.Serialize(tokenResponse.Content) : "No content";
                    throw new InvalidOperationException($"[ComplianceService] Resposta inválida ao obter AccessToken. Status: {tokenResponse.StatusCode}. Detalhes: {errorContent}");
                }

                // Armazena o token e a data de expiração (com um buffer de segurança)
                _cachedAccessToken = tokenResponse.Content.Data.AccessToken;
                _tokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.Content.Data.ExpiresIn - 60);
            }
            finally
            {
                _tokenGenerationLock.Release(); // Libera o lock
            }
        }

        // --- Lógica de Validação de Documentos ---
        public async Task<ComplianceResponse?> ValidaDocumentComplianceAsync(string document, string documentType)
        {
            // O AuthHeaderHandler, que usa GetAccessTokenAsync, já se encarregará de adicionar o token
            // antes que a requisição chegue à API externa.
            var request = new ComplianceRequest { Document = document };
            ApiResponse<ComplianceResponse> response;

            try
            {
                if (documentType.Equals("cpf", StringComparison.OrdinalIgnoreCase))
                {
                    response = await _validationClient.ValidateCpfAsync(request);
                }
                else if (documentType.Equals("cnpj", StringComparison.OrdinalIgnoreCase))
                {
                    response = await _validationClient.ValidateCnpjAsync(request);
                }
                else
                {
                    throw new ArgumentException("Tipo de documento inválido. Deve ser 'cpf' ou 'cnpj'.", nameof(documentType));
                }

                // Se chegar aqui, o status é 2xx. Erros 4xx/5xx seriam capturados por ApiException.
                var complianceApiResult = response.Content; // Refit já desserializa para response.Content

                if (complianceApiResult == null)
                {
                    return new ComplianceResponse { Error = "Resposta vazia ou inválida da API de Compliance.", Success = false };
                }

                // Sua lógica existente para processar a resposta da API de Compliance
                if (complianceApiResult.Success && complianceApiResult.Data != null)
                {
                    return new ComplianceResponse
                    {
                        Success = true,
                        Data = complianceApiResult.Data,
                        Message = complianceApiResult.Message
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
                        Message = $"API de Compliance retornou 200 OK com formato de dados desconhecido."
                    };
                }
            }
            catch (ApiException ex) // Exceção específica do Refit para erros HTTP (4xx, 5xx)
            {
                Console.WriteLine($"[ComplianceService] Refit API Exception (Validação): Status {ex.StatusCode}, Content: {ex.Content}");

                if (ex.StatusCode == HttpStatusCode.Unauthorized)
                {
                    ClearAccessToken(); // Força a reautenticação na próxima chamada
                    return new ComplianceResponse { Success = false, Error = "Autenticação falhou.", Message = $"Erro de autenticação (401) ao validar documento: {ex.Content}" };
                }

                return new ComplianceResponse
                {
                    Success = false,
                    Error = ex.Message,
                    Message = $"Erro na API de Compliance: Status {ex.StatusCode}. Detalhes: {ex.Content}"
                };
            }
            catch (Exception ex) // Outras exceções (problemas de rede, desserialização JSON se algo der muito errado, etc.)
            {
                Console.WriteLine($"[ComplianceService] Erro inesperado ao verificar compliance: {ex.Message}");
                return new ComplianceResponse { Success = false, Error = ex.Message, Message = $"Erro inesperado ao verificar compliance: {ex.Message}" };
            }
        }

        public void ClearCachedToken()
        {
            _cachedAccessToken = null;
            _tokenExpiry = DateTime.MinValue;
        }

        public async Task<string?> GetCachedAccessTokenAsync()
        {
            if (string.IsNullOrEmpty(_cachedAccessToken) || _tokenExpiry <= DateTime.UtcNow)
            {
                await GenerateAndCacheAccessTokenAsync();
            }
            return _cachedAccessToken;
        }
    }
}